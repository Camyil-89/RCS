using RCS.Service.UI;
using RCS.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RCS.Base.Command;
using System.IO;
using Microsoft.Win32;
using RCS.Service.UI.Selector;
using RCS.Service.UI.Client;
using RCS.Certificates;

namespace RCS.ViewModels.Windows
{
	public class InfoCertificateWindowVM : Base.ViewModel.BaseViewModel
	{


		#region Certificate: Description
		/// <summary>Description</summary>
		private Certificates.Certificate _Certificate;
		/// <summary>Description</summary>
		public Certificates.Certificate Certificate { get => _Certificate; set => Set(ref _Certificate, value); }
		#endregion


		#region AttributeView: Description
		/// <summary>Description</summary>
		private ObservableCollection<AttriburteView> _AttributeView = new ObservableCollection<AttriburteView>();
		/// <summary>Description</summary>
		public ObservableCollection<AttriburteView> AttributeView { get => _AttributeView; set => Set(ref _AttributeView, value); }
		#endregion

		#region IsOkTime: Description
		/// <summary>Description</summary>
		private string _IsOkTime;
		/// <summary>Description</summary>
		public string IsOkTime { get => _IsOkTime; set => Set(ref _IsOkTime, value); }
		#endregion

		#region VisibilitySelfSign: Description
		/// <summary>Description</summary>
		private Visibility _VisibilitySelfSign = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilitySelfSign { get => _VisibilitySelfSign; set => Set(ref _VisibilitySelfSign, value); }
		#endregion


		#region VisibilityParentFind: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityParentFind = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityParentFind { get => _VisibilityParentFind; set => Set(ref _VisibilityParentFind, value); }
		#endregion

		#region StatusCertificate: Description
		/// <summary>Description</summary>
		private string _StatusCertificate;
		/// <summary>Description</summary>
		public string StatusCertificate { get => _StatusCertificate; set => Set(ref _StatusCertificate, value); }
		#endregion


		#region IsTrusted: Description
		/// <summary>Description</summary>
		private string _IsTrusted = "0";
		/// <summary>Description</summary>
		public string IsTrusted { get => _IsTrusted; set => Set(ref _IsTrusted, value); }
		#endregion

		public void Init(Certificates.Certificate certificate)
		{
			Certificate = certificate;
			UpdateAttributeView();
			VisibilitySelfSign = certificate.Info.UID == certificate.Info.MasterUID ? Visibility.Visible : Visibility.Collapsed;
			VisibilityParentFind = certificate.Info.UID != certificate.Info.MasterUID ? Visibility.Visible : Visibility.Collapsed;

			if (certificate.Info.DateDead < DateTime.Now)
			{
				StatusCertificate = "Недействительный";
				IsTrusted = "0";
				IsOkTime = "0";
				return;
			}

			try
			{
				CertificateManager.Store.Load();
				var info = CertificateManager.Store.FindMasterCertificate(certificate);
				if (info.Status == Certificates.Store.StatusSearch.NotFoundParent)
				{
					if (ClientManager.CenterCertificationsPageVM.EnableDisconnectButton)
					{
						if (CertificateManager.RCSCheckValidCertificate(info.LastParent))
						{
							StatusCertificate = "Доверенный";
							IsTrusted = "1";
							return;
						}
					}
					else
					{
						StatusCertificate = "Недоверенный";
						IsTrusted = "0";
					}
				}
				else if (info.Status == Certificates.Store.StatusSearch.Find)
				{
					StatusCertificate = "Доверенный";
					IsTrusted = "1";
					return;
				}
				else if (info.Status == Certificates.Store.StatusSearch.TimeDead)
				{
					StatusCertificate = "Недействительный";
					IsTrusted = "0";
					IsOkTime = "0";
				}
				else if (info.Status == Certificates.Store.StatusSearch.NotValid)
				{
					IsTrusted = "0";
					if (VisibilitySelfSign == Visibility.Visible)
					{
						StatusCertificate = "Измененный";
						return;
					}
					StatusCertificate = "Недоверенный";
					return;
				}
				else
				{
					StatusCertificate = "Недействительный";
					IsTrusted = "0";
				}
			}
			catch (Exception ex) { }
			StatusCertificate = "Недоверенный";
		}


		#region FindMasterCommand: Description
		private ICommand _FindMasterCommand;
		public ICommand FindMasterCommand => _FindMasterCommand ??= new LambdaCommand(OnFindMasterCommandExecuted, CanFindMasterCommandExecute);
		private bool CanFindMasterCommandExecute(object e) => true;
		private void OnFindMasterCommandExecuted(object e)
		{
			try
			{
				var root = CertificateManager.Store.GetItem(Certificate.Info.MasterUID);
				if (root == null && ClientManager.CenterCertificationsPageVM.EnableDisconnectButton == true)
				{
					Service.UI.WindowManager.ShowInfoAboutCertificate(ClientManager.RequestCertificate(Certificate.Info.MasterUID));
				}
				else
					Service.UI.WindowManager.ShowInfoAboutCertificate(root.Certificate);
			}
			catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!"); }
		}
		#endregion

		#region UploadFileFromAttributeCommand: Description
		private ICommand _UploadFileFromAttributeCommand;
		public ICommand UploadFileFromAttributeCommand => _UploadFileFromAttributeCommand ??= new LambdaCommand(OnUploadFileFromAttributeCommandExecuted, CanUploadFileFromAttributeCommandExecute);
		private bool CanUploadFileFromAttributeCommandExecute(object e) => true;
		private void OnUploadFileFromAttributeCommandExecuted(object e)
		{
			try
			{
				var att = (e as Certificates.CertificateAttribute);
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Title = "Выберите место сохранения";
				dialog.FileName = att.FileName;

				if (dialog.ShowDialog() == true)
				{
					File.WriteAllBytes(dialog.FileName, (byte[])att.Data);
				}
			}
			catch (Exception ex)
			{
				MessageBoxHelper.WarningShow("Не удалось выгрузить!");
			}
		}
		#endregion

		private void UpdateAttributeView()
		{
			AttributeView.Clear();
			foreach (var i in Certificate.Info.Attributes)
			{
				AttributeView.Add(new AttriburteView() { Attribute = i, IsChange = false });
			}
		}
	}
}
