using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using RCS.Base.Command;
using RCS.Models.Certificates;
using RCS.Models.Certificates.Russian;
using RCS.Service;
using RCS.Service.UI;
using RCS.ViewModels.Windows;
using RCS.Views.Pages.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RCS.ViewModels.Pages.Main
{

	public class CreateCertificatePageVM : Base.ViewModel.BaseViewModel
	{
		public CreateCertificatePageVM()
		{
			#region Commands
			#endregion
			Navigate.CallbackOpenMenuEvent += CallbackOpen;
		}

		#region Parametrs
		public IEnumerable<KeyValuePair<string, string>> TypeAttributeCertificate
		{
			get
			{
				return EnumHelper.GetAllValuesAndDescriptions<Models.Certificates.Russian.TypeAttribute>().Where((i) => i.Value != "");
			}
		}
		public Settings Settings => App.Host.Services.GetRequiredService<Settings>();

		#region SelfSign: Description
		/// <summary>Description</summary>
		private bool _SelfSign;
		/// <summary>Description</summary>
		public bool SelfSign
		{
			get => _SelfSign; set
			{
				Set(ref _SelfSign, value);
				EnableButtonSelectCert = !value;
				if (value == true)
				{
					InfoSertificate.Master = InfoSertificate.Name;
					InfoSertificate.MasterUID = InfoSertificate.UID;
					SelectedStoreItem = null;
				}
			}
		}
		#endregion


		#region SelectedStoreItem: Description
		/// <summary>Description</summary>
		private StoreItem _SelectedStoreItem;
		/// <summary>Description</summary>
		public StoreItem SelectedStoreItem
		{
			get => _SelectedStoreItem; set
			{
				Set(ref _SelectedStoreItem, value);
				if (value != null)
				{
					InfoSertificate.Master = value.Certificate.Certificate.Info.Name;
					InfoSertificate.MasterUID = value.Certificate.Certificate.Info.UID;
					Console.WriteLine(InfoSertificate.MasterUID);
				}
			}
		}
		#endregion

		#region EnableButtonSelectCert: Description
		/// <summary>Description</summary>
		private bool _EnableButtonSelectCert = true;
		/// <summary>Description</summary>
		public bool EnableButtonSelectCert { get => _EnableButtonSelectCert; set => Set(ref _EnableButtonSelectCert, value); }
		#endregion

		#region InfoSertificate: Description
		/// <summary>Description</summary>
		private Models.Certificates.Russian.CertificateInfo _InfoSertificate = new CertificateInfo();
		/// <summary>Description</summary>
		public Models.Certificates.Russian.CertificateInfo InfoSertificate { get => _InfoSertificate; set => Set(ref _InfoSertificate, value); }
		#endregion

		#region SelectedTypeAttribute: Description
		/// <summary>Description</summary>
		private Models.Certificates.Russian.TypeAttribute _SelectedTypeAttribute = Models.Certificates.Russian.TypeAttribute.String;
		/// <summary>Description</summary>
		public Models.Certificates.Russian.TypeAttribute SelectedTypeAttribute { get => _SelectedTypeAttribute; set => Set(ref _SelectedTypeAttribute, value); }
		#endregion
		#endregion

		#region Commands



		//#region SelectCertMasterCommand: Description
		//private ICommand _SelectCertMasterCommand;
		//public ICommand SelectCertMasterCommand => _SelectCertMasterCommand ??= new LambdaCommand(OnSelectCertMasterCommandExecuted, CanSelectCertMasterCommandExecute);
		//private bool CanSelectCertMasterCommandExecute(object e) => true;
		//private void OnSelectCertMasterCommandExecuted(object e)
		//{
		//	CommonOpenFileDialog dialog = new CommonOpenFileDialog();
		//	dialog.IsFolderPicker = false;
		//	dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
		//	dialog.Multiselect = false;

		//	CommonFileDialogFilter filter = new CommonFileDialogFilter("Файлы сертификатов", "*.ссертификат");
		//	dialog.Filters.Add(filter);
		//	if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileName.EndsWith("ссертификат"))
		//	{
		//		try
		//		{
		//			var cert = Service.Certificate.CertificateProvider.RCSLoadCertificateSecret(dialog.FileName);
		//			InfoSertificate.Master = cert.Certificate.Info.Master;
		//			InfoSertificate.MasterUID = cert.Certificate.Info.MasterUID;

		//		} catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
		//	}
		//}
		//#endregion

		#region ClearInfoCommand: Description
		private ICommand _ClearInfoCommand;
		public ICommand ClearInfoCommand => _ClearInfoCommand ??= new LambdaCommand(OnClearInfoCommandExecuted, CanClearInfoCommandExecute);
		private bool CanClearInfoCommandExecute(object e) => true;
		private void OnClearInfoCommandExecuted(object e)
		{
			if (MessageBoxHelper.QuestionShow("Вы уверены что хотите очистить все поля?") != System.Windows.MessageBoxResult.Yes)
				return;
			InfoSertificate = new CertificateInfo();
		}
		#endregion

		#region CreateCertCommand: Description
		private ICommand _CreateCertCommand;
		public ICommand CreateCertCommand => _CreateCertCommand ??= new LambdaCommand(OnCreateCertCommandExecuted, CanCreateCertCommandExecute);
		private bool CanCreateCertCommandExecute(object e) => true;
		private void OnCreateCertCommandExecuted(object e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Выберите место сохранения";
			dialog.Filter = "Файлы .ссертификат|*.ссертификат";
			dialog.FileName = "Сертификат";
			dialog.DefaultExt = ".ссертификат";

			if (dialog.ShowDialog() == true)
			{
				CreateSettingsCertificate settings = new CreateSettingsCertificate();
				settings.Info = InfoSertificate;
				settings.Name = InfoSertificate.Name;
				Console.WriteLine(InfoSertificate.MasterUID);
				if (SelfSign == false)
					settings.MasterCertificate = Settings.CertificateStore.GetItem(InfoSertificate.MasterUID).Certificate;
				var cert = Service.Certificate.CertificateProvider.RCSCreateCertificate(settings);
				cert.SaveToFile(dialog.FileName);
				cert.Certificate.SaveToFile(dialog.FileName.Replace(".ссертификат", ".сертификат"));
			}
		}
		#endregion
		#endregion

		#region Functions
		private void CallbackOpen(Page Page)
		{
			if (Page == App.Host.Services.GetRequiredService<CreateCertificatePage>())
			{

			}
		}
		#endregion
	}
}
