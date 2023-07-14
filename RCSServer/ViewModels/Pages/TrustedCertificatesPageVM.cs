using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Certificates.Store;
using RCSServer.Base.Command;
using RCSServer.Service;
using RCSServer.Service.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCSServer.ViewModels.Pages
{

	public class TrustedCertificatesPageVM : Base.ViewModel.BaseViewModel
	{
		public TrustedCertificatesPageVM()
		{
			#region Commands
			#endregion
			Navigate.CallbackOpenMenuEvent += Navigate_CallbackOpenMenuEvent;
		}

		private void Navigate_CallbackOpenMenuEvent(System.Windows.Controls.Page page)
		{
			if (page != App.Host.Services.GetRequiredService<Views.Pages.TrustedCertificatesPage>())
				return;

			Update();
		}

		#region Parametrs
		public RCSServer.Service.Settings Settings = RCSServer.Service.Settings.Instance;
		#endregion


		#region ListCertificate: Description
		/// <summary>Description</summary>
		private ObservableCollection<StoreItem> _ListCertificate = new ObservableCollection<StoreItem>();
		/// <summary>Description</summary>
		public ObservableCollection<StoreItem> ListCertificate { get => _ListCertificate; set => Set(ref _ListCertificate, value); }
		#endregion

		#region Commands

		#region UpdateTrustedCommand: Description
		private ICommand _UpdateTrustedCommand;
		public ICommand UpdateTrustedCommand => _UpdateTrustedCommand ??= new LambdaCommand(OnUpdateTrustedCommandExecuted, CanUpdateTrustedCommandExecute);
		private bool CanUpdateTrustedCommandExecute(object e) => true;
		private void OnUpdateTrustedCommandExecuted(object e)
		{
			CertificateManager.Store.Load();
			Update();
		}
		#endregion
		#endregion

		#region Functions
		private void Update()
		{
			ListCertificate.Clear();
			foreach (var i in Settings.CertificateStore.CertificatesView)
			{
				ListCertificate.Add(i);
			}
		}
		#endregion
	}
}
