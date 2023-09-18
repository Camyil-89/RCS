using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Ping
	{
		public Ping()
		{
		}

		public DateTime Time { get; set; } = DateTime.Now;
	}
}
