using Microsoft.Extensions.DependencyInjection;
using RCS.ViewModels.Pages.Main;
using RCS.ViewModels.Windows;
using RCS.Views.Pages.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.ViewModels
{
	public class Locator
	{
		public MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();
		public CreateCertificatePage CreateCertificatePage => App.Host.Services.GetRequiredService<CreateCertificatePage>();
		public TrustedCertificatesPage TrustedCertificatesPage => App.Host.Services.GetRequiredService<TrustedCertificatesPage>();
		public CheckCertificatePage CheckCertificatePage => App.Host.Services.GetRequiredService<CheckCertificatePage>();
		public CenterCertificationsPage CenterCertificationsPage => App.Host.Services.GetRequiredService<CenterCertificationsPage>();

		public CreateCertificatePageVM CreateCertificatePageVM => App.Host.Services.GetRequiredService<CreateCertificatePageVM>();
		public TrustedCertificatesPageVM TrustedCertificatesPageVM => App.Host.Services.GetRequiredService<TrustedCertificatesPageVM>();
		public CheckCertificatePageVM CheckCertificatePageVM => App.Host.Services.GetRequiredService<CheckCertificatePageVM>();
		public CenterCertificationsPageVM CenterCertificationsPageVM => App.Host.Services.GetRequiredService<CenterCertificationsPageVM>();
	}
}
