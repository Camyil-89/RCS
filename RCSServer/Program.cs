using Microsoft.Extensions.Hosting;
using RCS.Service;
using RCSServer.Models;
using RCSServer.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RCSServer
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			XmlProvider.PathToSave = $"{XmlProvider.PathToSave}\\Server";
			XmlProvider.PathToTrustedCertificates = $"{XmlProvider.PathToSave}\\TrustedCertificates";
			Directory.SetCurrentDirectory($"{new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName}");
			Directory.CreateDirectory(XmlProvider.PathToSave);
			Directory.CreateDirectory(XmlProvider.PathToTrustedCertificates);
			var app = new App();
			app.InitializeComponent();
			Load();
			app.Run();
			RCSServer.Service.Startup.Server.Stop();
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
				RCSServer.Service.Settings.Instance.Parametrs = XmlProvider.Load<SettingsParametrs>($"{XmlProvider.PathToSave}\\settings.xml");
			}
			catch { }
		}
	}
}
