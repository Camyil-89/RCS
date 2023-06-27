using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Models
{
	public class ClientParametrs : Base.ViewModel.BaseViewModel
	{


		#region Address: Description
		/// <summary>Description</summary>
		private string _Address = "127.0.0.1";
		/// <summary>Description</summary>
		public string Address { get => _Address; set => Set(ref _Address, value); }
		#endregion


		#region TimeoutUpdateKeys: Description
		/// <summary>Description</summary>
		private int _TimeoutUpdateKeys = 30;
		/// <summary>Description</summary>
		public int TimeoutUpdateKeys
		{
			get => _TimeoutUpdateKeys; set
			{
				if (value < 5)
					value = 5;
				Set(ref _TimeoutUpdateKeys, value);
			}
		}
		#endregion

		#region Port: Description
		/// <summary>Description</summary>
		private int _Port = 1991;
		/// <summary>Description</summary>
		public int Port { get => _Port; set => Set(ref _Port, value); }
		#endregion


		#region AutoStartClient: Description
		/// <summary>Description</summary>
		private bool _AutoStartClient = false;
		/// <summary>Description</summary>
		public bool AutoStartClient { get => _AutoStartClient; set => Set(ref _AutoStartClient, value); }
		#endregion
	}
}
