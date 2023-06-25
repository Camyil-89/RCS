using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[Serializable]
	public class Ping : BasePacket
	{
		public DateTime Time { get; set; } = DateTime.Now;
		public Ping()
		{
			Type = PacketType.Ping;
		}
	}
}
