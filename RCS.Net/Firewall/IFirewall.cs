using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Firewall
{
	public interface IFirewall
	{
		public bool ValidateHeader(byte[] bytes);
		public bool Validate(byte[] bytes);
		public bool ValidatePacket(BasePacket packet);
		public bool ValidateConnect(TcpClient client);
	}
}
