using RCS.Net.Packets;
using RCS.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RCS.Service.UI
{
	public enum TypeCloseWindow : byte
	{
		UserCancel = 0,
		OK = 1,
	}
	public class WindowAnswer : Base.ViewModel.BaseViewModel
	{

		#region TypeClose: Description
		/// <summary>Description</summary>
		private TypeCloseWindow _TypeClose = TypeCloseWindow.UserCancel;
		/// <summary>Description</summary>
		public TypeCloseWindow TypeClose { get => _TypeClose; set => Set(ref _TypeClose, value); }
		#endregion


		#region Packet: Description
		/// <summary>Description</summary>
		private BasePacket _Packet;
		/// <summary>Description</summary>
		public BasePacket Packet { get => _Packet; set => Set(ref _Packet, value); }
		#endregion
	}
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

		public static WindowAnswer ShowTCPSenderWindow(BasePacket packet, RCSTCPConnection connection)
		{
			Views.Windows.TCPSenderWindow window = new Views.Windows.TCPSenderWindow();
			ViewModels.Windows.TCPSenderWindowVM vm = new ViewModels.Windows.TCPSenderWindowVM();
			window.DataContext = vm;

			vm.Init(packet);
			vm.Window = window;
			vm.Connection = connection;
			window.ShowDialog();


			var answer = new WindowAnswer();
			answer.Packet = vm.AnswerPacket;
			answer.TypeClose = vm.UserCancel == true ? TypeCloseWindow.UserCancel : TypeCloseWindow.OK;

			if (answer.Packet != null && answer.Packet.Type == PacketType.FirewallBLock)
			{
				MessageBoxHelper.WarningShow($"Пакет данных был заблокирован брандмауэром! Скорей всего вы отправили слишком большой файл!");
			}

			return answer;
		}
	}
}
