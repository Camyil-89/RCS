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
using System.Windows;
using System.Runtime.ConstrainedExecution;
using RCS.Service.Certificate;

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


		#region VisibilityCertificatePanel: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityCertificatePanel = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityCertificatePanel { get => _VisibilityCertificatePanel; set => Set(ref _VisibilityCertificatePanel, value); }
		#endregion

		#region SelectedCertificate: Description
		/// <summary>Description</summary>
		private Models.Certificates.Russian.CertificateSecret _SelectedCertificate;
		/// <summary>Description</summary>
		public Models.Certificates.Russian.CertificateSecret SelectedCertificate { get => _SelectedCertificate; set => Set(ref _SelectedCertificate, value); }
		#endregion
		#endregion

		#region Commands
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
					if (cert.Info.DateDead < DateTime.Now)
					{
						MessageBoxHelper.WarningShow("Данный сертификат не действителен!");
						return;
					}
					Settings.Instance.CertificateStore.Load();
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


		#region CheckFileOwnerCommand: Description
		private ICommand _CheckFileOwnerCommand;
		public ICommand CheckFileOwnerCommand => _CheckFileOwnerCommand ??= new LambdaCommand(OnCheckFileOwnerCommandExecuted, CanCheckFileOwnerCommandExecute);
		private bool CanCheckFileOwnerCommandExecute(object e) => true;
		private void OnCheckFileOwnerCommandExecuted(object e)
		{
			try
			{
				CommonOpenFileDialog dialog = new CommonOpenFileDialog();
				dialog.IsFolderPicker = false;
				dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
				dialog.Multiselect = false;

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					WindowManager.ShowInfoAboutCertificate(CertificateProvider.RCSLoadCertificateFromZip(dialog.FileName));
				}

			}
			catch (Exception ex) { MessageBoxHelper.WarningShow("Не удалось прочитать владельца!"); Console.WriteLine(ex); }
		}
		#endregion

		#region CheckSignCommand: Description
		private ICommand _CheckSignCommand;
		public ICommand CheckSignCommand => _CheckSignCommand ??= new LambdaCommand(OnCheckSignCommandExecuted, CanCheckSignCommandExecute);
		private bool CanCheckSignCommandExecute(object e) => true;
		private void OnCheckSignCommandExecuted(object e)
		{
			try
			{
				CommonOpenFileDialog dialog = new CommonOpenFileDialog();
				dialog.IsFolderPicker = false;
				dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
				dialog.Multiselect = false;

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					if (CertificateProvider.RCSCheckSignFile(dialog.FileName))
					{
						MessageBoxHelper.InfoShow($"Файл не изменялся!\n{dialog.FileName}");
					}
					else
					{
						MessageBoxHelper.ErrorShow($"Файл был изменен!\n{dialog.FileName}");
					}
				}

			}
			catch (Exception ex) { MessageBoxHelper.WarningShow("Не удалось проверить документ или .zip файл!\nСкорей всего файл был изменен!"); Console.WriteLine(ex); }
		}
		#endregion

		#region SignCommand: Description
		private ICommand _SignCommand;
		public ICommand SignCommand => _SignCommand ??= new LambdaCommand(OnSignCommandExecuted, CanSignCommandExecute);
		private bool CanSignCommandExecute(object e) => true;
		private void OnSignCommandExecuted(object e)
		{
			try
			{
				CommonOpenFileDialog dialog = new CommonOpenFileDialog();
				dialog.IsFolderPicker = false;
				dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
				dialog.Multiselect = false;

				if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					SelectedCertificate.SignZipFile(dialog.FileName);
					MessageBoxHelper.InfoShow($"Файл успешно подписан!\n{dialog.FileName}");
				}
				
			} catch (Exception ex) { MessageBoxHelper.WarningShow("Не удалось подписать документ или .zip файл!"); }
		}
		#endregion

		#region MoreInfoCommand: Description
		private ICommand _MoreInfoCommand;
		public ICommand MoreInfoCommand => _MoreInfoCommand ??= new LambdaCommand(OnMoreInfoCommandExecuted, CanMoreInfoCommandExecute);
		private bool CanMoreInfoCommandExecute(object e) => true;
		private void OnMoreInfoCommandExecuted(object e)
		{
			Service.UI.WindowManager.ShowInfoAboutCertificate(SelectedCertificate.Certificate);
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
					SelectedCertificate = Service.Certificate.CertificateProvider.RCSLoadCertificateSecret(dialog.FileName);
					Settings.Instance.CertificateStore.Load();
					var root = Settings.Instance.CertificateStore.FindMasterCertificate(SelectedCertificate.Certificate);

					if (root == null)
					{
						MessageBoxHelper.WarningShow("Данный сертификат не действителен!");
					}
					else
						MessageBoxHelper.InfoShow($"Сертификат действителен!");
					VisibilityCertificatePanel = Visibility.Visible;
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
			}
		}
		#endregion

		#region ViewInfoCertificateCommand: Description
		private ICommand _ViewInfoCertificateCommand;
		public ICommand ViewInfoCertificateCommand => _ViewInfoCertificateCommand ??= new LambdaCommand(OnViewInfoCertificateCommandExecuted, CanViewInfoCertificateCommandExecute);
		private bool CanViewInfoCertificateCommandExecute(object e) => true;
		private void OnViewInfoCertificateCommandExecuted(object e)
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
					Service.UI.WindowManager.ShowInfoAboutCertificate(cert);
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
			}
		}
		#endregion
		#endregion

		#region Functions


		#endregion
	}
}
