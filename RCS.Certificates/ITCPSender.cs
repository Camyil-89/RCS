using RCS.Net.Packets;
using RCS.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Certificates
{
	public interface ITCPSender
	{
		public BasePacket SendAndWait(BasePacket packet, RCSTCPConnection connection);
	}
}
