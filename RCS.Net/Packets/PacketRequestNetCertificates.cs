using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[Serializable]
	public class PacketRequestNetCertificates
	{
		public List<object> Certificates = new List<object>();
	}
}
