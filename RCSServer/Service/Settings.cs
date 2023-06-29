using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Certificates.Store;
using RCSServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.Service
{
    public class Settings : Base.ViewModel.BaseViewModel
	{
		public static Settings Instance => App.Host.Services.GetRequiredService<Settings>();


		#region Clients: Description
		/// <summary>Description</summary>
		private ObservableCollection<RCS.Net.Tcp.RCSClient> _Clients = new ObservableCollection<RCS.Net.Tcp.RCSClient>();
		/// <summary>Description</summary>
		public ObservableCollection<RCS.Net.Tcp.RCSClient> Clients { get => _Clients; set => Set(ref _Clients, value); }
		#endregion

		#region CErtificatesStore: Description
		/// <summary>Description</summary>
		private CertificateStore _CertificatesStore = new CertificateStore();
		/// <summary>Description</summary>
		public CertificateStore CertificatesStore { get => _CertificatesStore; set => Set(ref _CertificatesStore, value); }
		#endregion

		#region Parametrs: Description
		/// <summary>Description</summary>
		private SettingsParametrs _Parametrs = new SettingsParametrs();
		/// <summary>Description</summary>
		public SettingsParametrs Parametrs { get => _Parametrs; set => Set(ref _Parametrs, value); }
		#endregion
	}
}
