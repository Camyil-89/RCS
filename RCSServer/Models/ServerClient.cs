using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.Models
{
	public class ServerClient: Base.ViewModel.BaseViewModel
	{

		#region Client: Description
		/// <summary>Description</summary>
		private EasyTCP.ServerClient _Client;
		/// <summary>Description</summary>
		public EasyTCP.ServerClient Client { get => _Client; set => Set(ref _Client, value); }
		#endregion


		#region Ping: Description
		/// <summary>Description</summary>
		private double _Ping;
		/// <summary>Description</summary>
		public double Ping { get => _Ping; set => Set(ref _Ping, value); }
		#endregion


		#region RXBytes: Description
		/// <summary>Description</summary>
		private string _RXBytes;
		/// <summary>Description</summary>
		public string RXBytes { get => _RXBytes; set => Set(ref _RXBytes, value); }
		#endregion


		#region TXBytes: Description
		/// <summary>Description</summary>
		private string _TXBytes;
		/// <summary>Description</summary>
		public string TXBytes { get => _TXBytes; set => Set(ref _TXBytes, value); }
		#endregion


		#region RXPackets: Description
		/// <summary>Description</summary>
		private int _RXPackets;
		/// <summary>Description</summary>
		public int RXPackets { get => _RXPackets; set => Set(ref _RXPackets, value); }
		#endregion


		#region TXPackets: Description
		/// <summary>Description</summary>
		private int _TXPackets;
		/// <summary>Description</summary>
		public int TXPackets { get => _TXPackets; set => Set(ref _TXPackets, value); }
		#endregion


		#region RXSpeed: Description
		/// <summary>Description</summary>
		private string _RXSpeed;
		/// <summary>Description</summary>
		public string RXSpeed { get => _RXSpeed; set => Set(ref _RXSpeed, value); }
		#endregion


		#region TXSpeed: Description
		/// <summary>Description</summary>
		private string _TXSpeed;
		/// <summary>Description</summary>
		public string TXSpeed { get => _TXSpeed; set => Set(ref _TXSpeed, value); }
		#endregion
	}
}
