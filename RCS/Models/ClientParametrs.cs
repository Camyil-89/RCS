using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Models
{
	public class ClientParametrs: Base.ViewModel.BaseViewModel
	{

		#region IPAddress: Description
		/// <summary>Description</summary>
		private byte[] _IPAddress = new byte[] { 127, 0, 0, 1 };
		/// <summary>Description</summary>
		public byte[] IPAddress { get => _IPAddress; set => Set(ref _IPAddress, value); }
		#endregion


		#region Port: Description
		/// <summary>Description</summary>
		private int _Port = 1991;
		/// <summary>Description</summary>
		public int Port { get => _Port; set => Set(ref _Port, value); }
		#endregion
	}
}
