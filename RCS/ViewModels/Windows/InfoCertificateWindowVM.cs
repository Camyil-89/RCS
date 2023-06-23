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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows.Input;
using RCS.Base.Command;

namespace RCS.ViewModels.Windows
{
	public class InfoCertificateWindowVM: Base.ViewModel.BaseViewModel
	{


		#region Certificate: Description
		/// <summary>Description</summary>
		private Models.Certificates.Russian.Certificate _Certificate;
		/// <summary>Description</summary>
		public Models.Certificates.Russian.Certificate Certificate { get => _Certificate; set => Set(ref _Certificate, value); }
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

		#region Panels: Description
		/// <summary>Description</summary>
		private ObservableCollection<StackPanel> _Panels = new ObservableCollection<StackPanel>();
		/// <summary>Description</summary>
		public ObservableCollection<StackPanel> Panels { get => _Panels; set => Set(ref _Panels, value); }
		#endregion

		public void Init(Models.Certificates.Russian.Certificate certificate)
		{
			Certificate = certificate;

			Panels = Service.UI.CertificateUI.RCSGenerateUIElements(certificate, true);
			
			VisibilitySelfSign = certificate.Info.UID == certificate.Info.MasterUID ? Visibility.Visible: Visibility.Collapsed;
			VisibilityParentFind = certificate.Info.UID != certificate.Info.MasterUID ? Visibility.Visible: Visibility.Collapsed;

			if (VisibilitySelfSign == Visibility.Visible)
			{
				StatusCertificate = "Доверенный";
				IsTrusted = "1";
			}
			else
			{
				try
				{
					var root = Settings.Instance.CertificateStore.FindMasterCertificate(certificate);
					if (root == null)
					{
						StatusCertificate = "Недоверенный";
						IsTrusted = "0";
						return;
					}
					else
					{
						StatusCertificate = "Доверенный";
						IsTrusted = "1";
						return;
					}
				}
				catch (Exception ex) { }
				StatusCertificate = "Недоверенный";
			}
		}


		#region FindMasterCommand: Description
		private ICommand _FindMasterCommand;
		public ICommand FindMasterCommand => _FindMasterCommand ??= new LambdaCommand(OnFindMasterCommandExecuted, CanFindMasterCommandExecute);
		private bool CanFindMasterCommandExecute(object e) => true;
		private void OnFindMasterCommandExecuted(object e)
		{
			try
			{
				var root = Settings.Instance.CertificateStore.FindMasterCertificate(Certificate);
				Service.UI.WindowManager.ShowInfoAboutCertificate(root);
			}
			catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить сертификат!"); }
		}
		#endregion
	}
}
