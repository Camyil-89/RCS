using RCS.Base.Command;
using RCS.Net.Packets;
using RCS.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCS.ViewModels.Windows
{

	public class TCPSenderWindowVM : Base.ViewModel.BaseViewModel
	{
		public TCPSenderWindowVM()
		{
			#region Commands
			#endregion
		}

		#region Parametrs
		public Views.Windows.TCPSenderWindow Window;

		#region InfoProgress: Description
		/// <summary>Description</summary>
		private string _InfoProgress;
		/// <summary>Description</summary>
		public string InfoProgress { get => _InfoProgress; set => Set(ref _InfoProgress, value); }
		#endregion


		private BasePacket SendPacket = null;
		public BasePacket AnswerPacket = null;

		public RCSTCPConnection Connection;

		#region UserCancel: Description
		/// <summary>Description</summary>
		private bool _UserCancel = false;
		/// <summary>Description</summary>
		public bool UserCancel { get => _UserCancel; set => Set(ref _UserCancel, value); }
		#endregion
		#endregion

		#region Commands

		#region CancelCommand: Description
		private ICommand _CancelCommand;
		public ICommand CancelCommand => _CancelCommand ??= new LambdaCommand(OnCancelCommandExecuted, CanCancelCommandExecute);
		private bool CanCancelCommandExecute(object e) => true;
		private void OnCancelCommandExecuted(object e)
		{
			UserCancel = true;
			Connection.RemoveWaitPacket(SendPacket.UID);
			Window.Close();
		}
		#endregion
		#endregion

		#region Functions
		public void Init(BasePacket packet)
		{
			SendPacket = packet;
			Task.Run(Send);
		}
		private void Send()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			int RSTStopwatch = 0;
			InfoProgress = "Отправка пакета...";
			var info = Connection.SendAndWaitUnlimited(SendPacket);
			InfoProgress = "Пакет отправлен!";
			while (UserCancel == false)
			{
				if (info.Packet != null)
				{
					AnswerPacket = info.Packet;
					Connection.RemoveWaitPacket(info.Packet.UID);
					Window.Dispatcher.Invoke(() => { Window.Close(); });
					break;
				}
				if (info.RSTStopwatch)
				{
					info.RSTStopwatch = false;
					RSTStopwatch++;
				}
				Thread.Sleep(1);
				if (RSTStopwatch > 0)
					InfoProgress = $"Осталось совсем немного ({Math.Round(stopwatch.Elapsed.TotalSeconds, 1)} сек.)...";
				else
					InfoProgress = $"Ожидайте ответа ({Math.Round(stopwatch.Elapsed.TotalSeconds, 1)} сек.)...";
			}
		}
		#endregion
	}
}
