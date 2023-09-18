using RCS.Net.Packets;
using RCS.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace RCS.Certificates
{
	public interface ITCPSender
	{
		public T SendAndWait<T>(T packet, EasyTCP.Client client);
	}
}
