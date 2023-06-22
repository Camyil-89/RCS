using Microsoft.WindowsAPICodePack.Dialogs;
using RCS.Base.Command;
using RCS.Service.UI;
using RCS.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCS.ViewModels.Pages.Main
{

	public class CheckCertificatePageVM : Base.ViewModel.BaseViewModel
	{
		public CheckCertificatePageVM()
		{
			#region Commands
			#endregion
		}

		#region Parametrs
		#endregion

		#region Commands
		#endregion

		#region Functions

		#region CheckCertCommand: Description
		private ICommand _CheckCertCommand;
		public ICommand CheckCertCommand => _CheckCertCommand ??= new LambdaCommand(OnCheckCertCommandExecuted, CanCheckCertCommandExecute);
		private bool CanCheckCertCommandExecute(object e) => true;
		private void OnCheckCertCommandExecuted(object e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = false;
			dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
			dialog.Multiselect = false;

			CommonFileDialogFilter filter = new CommonFileDialogFilter("Файлы сертификатов", "*.сертификат");
			dialog.Filters.Add(filter);
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileName.EndsWith("сертификат"))
			{
				try
				{
					var cert = Service.Certificate.CertificateProvider.RCSLoadCertificate(dialog.FileName);
					var root = Settings.Instance.CertificateStore.FindMasterCertificate(cert);
					if (root == null)
					{
						MessageBoxHelper.WarningShow("Данный сертификат не действителен!");
						return;
					}
					MessageBoxHelper.InfoShow($"Сертификат действителен!");
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
			}
		}
		#endregion
		#endregion
	}
}
