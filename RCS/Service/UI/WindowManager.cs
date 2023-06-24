using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RCS.Service.UI
{
	public static class WindowManager
	{
		public static void ShowInfoAboutCertificate(Certificates.Certificate certificate)
		{
			Views.Windows.InfoCertificateWindow window = new Views.Windows.InfoCertificateWindow();

			ViewModels.Windows.InfoCertificateWindowVM vm = new ViewModels.Windows.InfoCertificateWindowVM();
			window.DataContext = vm;


			vm.Init(certificate);

			window.Show();
		}
		public static Certificates.CertificateAttribute ShowAddAttributeWindow(Certificates.TypeAttribute type)
		{
			Views.Windows.AddAttributeWindow window = new Views.Windows.AddAttributeWindow();
			ViewModels.Windows.AddAttributeWindowVM vm = new ViewModels.Windows.AddAttributeWindowVM();
			window.DataContext = vm;

			vm.Init(type);
			vm.Window = window;

			window.ShowDialog();
			if (vm.IsConfirm)
				return vm.CertificateAttribute;
			else
				return null;
		}
	}
}
