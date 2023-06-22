using RCS.Base.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCS.ViewModels.Windows
{
	public class MainVM : Base.ViewModel.BaseViewModel
	{
		public MainVM()
		{
			#region Commands
			#endregion
			try
			{
				Service.Certificate.CertificateProvider.Test();
			} catch (Exception ex) { Console.WriteLine(ex); }
		}

		#region Parametrs
		#endregion

		#region Commands
		#endregion

		#region Functions
		#endregion
	}
}
