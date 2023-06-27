using Microsoft.Extensions.DependencyInjection;
using RCS.Base.Command;
using RCS.Service;
using RCS.Service.UI;
using RCS.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.IO;
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

		#region GetCertificatesFromCenterCommand: Description
		private ICommand _GetCertificatesFromCenterCommand;
		public ICommand GetCertificatesFromCenterCommand => _GetCertificatesFromCenterCommand ??= new LambdaCommand(OnGetCertificatesFromCenterCommandExecuted, CanGetCertificatesFromCenterCommandExecute);
		private bool CanGetCertificatesFromCenterCommandExecute(object e) => true;
		private void OnGetCertificatesFromCenterCommandExecuted(object e)
		{
			try
			{
				string info = "";
				foreach (var i in Service.UI.Client.ClientManager.GetLastCertificates())
				{
					if (File.Exists($"{XmlProvider.PathToTrustedCertificates}\\{i.Info.UID}.сертификат") == false)
					{
						info += $"{i.Info.Name}\n";
						i.SaveToFile($"{XmlProvider.PathToTrustedCertificates}\\{i.Info.UID}");
					}
				}
				if (string.IsNullOrEmpty(info) == false)
				{
					Service.Settings.Instance.CertificateStore.Load();
					MessageBoxHelper.InfoShow($"Получены сертификаты:\n{info.TrimEnd()}");
				}
				else
				{
					MessageBoxHelper.InfoShow($"Нет доступных сертификатов!");
				}

			}
			catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось получить сертификаты у ЦС!\n\n{ex.Message}"); }
		}
		#endregion
		#endregion

		#region Functions
		#endregion
	}
}
