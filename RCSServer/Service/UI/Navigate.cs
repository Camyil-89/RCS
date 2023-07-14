using Microsoft.Extensions.DependencyInjection;
using RCS.ViewModels.Pages.Main;
using RCS.Views.Pages.Main;
using RCSServer.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RCSServer.Service.UI
{
	public static class Navigate
	{
		private static MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();
		private static ViewModels.Pages.SettingsPageVM SettingsPageVM => App.Host.Services.GetRequiredService<ViewModels.Pages.SettingsPageVM>();
		public delegate void CallbackOpenMenu(Page page);
		public static event CallbackOpenMenu CallbackOpenMenuEvent;
		public static void SelectPage(Page page)
		{
			MainVM.TagSelectClients = page == App.Host.Services.GetRequiredService<Views.Pages.ClientsPage>() ? "focus" : "";
			MainVM.TagSelectSettings = page == App.Host.Services.GetRequiredService<Views.Pages.SettingsPage>() ? "focus" : "";
			MainVM.TagTrustedCertificate = page == App.Host.Services.GetRequiredService<Views.Pages.TrustedCertificatesPage>() ? "focus" : "";
			MainVM.SelectedPage = page;

			CallbackOpenMenuEvent?.Invoke(page);
		}

		public static void SelectSettingsMenu(string name)
		{
			SettingsPageVM.VisibilityConnectionsSettings = name == "connections" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

			SettingsPageVM.TagConnectionSettings = name == "connections" ? "focus" : "";
		}
	}
}
