using Microsoft.Extensions.DependencyInjection;
using RCS.Base.Command;
using RCS.Service;
using RCS.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCS.ViewModels.Pages.Main
{

	public class TrustedCertificatesPageVM : Base.ViewModel.BaseViewModel
	{
		public TrustedCertificatesPageVM()
		{
			#region Commands
			#endregion
		}

		#region Parametrs
		public Settings Settings => App.Host.Services.GetRequiredService<Settings>();
		#endregion

		#region Commands

		#region UpdateTrustedCommand: Description
		private ICommand _UpdateTrustedCommand;
		public ICommand UpdateTrustedCommand => _UpdateTrustedCommand ??= new LambdaCommand(OnUpdateTrustedCommandExecuted, CanUpdateTrustedCommandExecute);
		private bool CanUpdateTrustedCommandExecute(object e) => true;
		private void OnUpdateTrustedCommandExecuted(object e)
		{
			Service.Settings.Instance.CertificateStore.Load();
		}
		#endregion
		#endregion

		#region Functions
		#endregion
	}
}
