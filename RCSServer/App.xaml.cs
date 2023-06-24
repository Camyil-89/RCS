using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RCSServer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static IHost __Host;
		public static IHost Host => __Host ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
		protected override async void OnStartup(StartupEventArgs e)
		{
			var host = Host;

			base.OnStartup(e);

			await host.StartAsync().ConfigureAwait(false);
		}

		protected override async void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			var host = Host;
			await host.StopAsync().ConfigureAwait(false);
			host.Dispose();
		}

		public static void ConfigureServices(HostBuilderContext builder, IServiceCollection services)
		{
			services.AddSingleton<ViewModels.Windows.MainVM>();
			services.AddSingleton<RCSServer.Service.Settings>();
		}
	}
}
