using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates.Store;
using RCS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Service
{
    public class Settings : Base.ViewModel.BaseViewModel
	{
		public static Settings Instance => App.Host.Services.GetRequiredService<Settings>();




		#region CertificateStore: Description
		/// <summary>Description</summary>
		private CertificateStore _CertificateStore = new CertificateStore();
		/// <summary>Description</summary>
		public CertificateStore CertificateStore { get => _CertificateStore; set => Set(ref _CertificateStore, value); }
		#endregion

		#region Parametrs: Description
		/// <summary>Description</summary>
		private SettingsParametrs _Parametrs = new SettingsParametrs();
		/// <summary>Description</summary>
		public SettingsParametrs Parametrs { get => _Parametrs; set => Set(ref _Parametrs, value); }
		#endregion
	}
}
