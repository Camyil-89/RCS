using Microsoft.VisualBasic;
using RCS.Net.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RCS.Net.Tcp
{
	public class ExceptionTimeout : Exception
	{
		public ExceptionTimeout(string message)
		: base(message)
		{
		}

		public ExceptionTimeout(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
	public class WaitInfoPacket
	{
		public DateTime Time { get; set; } = DateTime.Now;
		public int Timeout { get; set; }
		public BasePacket Packet { get; set; } = null;
	}
	public class RCSTCPConnection
	{
		private NetworkStream NetworkStream { get; set; }
		private List<Packets.BasePacket> _Packets = new List<Packets.BasePacket>();
		private Dictionary<Guid, WaitInfoPacket> WaitPackets = new Dictionary<Guid, WaitInfoPacket>();
		private RSAParameters PrivateKey = new RSAParameters();
		private RSAParameters PublicKey = new RSAParameters();
		private bool BlockRX = false;
		private object _lock1 = new object();
		private SemaphoreSlim semaphoreSlim1 = new SemaphoreSlim(1, 1);
		private bool BlockTX = false;
		private int BufferSize { get; set; } = 1024 * 32; // 1kb
		public int TimeoutWaitPacket { get; set; } = 5000;

		public byte[] Buffer;

		public delegate void CallbackReceive(BasePacket packet);
		public event CallbackReceive CallbackReceiveEvent;

		public delegate void CallbackUpdateKeys();
		public event CallbackUpdateKeys CallbackUpdateKeysEvent;
		public RCSTCPConnection(NetworkStream stream)
		{
			NetworkStream = stream;
			NetworkStream.ReadTimeout = 700;
			NetworkStream.WriteTimeout = 700;
		}
		public void Abort()
		{
			NetworkStream = null;
		}
		public async Task Send(BasePacket packet)
		{
			byte[] raw;
			raw = packet.Raw(PublicKey);
			if (packet.Type == PacketType.ValidatingCertificate)
				Console.WriteLine($"TX - {packet.UID};{packet.Type}; {DateTime.Now}.{DateTime.Now.Millisecond}");
			await WriteStream(raw);
			//_Packets.Add(packet);
			//if (BlockTX)
			//	return;
			//try
			//{
			//	do
			//	{
			//		byte[] raw;
			//		raw = _Packets.First().Raw(PublicKey);
			//		Console.WriteLine($"TX: {_Packets.First().UID};{_Packets.First().Type} {DateTime.Now}.{DateTime.Now.Millisecond}");
			//		WriteStream(raw);
			//		_Packets.RemoveAt(0);
			//	} while (_Packets.Count > 0 && BlockTX == false);
			//}
			//catch (Exception ex) { Console.WriteLine(ex); }
		}
		public void Start(bool RSA_initial)
		{
			Buffer = new byte[BufferSize];
			BlockTX = true;
			if (RSA_initial)
			{
				BlockRX = true;
			}
			Thread thread = new Thread(() => { RXHandler(); });
			thread.IsBackground = true;
			thread.Start();
			if (RSA_initial)
			{
				UpdateKeys();
			}
		}
		public void UpdateKeys()
		{
			Console.WriteLine($"[CONNECTION] UpdateKeys");
			try
			{
				BlockRX = true;
				BlockTX = true;
				Thread.Sleep(NetworkStream.ReadTimeout + 100);
				var rsa = new RSACryptoServiceProvider(2048); //384, 512, 1024, 2048, 4096.
				Packet packet = new Packet();
				packet.Data = new SRSAParametrs(rsa.ExportParameters(false)) { SizeKey = 2048 };
				packet.Type = PacketType.RSAGetKeys;
				WriteStream(packet.Raw(PublicKey)).Wait();
				var b_packet = WaitPacketReadNetworkStream(PacketType.RSAGetKeys, rsa.ExportParameters(true));
				PublicKey = ((SRSAParametrs[])b_packet.Data)[0].ToRSAParameters();
				PrivateKey = ((SRSAParametrs[])b_packet.Data)[1].ToRSAParameters();
				packet = new Packet() { Type = PacketType.RSAConfirm };
				WriteStream(packet.Raw(PublicKey)).Wait();
				WaitPacketReadNetworkStream(PacketType.RSAConfirm, PrivateKey);
				BlockRX = false;
				BlockTX = false;
				Console.WriteLine($"[CONNECTION] UpdateKeys CONFIRM");
				CallbackUpdateKeysEvent?.Invoke();
			}
			catch (Exception ex) { Console.WriteLine(ex); throw new Exception(ex.Message); }
		}
		private async Task WriteStream(byte[] data)
		{
			int length = data.Length;
			byte[] lengthBytes = BitConverter.GetBytes(length);

			byte[] result = new byte[length + sizeof(int)];
			lengthBytes.CopyTo(result, 0);
			data.CopyTo(result, sizeof(int));
			try
			{
				//await semaphoreSlim1.WaitAsync();

				await NetworkStream.WriteAsync(result);
				NetworkStream.Flush();
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			finally
			{
				//semaphoreSlim1.Release();
			}
		}
		private async Task RXHandler()
		{
			Console.WriteLine("START [RXHandler]");

			int bytesRead = 0;
			while (NetworkStream != null && NetworkStream.CanWrite && NetworkStream.CanRead)
			{
				Packets.BasePacket packet = null;
				if (BlockRX == true)
					continue;
				try
				{
					await _Read().ContinueWith(async task =>
					{
						packet = new Packet().FromRaw(task.Result, PrivateKey);
						packet.CallbackAnswerEvent += Packet_CallbackAnswerEvent;
						if (packet.Type == PacketType.ValidatingCertificate)
							Console.WriteLine($"RX - {packet.UID};{packet.Type}; {DateTime.Now}.{DateTime.Now.Millisecond}");
						if (packet.Type == PacketType.RSAGetKeys)
						{
							BlockTX = true;
							try
							{
								var par = ((SRSAParametrs)packet.Data);
								RSAParameters rsap = par.ToRSAParameters();
								Packet packet1 = new Packet();
								packet1.Type = PacketType.RSAGetKeys;
								var rsa = new RSACryptoServiceProvider(par.SizeKey);
								packet1.Data = new SRSAParametrs[2]
								{
							new SRSAParametrs(rsa.ExportParameters(false)),
							new SRSAParametrs(rsa.ExportParameters(true))
								};
								await WriteStream(packet1.Raw(rsap));

								PublicKey = rsa.ExportParameters(false);
								PrivateKey = rsa.ExportParameters(true);
								Console.WriteLine($"[CONNECTION] CreateKeys");
							}
							catch (Exception ex) { BlockTX = false; Console.WriteLine(ex); }

						}
						else if (packet.Type == PacketType.RSAConfirm)
						{
							Console.WriteLine($"[CONNECTION] RSAConfirm");
							await WriteStream(packet.Raw(PublicKey));
							BlockTX = false;
						}
						else if (WaitPackets.ContainsKey(packet.UID))
						{
							WaitPackets[packet.UID].Packet = packet;
						}
						else
						{
							Task.Run(() => { CallbackReceiveEvent?.Invoke(packet); });
						}
					});
				}
				catch (Exception ex) { Console.WriteLine(ex); Thread.Sleep(25); }
			}
			Console.WriteLine("END [RXHandler]");
		}
		private BasePacket WaitPacketReadNetworkStream(PacketType type, RSAParameters parameters)
		{
			while (true)
			{
				try
				{
					var data = ReadNetworkStream();
					if (data == null)
						continue;
					var packet = new Packet().FromRaw(data, parameters);
					if (packet.Type == type)
						return packet;
				}
				catch (Exception ex) { }
			}
		}
		private byte[] WaitReadNetworkStream()
		{
			while (true)
			{
				var data = ReadNetworkStream();
				if (data != null)
					return data;
			}
		}
		private async Task<byte[]> _Read()
		{
			int headerSize = sizeof(int);
			byte[] headerBuffer = new byte[headerSize];
			int bytesRead = await NetworkStream.ReadAsync(headerBuffer, 0, headerSize).ConfigureAwait(false);

			if (bytesRead < headerSize)
			{
				return null;
			}
			int packetLength = BitConverter.ToInt32(headerBuffer, 0);
			int totalBytesRead = 0;
			using (MemoryStream ms = new MemoryStream())
			{
				while (totalBytesRead < packetLength)
				{
					//Console.WriteLine($">{bytesRead}\\{packetLength} {DateTime.Now}.{DateTime.Now.Millisecond}");
					bytesRead = await NetworkStream.ReadAsync(Buffer, 0, Buffer.Length).ConfigureAwait(false);
					totalBytesRead += bytesRead;
					ms.Write(Buffer, 0, bytesRead);
					//Console.WriteLine($"<{bytesRead}\\{packetLength} {DateTime.Now}.{DateTime.Now.Millisecond}");
				}
				if (totalBytesRead < packetLength)
				{
					Console.WriteLine($"ERROR READ");
					return null;
				}
				return ms.ToArray();
			}
		}
		private byte[] ReadNetworkStream()
		{

			if (NetworkStream.DataAvailable == false)
				return null;
			return _Read().Result;
		}
		public BasePacket SendAndWait(BasePacket packet)
		{
			WaitPackets.Add(packet.UID, new WaitInfoPacket() { Timeout = TimeoutWaitPacket });
			Send(packet);
			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				while (stopwatch.ElapsedMilliseconds < TimeoutWaitPacket)
				{
					if (WaitPackets[packet.UID].Packet != null)
					{
						var x = WaitPackets[packet.UID].Packet;
						WaitPackets.Remove(packet.UID);
						return x;
					}
				}
			}
			catch (Exception ex) { WaitPackets.Remove(packet.UID); Console.WriteLine(ex); }
			throw new TimeoutException("Timeout wait packet");
		}
		private void Packet_CallbackAnswerEvent(BasePacket packet)
		{
			if (packet.Type == PacketType.ValidatingCertificate)
				Console.WriteLine($"[CONNECTION] Answer {packet.UID}; {packet.Type} {DateTime.Now}.{DateTime.Now.Millisecond}");
			Send(packet);
		}
	}
}
