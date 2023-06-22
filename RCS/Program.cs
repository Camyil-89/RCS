using Microsoft.Extensions.Hosting;
using RCS.Models;
using RCS.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RCS
{
	public static class Program
	{
		public static string PathToSave = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\RCS";
		[STAThread]
		public static void Main(string[] args)
		{
			Directory.SetCurrentDirectory($"{new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName}");

			var app = new App();
			app.InitializeComponent();
			Load();
			app.Exit += App_Exit;
			app.DispatcherUnhandledException += App_DispatcherUnhandledException;
			app.Run();
		}

		private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				Log.WriteError(e.Exception);
			}
			catch (Exception ex) { Log.WriteLine(ex, LogLevel.Error); }
		}

		private static void App_Exit(object sender, System.Windows.ExitEventArgs e)
		{
			try
			{
				XmlProvider.Save<SettingsParametrs>($"{PathToSave}\\settings.xml", Settings.Instance.Parametrs);
			}
			catch { }
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var builder = Host.CreateDefaultBuilder(args);
			builder.UseContentRoot(Environment.CurrentDirectory);
			builder.ConfigureServices(App.ConfigureServices);

			return builder;
		}
		private static void Load()
		{
			try
			{
				Settings.Instance.Parametrs = XmlProvider.Load<SettingsParametrs>($"{PathToSave}\\settings.xml");
			}
			catch { }
		}
	}
}
