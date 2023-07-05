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
	public class ConnectionStatistics : Base.ViewModel.BaseViewModel
	{

		#region TXBytes: Description
		/// <summary>Description</summary>
		private long _TXBytes;
		/// <summary>Description</summary>
		public long TXBytes
		{
			get => _TXBytes; set
			{
				Set(ref _TXBytes, value);
				TXBytesScale = RoundByte(TXBytes);
			}
		}
		#endregion

		#region RXBytes: Description
		/// <summary>Description</summary>
		private long _RXBytes;
		/// <summary>Description</summary>
		public long RXBytes
		{
			get => _RXBytes; set
			{
				Set(ref _RXBytes, value);
				RXBytesScale = RoundByte(RXBytes);
			}
		}
		#endregion


		#region TimeConnect: Description
		/// <summary>Description</summary>
		private DateTime _TimeConnect = DateTime.Now;
		/// <summary>Description</summary>
		public DateTime TimeConnect { get => _TimeConnect; set => Set(ref _TimeConnect, value); }
		#endregion


		#region TXBytesScale: Description
		/// <summary>Description</summary>
		private string _TXBytesScale;
		/// <summary>Description</summary>
		public string TXBytesScale { get => _TXBytesScale; set => Set(ref _TXBytesScale, value); }
		#endregion


		#region RXBytesScale: Description
		/// <summary>Description</summary>
		private string _RXBytesScale;
		/// <summary>Description</summary>
		public string RXBytesScale { get => _RXBytesScale; set => Set(ref _RXBytesScale, value); }
		#endregion

		public static string RoundByte(long Bytes)
		{
			if (Bytes < Math.Pow(1024, 1)) return $"{Bytes} Б";
			else if (Bytes < Math.Pow(1024, 2)) return $"{Math.Round((float)Bytes / 1024, 2)} Кбайт";
			else if (Bytes < Math.Pow(1024, 3)) return $"{Math.Round((float)Bytes / Math.Pow(1024, 2), 2)} Мбайт";
			else if (Bytes < Math.Pow(1024, 4)) return $"{Math.Round((float)Bytes / Math.Pow(1024, 3), 2)} Гбайт";
			return "";
		}
	}
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
		public Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();
	}
	public enum ModeConnection : byte
	{
		RequestAnswer = 1,
		Unlimited = 2,
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
		private int BufferSize { get; set; } = 1024 * 512; // 1kb
		public int TimeoutWaitPacket { get; set; } = 10_000;

		public ModeConnection Mode { get; set; } = ModeConnection.Unlimited;
		public byte[] Buffer;
		public ConnectionStatistics Statistics = new ConnectionStatistics();
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
			byte[] raw = new byte[0];
			raw = packet.Raw(PublicKey);
			await WriteStream(raw);
			if (packet.Type == PacketType.ValidatingCertificate)
				Console.WriteLine($"TX ({raw.Length}) - {packet.UID};{packet.Type}; {DateTime.Now}.{DateTime.Now.Millisecond}");
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
				UpdateKeys(false);
			}
		}
		public void UpdateKeys(bool delay = true)
		{
			Console.WriteLine($"[CONNECTION] UpdateKeys");
			try
			{
				BlockRX = true;
				BlockTX = true;
				if (delay)
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
				Statistics.TXBytes += result.LongLength;
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
							if (packet.Type == PacketType.RSTStopwatch)
							{
								Console.WriteLine($"[RST] {packet.Type};{packet.UID}");
								WaitPackets[packet.UID].Stopwatch.Restart();
							}
							else
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
				Console.WriteLine($"[BAD HEAD]");
				return null;
			}
			int packetLength = BitConverter.ToInt32(headerBuffer, 0);
			int totalBytesRead = 0;
			using (MemoryStream ms = new MemoryStream())
			{
				while (totalBytesRead < packetLength)
				{
					var buffer_size = Buffer.Length <= packetLength - totalBytesRead ? Buffer.Length: packetLength - totalBytesRead;
					//Console.WriteLine($">{bytesRead}\\{packetLength} {buffer_size} {DateTime.Now}.{DateTime.Now.Millisecond}");
					bytesRead = await NetworkStream.ReadAsync(Buffer, 0, buffer_size).ConfigureAwait(false);
					totalBytesRead += bytesRead;
					ms.Write(Buffer, 0, bytesRead);
					Statistics.RXBytes += bytesRead;
					if (Mode == ModeConnection.RequestAnswer)
					{
						foreach (var i in WaitPackets)
						{
							i.Value.Stopwatch.Restart();
						}
					}
					//Console.WriteLine($"<{ms.Length}\\{packetLength} {DateTime.Now}.{DateTime.Now.Millisecond}");
				}
				if (totalBytesRead < packetLength)
				{
					Console.WriteLine($"ERROR READ");
					return null;
				}
				//Console.WriteLine($"<{ms.Length}\\{packetLength} {DateTime.Now}.{DateTime.Now.Millisecond}");
				//byte[] data = new byte[packetLength];
				//ms.Position = 0;
				//ms.Read(data,0, packetLength);
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
			var wait_info_packet = new WaitInfoPacket() { Timeout = TimeoutWaitPacket };
			WaitPackets.Add(packet.UID, wait_info_packet);
			Send(packet);
			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				while (wait_info_packet.Stopwatch.ElapsedMilliseconds < TimeoutWaitPacket)
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
