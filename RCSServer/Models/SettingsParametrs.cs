using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.Models
{
	public class SettingsParametrs: Base.ViewModel.BaseViewModel
	{

		#region PathToCertificate: Description
		/// <summary>Description</summary>
		private string _PathToCertificate;
		/// <summary>Description</summary>
		public string PathToCertificate { get => _PathToCertificate; set => Set(ref _PathToCertificate, value); }
		#endregion
	}
}
