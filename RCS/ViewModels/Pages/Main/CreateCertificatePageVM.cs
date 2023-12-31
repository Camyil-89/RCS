﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using RCS.Base.Command;
using RCS.Certificates;
using RCS.Certificates;
using RCS.Service;
using RCS.Service.UI;
using RCS.Service.UI.Selector;
using RCS.ViewModels.Windows;
using RCS.Views.Pages.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
				return EnumHelper.GetAllValuesAndDescriptions<Certificates.TypeAttribute>().Where((i) => i.Value != "");
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
					SelectedCertificate = null;
				}
			}
		}
		#endregion


		#region KeySizeList: Description
		/// <summary>Description</summary>
		private ObservableCollection<int> _KeySizeList = new ObservableCollection<int>() { 512, 1024, 2048, 4096, 3072, 7680, 15360 };
		/// <summary>Description</summary>
		public ObservableCollection<int> KeySizeList { get => _KeySizeList; set => Set(ref _KeySizeList, value); }
		#endregion


		#region SelectedKeySize: Description
		/// <summary>Description</summary>
		private int _SelectedKeySize = 2048;
		/// <summary>Description</summary>
		public int SelectedKeySize { get => _SelectedKeySize; set => Set(ref _SelectedKeySize, value); }
		#endregion

		#region SelectedCertificate: Description
		/// <summary>Description</summary>
		private Certificates.CertificateSecret _SelectedCertificate;
		/// <summary>Description</summary>
		public Certificates.CertificateSecret SelectedCertificate { get => _SelectedCertificate; set => Set(ref _SelectedCertificate, value); }
		#endregion

		#region AttributeView: Description
		/// <summary>Description</summary>
		private ObservableCollection<AttriburteView> _AttributeView = new ObservableCollection<AttriburteView>();
		/// <summary>Description</summary>
		public ObservableCollection<AttriburteView> AttributeView { get => _AttributeView; set => Set(ref _AttributeView, value); }
		#endregion


		#region IsSSL: Description
		/// <summary>Description</summary>
		private bool _IsSSL = false;
		/// <summary>Description</summary>
		public bool IsSSL { get => _IsSSL; set => Set(ref _IsSSL, value); }
		#endregion
		#region EnableButtonSelectCert: Description
		/// <summary>Description</summary>
		private bool _EnableButtonSelectCert = true;
		/// <summary>Description</summary>
		public bool EnableButtonSelectCert { get => _EnableButtonSelectCert; set => Set(ref _EnableButtonSelectCert, value); }
		#endregion

		#region InfoSertificate: Description
		/// <summary>Description</summary>
		private Certificates.CertificateInfo _InfoSertificate = new CertificateInfo();
		/// <summary>Description</summary>
		public Certificates.CertificateInfo InfoSertificate { get => _InfoSertificate; set => Set(ref _InfoSertificate, value); }
		#endregion

		#region SelectedTypeAttribute: Description
		/// <summary>Description</summary>
		private Certificates.TypeAttribute _SelectedTypeAttribute = Certificates.TypeAttribute.String;
		/// <summary>Description</summary>
		public Certificates.TypeAttribute SelectedTypeAttribute { get => _SelectedTypeAttribute; set => Set(ref _SelectedTypeAttribute, value); }
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
		}
		#endregion


		#region SelectCertMasterCommand: Description
		private ICommand _SelectCertMasterCommand;
		public ICommand SelectCertMasterCommand => _SelectCertMasterCommand ??= new LambdaCommand(OnSelectCertMasterCommandExecuted, CanSelectCertMasterCommandExecute);
		private bool CanSelectCertMasterCommandExecute(object e) => true;
		private void OnSelectCertMasterCommandExecuted(object e)
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
					SelectedCertificate = Certificates.CertificateManager.RCSLoadCertificateSecret(dialog.FileName);
					CertificateManager.Store.Load();
					var root = CertificateManager.Store.FindMasterCertificate(SelectedCertificate.Certificate).Certificate;

					if (root == null)
					{
						MessageBoxHelper.WarningShow("Данный сертификат не действителен!");
					}
					else
						MessageBoxHelper.InfoShow($"Сертификат действителен!");

					InfoSertificate.MasterUID = SelectedCertificate.Certificate.Info.UID;
					InfoSertificate.Master = SelectedCertificate.Certificate.Info.Name;
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
			}
		}
		#endregion

		#region AddAttributeCommand: Description
		private ICommand _AddAttributeCommand;
		public ICommand AddAttributeCommand => _AddAttributeCommand ??= new LambdaCommand(OnAddAttributeCommandExecuted, CanAddAttributeCommandExecute);
		private bool CanAddAttributeCommandExecute(object e) => true;
		private void OnAddAttributeCommandExecuted(object e)
		{
			var att = WindowManager.ShowAddAttributeWindow(SelectedTypeAttribute);
			try
			{
				if (att != null)
				{
					InfoSertificate.AddAttribute(att);
					AttributeView.Add(new AttriburteView() { Attribute = att });
				}
			}
			catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось добавить поле!\n{ex.Message}"); }
		}
		#endregion


		#region CreateCertCommand: Description
		private ICommand _CreateCertCommand;
		public ICommand CreateCertCommand => _CreateCertCommand ??= new LambdaCommand(OnCreateCertCommandExecuted, CanCreateCertCommandExecute);
		private bool CanCreateCertCommandExecute(object e) => true;
		private void OnCreateCertCommandExecuted(object e)
		{

			if (string.IsNullOrEmpty(InfoSertificate.Name))
			{
				MessageBoxHelper.WarningShow($"Укажите кому выдан!");
				return;
			}

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Выберите место сохранения";
			dialog.Filter = "Файлы .ссертификат|*.ссертификат";
			dialog.FileName = InfoSertificate.Name;
			dialog.DefaultExt = ".ссертификат";

			if (SelectedCertificate == null && SelfSign == false)
			{
				MessageBoxHelper.WarningShow($"Выберите родительский сертификат!");
				return;
			}

			if (dialog.ShowDialog() == true)
			{
				CreateSettingsCertificate settings = new CreateSettingsCertificate();
				settings.Info = InfoSertificate;
				settings.SizeKey = SelectedKeySize;
				settings.Name = InfoSertificate.Name;
				if (SelfSign == false)
					settings.MasterCertificate = SelectedCertificate;
				if (IsSSL)
				{
					var x_cert = CreateCertificate(Certificates.CertificateManager.RCSCreateCertificate(settings));
					settings.Info.AddAttribute(new CertificateAttribute() { Name = "RCS.SSL.CERTIFICATE.X509", Type = TypeAttribute.ByteArray, Data = x_cert.Export(X509ContentType.Pfx), FileName = "ssl.pfx" });
				}
				var cert = Certificates.CertificateManager.RCSCreateCertificate(settings);
				cert.SaveToFile(dialog.FileName);
				cert.Certificate.SaveToFile(dialog.FileName.Replace(".ссертификат", ".сертификат"));
			}
			InfoSertificate = new CertificateInfo();
			SelectedCertificate = null;
			CertificateManager.Store.Load();
			UpdateViewAttribute();
		}
		#endregion
		public static X509Certificate2 CreateCertificate(CertificateSecret certificate)
		{
			using (RSA rsa = RSA.Create(certificate.Certificate.LengthKey))
			{
				// Импорт открытого и закрытого ключей
				rsa.ImportRSAPublicKey(certificate.Certificate.Info.PublicKey, out _);
				rsa.ImportRSAPrivateKey(certificate.PrivateKey, out _);

				// Создание запроса на самоподписанный сертификат
				CertificateRequest request = new CertificateRequest("CN=RCS.SSL.CERTIFICATE.X509", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

				// Создание самоподписанного сертификата из запроса на сертификат
				X509Certificate2 cert = request.CreateSelfSigned(certificate.Certificate.Info.DateCreate, certificate.Certificate.Info.DateDead);
				return cert;
			}
		}
		#region CreateSSLCertificateFromSecretCommand: Description
		private ICommand _CreateSSLCertificateFromSecretCommand;
		public ICommand CreateSSLCertificateFromSecretCommand => _CreateSSLCertificateFromSecretCommand ??= new LambdaCommand(OnCreateSSLCertificateFromSecretCommandExecuted, CanCreateSSLCertificateFromSecretCommandExecute);
		private bool CanCreateSSLCertificateFromSecretCommandExecute(object e) => true;
		private void OnCreateSSLCertificateFromSecretCommandExecuted(object e)
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
					var cert = Certificates.CertificateManager.RCSLoadCertificateSecret(dialog.FileName);
					SaveFileDialog dialog1 = new SaveFileDialog();
					dialog1.Title = "Выберите место сохранения";
					dialog1.Filter = "Файлы .ssl.сертификат|*.ssl.сертификат";
					var fl = new FileInfo(dialog.FileName);

					var flin = new FileInfo(dialog.FileName);
					dialog1.FileName = flin.Name.Replace(flin.Extension, ".ssl.ссертификат");
					dialog1.DefaultExt = ".ssl.ссертификат";



					if (dialog1.ShowDialog() == true)
					{
						var path_public_cert = dialog1.FileName.Replace("ссертификат", "сертификат");

						var x_cert = CreateCertificate(cert);

						cert.Certificate.Info.AddAttribute(new CertificateAttribute() { Name = "RCS.SSL.CERTIFICATE.X509", Type = TypeAttribute.ByteArray, Data = x_cert.Export(X509ContentType.Pfx) });
						cert.SaveToFile(dialog1.FileName);
						Console.WriteLine(dialog1.FileName);
						cert = Certificates.CertificateManager.RCSLoadCertificateSecret(dialog.FileName);
						cert.Certificate.Info.AddAttribute(new CertificateAttribute() { Name = "RCS.SSL.CERTIFICATE.X509", Type = TypeAttribute.ByteArray, Data = x_cert.PublicKey.ExportSubjectPublicKeyInfo() });
						cert.Certificate.SaveToFile(path_public_cert);
						Console.WriteLine(path_public_cert);
						MessageBoxHelper.InfoShow($".ssl.ссертификат и .ssl.сертификат были созданы!");
					}
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}\n\n{ex}"); }
			}
		}
		#endregion

		#region ChangeFileAttributeCommand: Description
		private ICommand _ChangeFileAttributeCommand;
		public ICommand ChangeFileAttributeCommand => _ChangeFileAttributeCommand ??= new LambdaCommand(OnChangeFileAttributeCommandExecuted, CanChangeFileAttributeCommandExecute);
		private bool CanChangeFileAttributeCommandExecute(object e) => true;
		private void OnChangeFileAttributeCommandExecuted(object e)
		{

			var attribute = (e as CertificateAttribute);
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = false;
			dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
			dialog.Multiselect = false;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				try
				{
					attribute.Data = File.ReadAllBytes(dialog.FileName);
					attribute.FileName = new FileInfo(dialog.FileName).Name;
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить файл!\n\n{dialog.FileName}"); }
				UpdateViewAttribute();
			}
		}
		#endregion

		#region CreateCertificateFromSecretCommand: Description
		private ICommand _CreateCertificateFromSecretCommand;
		public ICommand CreateCertificateFromSecretCommand => _CreateCertificateFromSecretCommand ??= new LambdaCommand(OnCreateCertificateFromSecretCommandExecuted, CanCreateCertificateFromSecretCommandExecute);
		private bool CanCreateCertificateFromSecretCommandExecute(object e) => true;
		private void OnCreateCertificateFromSecretCommandExecuted(object e)
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
					var cert = Certificates.CertificateManager.RCSLoadCertificateSecret(dialog.FileName);
					SaveFileDialog dialog1 = new SaveFileDialog();
					dialog1.Title = "Выберите место сохранения";
					dialog1.Filter = "Файлы .сертификат|*.сертификат";
					var fl = new FileInfo(dialog.FileName);
					dialog1.FileName = fl.Name.Replace($".ссертификат", ".сертификат");
					dialog1.DefaultExt = ".сертификат";

					if (dialog1.ShowDialog() == true)
					{
						cert.Certificate.SaveToFile(dialog1.FileName);
					}
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!\n\n{dialog.FileName}"); }
			}
		}
		#endregion


		#region MoveAttributeUpCommand: Description
		private ICommand _MoveAttributeUpCommand;
		public ICommand MoveAttributeUpCommand => _MoveAttributeUpCommand ??= new LambdaCommand(OnMoveAttributeUpCommandExecuted, CanMoveAttributeUpCommandExecute);
		private bool CanMoveAttributeUpCommandExecute(object e) => true;
		private void OnMoveAttributeUpCommandExecuted(object e)
		{
			var index = InfoSertificate.Attributes.IndexOf((e as CertificateAttribute));
			if (index > 0)
			{
				var selectedItem = InfoSertificate.Attributes[index];

				InfoSertificate.Attributes.RemoveAt(index);
				InfoSertificate.Attributes.Insert(index - 1, selectedItem);
				UpdateViewAttribute();
			}
		}
		#endregion

		#region MoveAttributeDownCommand: Description
		private ICommand _MoveAttributeDownCommand;
		public ICommand MoveAttributeDownCommand => _MoveAttributeDownCommand ??= new LambdaCommand(OnMoveAttributeDownCommandExecuted, CanMoveAttributeDownCommandExecute);
		private bool CanMoveAttributeDownCommandExecute(object e) => true;
		private void OnMoveAttributeDownCommandExecuted(object e)
		{
			var index = InfoSertificate.Attributes.IndexOf((e as CertificateAttribute));
			if (index < InfoSertificate.Attributes.Count - 1)
			{
				var selectedItem = InfoSertificate.Attributes[index];

				InfoSertificate.Attributes.RemoveAt(index);
				InfoSertificate.Attributes.Insert(index + 1, selectedItem);
				UpdateViewAttribute();
			}
		}
		#endregion

		#region DeleteAttributeCommand: Description
		private ICommand _DeleteAttributeCommand;
		public ICommand DeleteAttributeCommand => _DeleteAttributeCommand ??= new LambdaCommand(OnDeleteAttributeCommandExecuted, CanDeleteAttributeCommandExecute);
		private bool CanDeleteAttributeCommandExecute(object e) => true;
		private void OnDeleteAttributeCommandExecuted(object e)
		{
			var att = (e as Certificates.CertificateAttribute);

			if (MessageBoxHelper.QuestionShow($"Вы уверены что хотите удалить поле?\n{att.Name} [{att.Type.Description()}]") != System.Windows.MessageBoxResult.Yes)
				return;

			InfoSertificate.Attributes.Remove(att);
			UpdateViewAttribute();
		}
		#endregion
		#endregion

		#region Functions
		private void UpdateViewAttribute()
		{
			AttributeView.Clear();
			foreach (var i in InfoSertificate.Attributes)
			{
				AttributeView.Add(new AttriburteView() { Attribute = i });
			}
		}
		private void CallbackOpen(Page Page)
		{
			if (Page == App.Host.Services.GetRequiredService<CreateCertificatePage>())
			{

			}
		}
		#endregion
	}
}
