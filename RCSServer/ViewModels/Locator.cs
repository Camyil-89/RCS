using Microsoft.Extensions.DependencyInjection;
using RCSServer.ViewModels.Pages;
using RCSServer.ViewModels.Windows;
using RCSServer.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.ViewModels
{
	public class Locator
	{
		public MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();

		public RCSServer.ViewModels.Pages.ClientsPageVM ClientsPageVM => App.Host.Services.GetRequiredService<RCSServer.ViewModels.Pages.ClientsPageVM>();

		public ClientsPage ClientsPage => App.Host.Services.GetRequiredService<ClientsPage>();
	}
}
