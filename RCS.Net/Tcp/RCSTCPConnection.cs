using Microsoft.VisualBasic;
using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Tcp
{
	public class RCSTCPConnection
	{
		private NetworkStream NetworkStream { get; set; }
		private List<Packets.BasePacket> _Packets = new List<Packets.BasePacket>();
		private Dictionary<Guid, BasePacket> WaitPackets = new Dictionary<Guid, BasePacket>();
		private RSAParameters PrivateKey = new RSAParameters();
		private RSAParameters PublicKey = new RSAParameters();
		private bool BlockRX = false;


		private int BufferSize { get; set; } = 1024; // 1kb
		public int TimeoutWaitPacket { get; set; } = 3000;

		public byte[] Buffer;

		public delegate void CallbackReceive(BasePacket packet);
		public event CallbackReceive CallbackReceiveEvent;
		public RCSTCPConnection(NetworkStream stream)
		{
			NetworkStream = stream;

		}
		public void Send(BasePacket packet)
		{
			_Packets.Add(packet);
		}
		public void Start(bool RSA_initial)
		{
			BufferSize = 1024;
			Buffer = new byte[BufferSize];
			if (RSA_initial)
			{
				BlockRX = true;
			}
			Task.Run(TXHandler);
			Task.Run(RXHandler);

			if (RSA_initial)
			{
				try
				{
					var rsa = new RSACryptoServiceProvider(384); //384, 512, 1024, 2048, 4096.
					Packet packet = new Packet();
					packet.Data = new SRSAParametrs(rsa.ExportParameters(false)) { SizeKey = 384};
					packet.Type = PacketType.RSAGetKeys;
					Send(packet);
					var b_packet = new Packet().FromRaw(ReadNetworkStream(), rsa.ExportParameters(true));
					PublicKey = ((SRSAParametrs[])b_packet.Data)[0].ToRSAParameters();
					PrivateKey = ((SRSAParametrs[])b_packet.Data)[1].ToRSAParameters();
					BlockRX = false;
				}
				catch (Exception ex) { Console.WriteLine(ex); throw new Exception(ex.Message); }
			}
		}
		private void TXHandler()
		{
			Console.WriteLine("START [TXHandler]");
			try
			{
				while (NetworkStream.CanWrite)
				{
					if (_Packets.Count > 0)
					{
						try
						{
							do
							{
								//Console.WriteLine($"tx {_Packets.Count}");
								var raw = _Packets.First().Raw(PublicKey);
								WriteStream(raw);
								_Packets.RemoveAt(0);
							} while (_Packets.Count > 0);
						}
						catch (Exception ex) { Console.WriteLine(ex); }
					}
					Thread.Sleep(1);
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			Console.WriteLine("END [TXHandler]");
		}
		private void WriteStream(byte[] data)
		{
			NetworkStream.Write(data);
		}
		private void RXHandler()
		{
			Console.WriteLine("START [RXHandler]");

			int bytesRead = 0;
			byte[] buffer = new byte[BufferSize];
			while (NetworkStream != null && NetworkStream.CanRead)
			{
				Packets.BasePacket packet = null;
				if (BlockRX == true)
					continue;
				try
				{
					var data = ReadNetworkStream();
					//Console.WriteLine($"rx: {data.Length};");
					packet = new Packet().FromRaw(data, PrivateKey);
					packet.CallbackAnswerEvent += Packet_CallbackAnswerEvent;
					if (WaitPackets.ContainsKey(packet.UID))
					{
						WaitPackets[packet.UID] = packet;
					}
					else if (packet.Type == PacketType.RSAGetKeys)
					{
						var par = ((SRSAParametrs)packet.Data);
						RSAParameters rsap = par.ToRSAParameters();
						Packet packet1 = new Packet();
						var rsa = new RSACryptoServiceProvider(par.SizeKey);
						packet1.Data = new SRSAParametrs[2]
						{
							new SRSAParametrs(rsa.ExportParameters(false)),
							new SRSAParametrs(rsa.ExportParameters(true))
						};
						WriteStream(packet1.Raw(rsap));

						PublicKey = rsa.ExportParameters(false);
						PrivateKey = rsa.ExportParameters(true);
					}
					else
						CallbackReceiveEvent?.Invoke(packet);
				}
				catch (Exception ex) { Console.WriteLine(ex); }
			}
			Console.WriteLine("END [RXHandler]");
		}
		private byte[] ReadNetworkStream()
		{
			int bytesRead = 0;
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
				Thread.Sleep(1);
			}
			throw new Exception("Timeout wait packet");
		}
		private void Packet_CallbackAnswerEvent(BasePacket packet)
		{
			Console.WriteLine($"[CONNECTION] Answer");
			Send(packet);
		}
	}
}
