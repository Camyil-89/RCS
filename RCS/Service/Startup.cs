﻿using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Models;
using RCS.Service.UI;
using RCS.Service.UI.Client;
using RCS.Views.Pages.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RCS.Service
{
	public static class Startup
	{
		private static bool IsInit = false;
		public static void Init()
		{
			if (IsInit)
				return;
			IsInit = true;

			try
			{
				//Service.Certificate.CertificateProvider.Test();
			}
			catch (Exception ex) { Console.WriteLine(ex); }


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

			Navigate.SelectMenu(App.Host.Services.GetRequiredService<CreateCertificatePage>());

			//var cert = XmlProvider.Load<Certificate>("C:\\Users\\zhuko\\Documents\\RCS\\Сертификат.сертификат");
			//Console.WriteLine(cert.Info.Raw());
			//Console.WriteLine($"{cert.LengthKey};{cert.Info.RawByte().Length}");
			//Console.WriteLine($"{string.Join(";", cert.Info.RawByte())}");
			//Console.WriteLine($"{cert.Verify(cert.Info.RawByte(), cert.Sign)}");
		}

		private static void MainWindow_Closed(object? sender, EventArgs e)
		{
			foreach (Window window in App.Current.Windows)
			{
				window.Close();
			}
		}

		public static void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Log.WriteLine($"Startup.MainWindow_Loaded", LogLevel.Warning);
			CertificateManager.TCPSender = new TCPSender();
			CertificateManager.Store.Load();
			try
			{
				Settings.Instance.Certificate = CertificateManager.RCSLoadCertificate(Settings.Instance.Parametrs.PathToCertificate);
			} catch { }
			if (Settings.Instance.Parametrs.Client.AutoStartClient)
				ClientManager.Connect();
		}
	}
}
