using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Service.UI
{
	public static class WindowManager
	{
		public static void ShowInfoAboutCertificate(Models.Certificates.Russian.Certificate certificate)
		{
			Views.Windows.InfoCertificateWindow window = new Views.Windows.InfoCertificateWindow();

			ViewModels.Windows.InfoCertificateWindowVM vm = new ViewModels.Windows.InfoCertificateWindowVM();
			window.DataContext = vm;


			vm.Init(certificate);

			window.Show();
		}
	}
}
