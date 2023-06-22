using Microsoft.Extensions.DependencyInjection;
using RCS.ViewModels.Windows;
using RCS.Views.Pages.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RCS.Service.UI
{
    public static class Navigate
    {
		private static MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();
		public delegate void CallbackOpenMenu(Page page);
		public static event CallbackOpenMenu CallbackOpenMenuEvent;
		public static void SelectMenu(Page page)
		{
			Log.WriteLine("Navigate.SelectMenu");
			MainVM.SelectedPageCreateCertificate = page == App.Host.Services.GetRequiredService<CreateCertificatePage>() ? "focus" : "";
			MainVM.SelectedPageTrustedCertificates = page == App.Host.Services.GetRequiredService<TrustedCertificatesPage>() ? "focus" : "";
			MainVM.SelectedPageCheckCertificate = page == App.Host.Services.GetRequiredService<CheckCertificatePage>() ? "focus" : "";

			MainVM.SelectedPage = page;
			CallbackOpenMenuEvent?.Invoke(page);
		}
	}
}
