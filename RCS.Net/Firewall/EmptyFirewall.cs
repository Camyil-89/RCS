using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Firewall
{
	internal class EmptyFirewall : IFirewall
	{
		public bool Validate(byte[] bytes)
		{
			return true;
		}

		public bool ValidateConnect(TcpClient client)
		{
			return true;
		}

		public bool ValidateHeader(HeaderPacket header)
		{
			return true;
		}

		public bool ValidatePacket(BasePacket packet)
		{
			return true;
		}
	}
}
