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
using System.Collections.ObjectModel;
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
		#region Panels: Description
		/// <summary>Description</summary>
		private ObservableCollection<StackPanel> _Panels = new ObservableCollection<StackPanel>();
		/// <summary>Description</summary>
		public ObservableCollection<StackPanel> Panels { get => _Panels; set => Set(ref _Panels, value); }
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

		#region ClearInfoCommand: Description


		private ICommand _ClearInfoCommand;
		public ICommand ClearInfoCommand => _ClearInfoCommand ??= new LambdaCommand(OnClearInfoCommandExecuted, CanClearInfoCommandExecute);
		private bool CanClearInfoCommandExecute(object e) => true;
		private void OnClearInfoCommandExecuted(object e)
		{
			if (MessageBoxHelper.QuestionShow("Вы уверены что хотите очистить все поля?") != System.Windows.MessageBoxResult.Yes)
				return;
			InfoSertificate = new CertificateInfo();
			ViewAttributes();
		}
		#endregion


		#region AddAttributeCommand: Description
		private ICommand _AddAttributeCommand;
		public ICommand AddAttributeCommand => _AddAttributeCommand ??= new LambdaCommand(OnAddAttributeCommandExecuted, CanAddAttributeCommandExecute);
		private bool CanAddAttributeCommandExecute(object e) => true;
		private void OnAddAttributeCommandExecuted(object e)
		{
			var att = WindowManager.ShowAddAttributeWindow(SelectedTypeAttribute);
			if (att != null)
				InfoSertificate.Attributes.Add(att);

			ViewAttributes();
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

			if (SelectedStoreItem == null && SelfSign == false)
			{
				MessageBoxHelper.WarningShow($"Выберите родительский сертификат!");
				return;
			}

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
		private void ViewAttributes()
		{
			Panels.Clear();
			Panels = Service.UI.CertificateUI.RCSGenerateUIElements(InfoSertificate, true, true);
			foreach (var i in Panels)
			{
				var btn = (((Grid)i.Children[0]).Children[0] as Button);
				btn.Click += Btn_Click;
			}
		}
		private void Btn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var att = ((sender as Button).Tag as Models.Certificates.Russian.CertificateAttribute);

			if (MessageBoxHelper.QuestionShow($"Вы уверены что хотите удалить поле?\n{att.Name} [{att.Type.Description()}]") != System.Windows.MessageBoxResult.Yes)
				return;
			
			InfoSertificate.Attributes.Remove(att);
			ViewAttributes();
		}
		#endregion
	}
}
