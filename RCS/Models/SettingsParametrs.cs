using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Models
{
	public class SettingsParametrs: Base.ViewModel.BaseViewModel
	{

		#region Client: Description
		/// <summary>Description</summary>
		private ClientParametrs _Client = new ClientParametrs();
		/// <summary>Description</summary>
		public ClientParametrs Client { get => _Client; set => Set(ref _Client, value); }
		#endregion
	}
}
