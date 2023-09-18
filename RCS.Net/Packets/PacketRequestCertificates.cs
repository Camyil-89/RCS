using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[Serializable]
	public class PacketRequestCertificates
	{
		public List<object> Certificates { get; set; } = new List<object>();
	}
}
