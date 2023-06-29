using Microsoft.Extensions.DependencyInjection;
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
		public delegate void CallbackOpenMenu(Page page);
		public static event CallbackOpenMenu CallbackOpenMenuEvent;
		public static void SelectPage(Page page)
		{
			MainVM.TagSelectClients = page == App.Host.Services.GetRequiredService<Views.Pages.ClientsPage>() ? "focus" : "";

			MainVM.SelectedPage = page;
			CallbackOpenMenuEvent?.Invoke(page);
		}
	}
}
