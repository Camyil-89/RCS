using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[Serializable]
	public class PacketCertificate
	{
		public object CertificateObj { get; set; } = "";
		public bool IsValid = false;
	}
}
