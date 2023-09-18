using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[Serializable]
	public class PacketRequestCertificate
	{
		public object Certificate { get; set; } = null;
		public Guid RequestPacket { get; set; }
	}
}
