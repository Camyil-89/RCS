using RCS.Certificates;
using RCS.Net.Packets;
using RCS.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Service.UI.Client
{
	public class TCPSender : ITCPSender
	{
		public BasePacket SendAndWait(BasePacket packet, RCSTCPConnection connection)
		{
			var answer = WindowManager.ShowTCPSenderWindow(packet, connection);
			return answer.TypeClose == TypeCloseWindow.OK ? answer.Packet : null;
		}
	}
}
