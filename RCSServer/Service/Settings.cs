﻿using Microsoft.Extensions.DependencyInjection;
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

		public CertificateStore CertificateStore => CertificateManager.Store;

		#region Cerificate: Description
		/// <summary>Description</summary>
		private CertificateSecret _Cerificate;
		/// <summary>Description</summary>
		public CertificateSecret Certificate { get => _Cerificate; set => Set(ref _Cerificate, value); }
		#endregion

		#region Clients: Description
		/// <summary>Description</summary>
		private ObservableCollection<Models.ServerClient> _Clients = new ObservableCollection<Models.ServerClient>();
		/// <summary>Description</summary>
		public ObservableCollection<Models.ServerClient> Clients { get => _Clients; set => Set(ref _Clients, value); }
		#endregion

		#region Parametrs: Description
		/// <summary>Description</summary>
		private SettingsParametrs _Parametrs = new SettingsParametrs();
		/// <summary>Description</summary>
		public SettingsParametrs Parametrs { get => _Parametrs; set => Set(ref _Parametrs, value); }
		#endregion
	}
}
