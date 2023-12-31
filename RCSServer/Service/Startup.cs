﻿using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Net.Packets;
using RCS.Net;
using RCS.Service;
using RCSServer.Models;
using RCSServer.Service.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RCSServer.Service
{
	public static class Startup
	{
		private static bool IsInit = false;
		private static object _lock = new object();
		public static void Init()
		{
			if (IsInit)
				return;
			IsInit = true;
			//BindingOperations.EnableCollectionSynchronization(Settings.Instance.Clients, _lock);

			Log.WriteLine($"Startup.Init", LogLevel.Warning);
			App.Current.MainWindow.Loaded += Service.Startup.MainWindow_Loaded;
			App.Current.MainWindow.Closed += MainWindow_Closed;

			NavigationCommands.BrowseBack.InputGestures.Clear();
			NavigationCommands.BrowseForward.InputGestures.Clear();
			NavigationCommands.BrowseStop.InputGestures.Clear();
			NavigationCommands.BrowseHome.InputGestures.Clear();
			NavigationCommands.DecreaseZoom.InputGestures.Clear();
			NavigationCommands.Favorites.InputGestures.Clear();
			NavigationCommands.FirstPage.InputGestures.Clear();
			NavigationCommands.GoToPage.InputGestures.Clear();
			NavigationCommands.IncreaseZoom.InputGestures.Clear();
			NavigationCommands.LastPage.InputGestures.Clear();
			NavigationCommands.NavigateJournal.InputGestures.Clear();
			NavigationCommands.NextPage.InputGestures.Clear();
			NavigationCommands.PreviousPage.InputGestures.Clear();
			NavigationCommands.Refresh.InputGestures.Clear();
			NavigationCommands.Search.InputGestures.Clear();
			NavigationCommands.Zoom.InputGestures.Clear();

			Navigate.SelectPage(App.Host.Services.GetRequiredService<Views.Pages.ClientsPage>());

			RCS.Certificates.CertificateManager.Store.PathStore = XmlProvider.PathToTrustedCertificates;
		}

		private static void MainWindow_Closed(object? sender, EventArgs e)
		{
			foreach (Window window in App.Current.Windows)
			{
				window.Close();
			}
			try
			{
				XmlProvider.Save<SettingsParametrs>($"{XmlProvider.PathToSave}\\settings.xml", RCSServer.Service.Settings.Instance.Parametrs);
			}
			catch { }
		}

		public static void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Log.WriteLine($"Startup.MainWindow_Loaded", LogLevel.Warning);
			RCS.Certificates.CertificateManager.Store.Load();
			try
			{
				RCSServer.Service.Settings.Instance.Certificate = CertificateManager.RCSLoadCertificateSecret(Settings.Instance.Parametrs.PathToCertificate);
			}
			catch { }
			Server.ServerManager.Start();
		}
	}
}
