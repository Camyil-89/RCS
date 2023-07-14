using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct HeaderPacket
	{
		public byte Version;
		public int DataSize;
		public int UID;
		public static HeaderPacket Create(int data_size, byte version, int uid)
		{
			var x = new HeaderPacket();
			x.DataSize = data_size;
			x.Version = version;

			x.UID = uid;
			return x;	
		}
	}

	public static class Header
	{
		/// <summary>
		/// версия заголовка 1
		/// </summary>
		/// <param name="data_size"></param>
		/// <param name="uid"></param>
		/// <returns></returns>
		public static HeaderPacket Header1(int data_size, int uid)
		{
			return HeaderPacket.Create(data_size, 1, uid);
		}
	}
}
