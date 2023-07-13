using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RCS.Net.Firewall
{
	public class SvarogClientInfo
	{
		public DateTime Time { get; set; } = DateTime.Now;
	}
	public class SvarogFirewall : IFirewall
	{
		private Dictionary<string, List<SvarogClientInfo>> Clients { get; set; } = new Dictionary<string, List<SvarogClientInfo>>();
		public Dictionary<string, DateTime> BannedIP { get; set; } = new Dictionary<string, DateTime>();

		public int TimeBanned { get; set; } = 300; // seconds
		public int MaxPerMinuteConnection { get; set; } = 10;
		public int MaxSizePacket { get; set; } = 1024 * 1024 * 25; // 25 mb
		public bool Validate(byte[] bytes)
		{
			return bytes.Length < MaxSizePacket;
		}
		private void CheckClients()
		{
			foreach (var i in Clients)
			{
				DateTime currentTime = DateTime.Now;
				DateTime oldestAllowedTime = currentTime.AddSeconds(-60);
				Clients[i.Key].RemoveAll(cl => cl.Time < oldestAllowedTime);

				if (Clients[i.Key].Count > MaxPerMinuteConnection)
				{
					if (BannedIP.ContainsKey(i.Key))
						BannedIP[i.Key] = DateTime.Now;
					else 
						BannedIP.Add(i.Key, DateTime.Now);
				}
			}
			List<string> remove = new List<string>();
			foreach (var i in BannedIP)
			{
				if (i.Value < DateTime.Now.AddSeconds(-TimeBanned))
					remove.Add(i.Key);
			}
			foreach (var i in remove)
				BannedIP.Remove(i);
		}
		public bool ValidateConnect(TcpClient client)
		{
			var ip = client.Client.RemoteEndPoint.ToString().Split(":")[0];
			if (Clients.ContainsKey(ip))
			{
				Clients[ip].Add(new SvarogClientInfo());
			}
			else
				Clients.Add(ip, new List<SvarogClientInfo>() { new SvarogClientInfo() });
			CheckClients();
			return BannedIP.ContainsKey(ip) == false;
		}

		public bool ValidateHeader(byte[] bytes)
		{
			int packetLength = BitConverter.ToInt32(bytes, 0);
			return packetLength < MaxSizePacket;
		}

		public bool ValidatePacket(BasePacket packet)
		{
			return true;
		}
	}
}
