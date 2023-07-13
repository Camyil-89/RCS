using Microsoft.VisualBasic;
using RCS.Net.Firewall;
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
		Client = 1,
		Server = 2,
	}
	public class RCSTCPConnection
	{
		private List<Packets.BasePacket> _Packets = new List<Packets.BasePacket>();
		private Dictionary<Guid, WaitInfoPacket> WaitPackets = new Dictionary<Guid, WaitInfoPacket>();
		private byte[] PublicKey;
		private byte[] PrivateKey;
		private RCSMagma Magma = new RCSMagma();
		private bool BlockRX = false;
		private object _lock1 = new object();
		private bool BlockTX = false;
		private bool InitialConnect = false;
		private int BufferSize { get; set; } = 1024 * 512; // 1kb
		private IFirewall Firewall { get; set; }
		public int TimeoutWaitPacket { get; set; } = 10_000;
		public NetworkStream NetworkStream { get; set; }

		public ModeConnection Mode { get; set; }
		public byte[] Buffer;
		public ConnectionStatistics Statistics = new ConnectionStatistics();
		public delegate void CallbackReceive(BasePacket packet);
		public event CallbackReceive CallbackReceiveEvent;

		public delegate void CallbackUpdateKeys();
		public event CallbackUpdateKeys CallbackUpdateKeysEvent;
		public RCSTCPConnection(NetworkStream stream, byte[] public_key, byte[] private_key, IFirewall firewall)
		{
			NetworkStream = stream;
			NetworkStream.ReadTimeout = 700;
			NetworkStream.WriteTimeout = 700;
			PublicKey = public_key;
			PrivateKey = private_key;
			if (PublicKey == null)
				Mode = ModeConnection.Server;
			else
				Mode = ModeConnection.Client;
			Firewall = firewall;
		}
		public void Abort()
		{
			NetworkStream = null;
		}
		public async Task Send(BasePacket packet)
		{
			byte[] raw = new byte[0];
			raw = packet.Raw(Magma);
			await WriteStream(raw);
			if (packet.Type == PacketType.ValidatingCertificate)
				Console.WriteLine($"TX ({raw.Length}) - {packet.UID};{packet.Type}; {DateTime.Now}.{DateTime.Now.Millisecond}");
		}
		public void Start()
		{
			Buffer = new byte[BufferSize];
			BlockTX = true;
			if (Mode == ModeConnection.Client)
			{
				BlockRX = true;
			}
			Thread thread = new Thread(() => { RXHandler(); });
			thread.IsBackground = true;
			thread.Start();
			if (Mode == ModeConnection.Client)
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

				if (InitialConnect == false)
				{
					Packet packet = new Packet();
					packet.Type = PacketType.InitialConnect;
					packet.Data = Magma;
					WriteStream(packet.Raw(PublicKey));
					var answer = new Packet().FromRaw(WaitReadNetworkStream(), Magma);

					InitialConnect = true;
				}
				else
				{
					var magma = new RCSMagma();
					Packet packet = new Packet();
					packet.Type = PacketType.InitialConnect;
					packet.Data = magma;
					WriteStream(packet.Raw(Magma));
					Magma = magma;
					var answer = new Packet().FromRaw(WaitReadNetworkStream(), Magma);
				}

				BlockRX = false;
				BlockTX = false;
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
				await NetworkStream.WriteAsync(result);
				await NetworkStream.FlushAsync();
				Statistics.TXBytes += result.LongLength;
			}
			catch (Exception ex) { Console.WriteLine(ex); }
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
						if (Mode == ModeConnection.Server && InitialConnect == false)
						{
							try
							{
								packet = new Packet().FromRaw(task.Result, PrivateKey);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
								Abort();
							}
						}
						else
							packet = new Packet().FromRaw(task.Result, Magma);
						if (Firewall.ValidatePacket(packet) == false)
						{
							packet.Type = PacketType.FirewallBLock;
							packet.Answer(packet);
							Console.WriteLine($"[CONNECTION FIREWALL] ValidatePacket");
							return;
						}
						packet.CallbackAnswerEvent += Packet_CallbackAnswerEvent;
						if (packet.Type == PacketType.ValidatingCertificate)
							Console.WriteLine($"RX - {packet.UID};{packet.Type}; {DateTime.Now}.{DateTime.Now.Millisecond}");
						if (packet.Type == PacketType.InitialConnect)
						{
							BlockTX = true;
							try
							{
								Magma = (RCSMagma)packet.Data;
								packet.Type = PacketType.ConfirmConnect;
								packet.Answer(packet);
								InitialConnect = true;
								Console.WriteLine($"[CONNECTION] InitialConnect");
							}
							catch (Exception ex) { BlockTX = false; Console.WriteLine(ex); }

						}
						else if (packet.Type == PacketType.ConfirmConnect)
						{
							Console.WriteLine($"[CONNECTION] ConfirmConnect");
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
		private BasePacket WaitPacketReadNetworkStream(PacketType type, byte[] public_key)
		{
			while (true)
			{
				try
				{
					var data = ReadNetworkStream();
					if (data == null)
						continue;
					var packet = new Packet().FromRaw(data, public_key);
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

			if (Firewall.ValidateHeader(headerBuffer) == false)
			{
				Console.WriteLine($"[CONNECTION FIREWALL] ValidateHeader");
				// очищаем поток
				while (totalBytesRead < packetLength)
				{
					var buffer_size = Buffer.Length <= packetLength - totalBytesRead ? Buffer.Length : packetLength - totalBytesRead;
					bytesRead = await NetworkStream.ReadAsync(Buffer, 0, buffer_size).ConfigureAwait(false);
					totalBytesRead += bytesRead;
					Statistics.RXBytes += bytesRead;
				}
				return null;
			}
			using (MemoryStream ms = new MemoryStream())
			{
				while (totalBytesRead < packetLength)
				{
					var buffer_size = Buffer.Length <= packetLength - totalBytesRead ? Buffer.Length : packetLength - totalBytesRead;
					bytesRead = await NetworkStream.ReadAsync(Buffer, 0, buffer_size).ConfigureAwait(false);
					totalBytesRead += bytesRead;
					ms.Write(Buffer, 0, bytesRead);
					Statistics.RXBytes += bytesRead;
				}
				if (totalBytesRead < packetLength)
				{
					Console.WriteLine($"ERROR READ");
					return null;
				}
				var data = ms.ToArray();
				if (Firewall.Validate(data))
				{
					return data;
				}
				Console.WriteLine($"[CONNECTION FIREWALL] ValidateHeader");
				return null;
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
