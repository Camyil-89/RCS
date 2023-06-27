using Microsoft.VisualBasic;
using RCS.Net.Packets;
using System;
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
	public class RCSTCPConnection
	{
		private NetworkStream NetworkStream { get; set; }
		private List<Packets.BasePacket> _Packets = new List<Packets.BasePacket>();
		private Dictionary<Guid, BasePacket> WaitPackets = new Dictionary<Guid, BasePacket>();
		private RSAParameters PrivateKey = new RSAParameters();
		private RSAParameters PublicKey = new RSAParameters();
		private bool BlockRX = false;
		private bool BlockTX = false;
		private AutoResetEvent eventWaitTXHandle = new AutoResetEvent(false);
		private int BufferSize { get; set; } = 1024 * 100; // 1kb
		public int TimeoutWaitPacket { get; set; } = 3000;

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
		public void Send(BasePacket packet)
		{
			eventWaitTXHandle.Set();
			_Packets.Add(packet);
			if (BlockTX)
				return;
			try
			{
				do
				{
					var raw = _Packets.First().Raw(PublicKey);
					WriteStream(raw);
					_Packets.RemoveAt(0);
				} while (_Packets.Count > 0 && BlockTX == false);
			}
			catch (Exception ex) { Console.WriteLine(ex); }
		}
		public void Start(bool RSA_initial)
		{
			Buffer = new byte[BufferSize];
			BlockTX = true;
			if (RSA_initial)
			{
				BlockRX = true;
			}
			Task.Run(RXHandler);

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
				WriteStream(packet.Raw(PublicKey));
				var b_packet = WaitPacketReadNetworkStream(PacketType.RSAGetKeys, rsa.ExportParameters(true));
				PublicKey = ((SRSAParametrs[])b_packet.Data)[0].ToRSAParameters();
				PrivateKey = ((SRSAParametrs[])b_packet.Data)[1].ToRSAParameters();
				packet = new Packet() { Type = PacketType.RSAConfirm };
				WriteStream(packet.Raw(PublicKey));
				WaitPacketReadNetworkStream(PacketType.RSAConfirm, PrivateKey);
				BlockRX = false;
				BlockTX = false;
				Console.WriteLine($"[CONNECTION] UpdateKeys CONFIRM");
				CallbackUpdateKeysEvent?.Invoke();
			}
			catch (Exception ex) { Console.WriteLine(ex); throw new Exception(ex.Message); }
		}
		private void WriteStream(byte[] data)
		{
			NetworkStream.Write(data);
		}
		private void RXHandler()
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
					using (MemoryStream memoryStream = new MemoryStream())
					{
						do
						{
							bytesRead = NetworkStream.Read(Buffer, 0, Buffer.Length);
							memoryStream.Write(Buffer, 0, bytesRead);
						}
						while (bytesRead == Buffer.Length);
						packet = new Packet().FromRaw(memoryStream.ToArray(), PrivateKey);
					}
					packet.CallbackAnswerEvent += Packet_CallbackAnswerEvent;
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
							WriteStream(packet1.Raw(rsap));

							PublicKey = rsa.ExportParameters(false);
							PrivateKey = rsa.ExportParameters(true);
							Console.WriteLine($"[CONNECTION] CreateKeys");
						}
						catch (Exception ex) { BlockTX = false; Console.WriteLine(ex); }

					}
					else if (packet.Type == PacketType.RSAConfirm)
					{
						Console.WriteLine($"[CONNECTION] RSAConfirm");
						WriteStream(packet.Raw(PublicKey));
						BlockTX = false;
					}
					else if (WaitPackets.ContainsKey(packet.UID))
					{
						WaitPackets[packet.UID] = packet;
					}
					else
						CallbackReceiveEvent?.Invoke(packet);
				}
				catch (Exception ex) { Thread.Sleep(1); }
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
				} catch (Exception ex) { }
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
		private byte[] ReadNetworkStream()
		{
			int bytesRead = 0;
			if (NetworkStream.DataAvailable == false)
				return null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				do
				{
					bytesRead = NetworkStream.Read(Buffer, 0, Buffer.Length);
					memoryStream.Write(Buffer, 0, bytesRead);
				}
				while (bytesRead == Buffer.Length);
				return memoryStream.ToArray();
			}
		}
		public BasePacket SendAndWait(BasePacket packet)
		{
			WaitPackets.Add(packet.UID, null);
			Stopwatch stopwatch = Stopwatch.StartNew();
			Send(packet);
			while (stopwatch.ElapsedMilliseconds < TimeoutWaitPacket)
			{
				if (WaitPackets[packet.UID] != null)
				{
					var x = WaitPackets[packet.UID];
					WaitPackets.Remove(packet.UID);
					return x;
				}
				//Thread.Sleep(1);
			}
			throw new TimeoutException("Timeout wait packet");
		}
		private void Packet_CallbackAnswerEvent(BasePacket packet)
		{
			//Console.WriteLine($"[CONNECTION] Answer {packet}");
			Send(packet);
		}
	}
}
