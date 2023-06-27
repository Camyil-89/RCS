using RCSServer.Base.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.ViewModels.Windows
{
	public class MainVM : Base.ViewModel.BaseViewModel
	{
		public MainVM()
		{
			#region Commands
			#endregion
			Service.Startup.Init();
		}

		#region Parametrs

		#region Test: Description
		/// <summary>Description</summary>
		private string _Test;
		/// <summary>Description</summary>
		public string Test { get => _Test; set => Set(ref _Test, value); }
		#endregion
		#endregion

		#region Commands
		#endregion

		#region Functions
		#endregion
	}
}
