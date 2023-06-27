using Microsoft.Extensions.DependencyInjection;
using RCS.ViewModels.Pages.Main;
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
		private static CenterCertificationsPageVM CenterCertificationsPageVM => App.Host.Services.GetRequiredService<CenterCertificationsPageVM>();
		public delegate void CallbackOpenMenu(Page page);
		public static event CallbackOpenMenu CallbackOpenMenuEvent;
		public static void SelectMenu(Page page)
		{
			Log.WriteLine("Navigate.SelectMenu");
			MainVM.SelectedPageCreateCertificate = page == App.Host.Services.GetRequiredService<CreateCertificatePage>() ? "focus" : "";
			MainVM.SelectedPageTrustedCertificates = page == App.Host.Services.GetRequiredService<TrustedCertificatesPage>() ? "focus" : "";
			MainVM.SelectedPageCheckCertificate = page == App.Host.Services.GetRequiredService<CheckCertificatePage>() ? "focus" : "";
			MainVM.SelectedPageCenterCertifications = page == App.Host.Services.GetRequiredService<CenterCertificationsPage>() ? "focus" : "";

			MainVM.SelectedPage = page;
			CallbackOpenMenuEvent?.Invoke(page);
		}

		public static void SelectCenterCertificationsMenu(string name)
		{
			CenterCertificationsPageVM.VisibilityConnectionMenu = name == "connections" ? System.Windows.Visibility.Visible: System.Windows.Visibility.Collapsed;
			CenterCertificationsPageVM.VisibilityCheckCertificate = name == "check_cert" ? System.Windows.Visibility.Visible: System.Windows.Visibility.Collapsed;
			CenterCertificationsPageVM.VisibilityRequestCertificate = name == "req_cert" ? System.Windows.Visibility.Visible: System.Windows.Visibility.Collapsed;


			CenterCertificationsPageVM.TagSelectMenuConnections = name == "connections" ? "focus" : "";
			CenterCertificationsPageVM.TagCheckCert = name == "check_cert" ? "focus" : "";
			CenterCertificationsPageVM.TagReqCert = name == "req_cert" ? "focus" : "";
		}
	}
}
