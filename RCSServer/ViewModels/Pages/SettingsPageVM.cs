using Microsoft.WindowsAPICodePack.Dialogs;
using RCS.Certificates;
using RCS.Service.UI;
using RCS.Service;
using RCSServer.Base.Command;
using RCSServer.Service;
using RCSServer.Service.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RCSServer.Service.Server;

namespace RCSServer.ViewModels.Pages
{

	public class SettingsPageVM : Base.ViewModel.BaseViewModel
	{
		public SettingsPageVM()
		{
			#region Commands
			#endregion
			RCSServer.Service.UI.Navigate.CallbackOpenMenuEvent += Navigate_CallbackOpenMenuEvent;
		}

		private void Navigate_CallbackOpenMenuEvent(System.Windows.Controls.Page page)
		{
			UpdateValue();
		}
		private void UpdateValue()
		{
			if (Service.Settings.Instance.Cerificate != null)
			{
				SelectedCertificate = Service.Settings.Instance.Cerificate.Certificate.Info.Name;
				EnableViewCert = true;
			}
			else
			{
				SelectedCertificate = "Не выбран!";
				EnableViewCert = false;
			}
		}

		#region Parametrs




		#region EnableViewCert: Description
		/// <summary>Description</summary>
		private bool _EnableViewCert = false;
		/// <summary>Description</summary>
		public bool EnableViewCert { get => _EnableViewCert; set => Set(ref _EnableViewCert, value); }
		#endregion

		#region SelectedCertificate: Description
		/// <summary>Description</summary>
		private string _SelectedCertificate;
		/// <summary>Description</summary>
		public string SelectedCertificate { get => _SelectedCertificate; set => Set(ref _SelectedCertificate, value); }
		#endregion

		#region VisibilityConnectionsSettings: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityConnectionsSettings = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityConnectionsSettings { get => _VisibilityConnectionsSettings; set => Set(ref _VisibilityConnectionsSettings, value); }
		#endregion

		#region TagConnectionSettings: Description
		/// <summary>Description</summary>
		private string _TagConnectionSettings;
		/// <summary>Description</summary>
		public string TagConnectionSettings { get => _TagConnectionSettings; set => Set(ref _TagConnectionSettings, value); }
		#endregion
		#endregion

		#region Commands

		#region OpenMenuCommand: Description
		private ICommand _OpenMenuCommand;
		public ICommand OpenMenuCommand => _OpenMenuCommand ??= new LambdaCommand(OnOpenMenuCommandExecuted, CanOpenMenuCommandExecute);
		private bool CanOpenMenuCommandExecute(object e) => true;
		private void OnOpenMenuCommandExecuted(object e)
		{
			RCSServer.Service.UI.Navigate.SelectSettingsMenu(e.ToString());
		}
		#endregion
		#region SelectCertificateCommand: Description
		private ICommand _SelectCertificateCommand;
		public ICommand SelectCertificateCommand => _SelectCertificateCommand ??= new LambdaCommand(OnSelectCertificateCommandExecuted, CanSelectCertificateCommandExecute);
		private bool CanSelectCertificateCommandExecute(object e) => true;
		private void OnSelectCertificateCommandExecuted(object e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = false;
			dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
			dialog.Multiselect = false;

			CommonFileDialogFilter filter = new CommonFileDialogFilter("Файлы сертификатов", "*.ссертификат");
			dialog.Filters.Add(filter);
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileName.EndsWith("ссертификат"))
			{
				try
				{
					var cert = RCS.Certificates.CertificateManager.RCSLoadCertificateSecret(dialog.FileName);
					if (cert.Certificate.Info.DateDead < DateTime.Now)
					{
						MessageBoxHelper.WarningShow("Данный сертификат не действителен! Истекло время!");
						UpdateValue();
						return;
					}
					if (CertificateManager.RCSValidatingCertificate(cert.Certificate))
					{
						Service.Settings.Instance.Parametrs.PathToCertificate = dialog.FileName;
						Service.Settings.Instance.Cerificate = cert;
						MessageBoxHelper.InfoShow($"Сертификат действителен! Чтобы изменения  вступили в силу, перезапустите сервер!");
					}
					else
					{
						MessageBoxHelper.WarningShow("Данный сертификат не действителен!");
					}
					UpdateValue();
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
			}
		}
		#endregion

		#region ViewCertificateCommand: Description
		private ICommand _ViewCertificateCommand;
		public ICommand ViewCertificateCommand => _ViewCertificateCommand ??= new LambdaCommand(OnViewCertificateCommandExecuted, CanViewCertificateCommandExecute);
		private bool CanViewCertificateCommandExecute(object e) => true;
		private void OnViewCertificateCommandExecuted(object e)
		{
			RCS.Service.UI.WindowManager.ShowInfoAboutCertificate(RCSServer.Service.Settings.Instance.Cerificate.Certificate);
		}
		#endregion
		#endregion

		#region Functions
		#endregion
	}

}
