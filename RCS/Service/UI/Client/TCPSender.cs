using RCS.Certificates;
using RCS.Net.Packets;
using RCS.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTCP;

namespace RCS.Service.UI.Client
{
	public class TCPSender : ITCPSender
	{
		public T SendAndWait<T>(T packet, EasyTCP.Client client)
		{
			var answer = WindowManager.ShowTCPSenderWindow<T>(packet, client);
			return answer.TypeClose == TypeCloseWindow.OK ? answer.Packet : default(T);
		}
	}
}
