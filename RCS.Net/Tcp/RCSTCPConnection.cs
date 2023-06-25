using Microsoft.VisualBasic;
using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Tcp
{
	public class RCSTCPConnection
	{
		NetworkStream NetworkStream { get; set; }
		public int BufferSize { get; set; } = 1024; // 1kb
		private List<Packets.BasePacket> _Packets = new List<Packets.BasePacket>();

		Dictionary<Guid, BasePacket> WaitPackets = new Dictionary<Guid, BasePacket>();
		public int TimeoutWaitPacket { get; set; } = 3000;

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
		public void Start()
		{
			Task.Run(TXHandler);
			Task.Run(RXHandler);
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
								var raw = _Packets.First().Raw();
								NetworkStream.Write(raw);
								_Packets.RemoveAt(0);
							} while (_Packets.Count > 0);
						} catch { }
					}
					Thread.Sleep(1);
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			Console.WriteLine("END [TXHandler]");
		}
		private void RXHandler()
		{
			Console.WriteLine("START [RXHandler]");

			int bytesRead = 0;
			byte[] buffer = new byte[BufferSize];
			while (NetworkStream != null && NetworkStream.CanRead)
			{
				Packets.BasePacket packet = null;
				try
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						do
						{
							bytesRead = NetworkStream.Read(buffer, 0, buffer.Length);
							memoryStream.Write(buffer, 0, bytesRead);
						}
						while (bytesRead == buffer.Length);

						packet = new Packet().FromRaw(memoryStream.GetBuffer());
						packet.CallbackAnswerEvent += Packet_CallbackAnswerEvent;
						if (WaitPackets.ContainsKey(packet.UID))
						{
							WaitPackets[packet.UID] = packet;
						}
						else
							CallbackReceiveEvent?.Invoke(packet);
					}
				}
				catch (Exception ex) { }
			}
			Console.WriteLine("END [RXHandler]");
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
