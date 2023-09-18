using RCS.Base.Command;
using RCS.Net.Packets;
using RCS.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices.ObjectiveC;
using EasyTCP;
using RCS.Service.UI;

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


		private object SendPacket = null;
		public object AnswerPacket = null;

		public EasyTCP.Client Client;

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
			//Connection.RemoveWaitPacket(SendPacket.UID);
			Window.Close();
		}
		#endregion
		#endregion

		#region Functions
		public void Init<T>(object packet)
		{
			SendPacket = packet;
			Task.Run(() =>
			{
				Send<T>();
			});
		}
		private static string FormatBytesPerSecond(double bytesPerSecond)
		{
			string[] suffixes = { "Б/с", "КБ/с", "МБ/с", "ГБ/с", "ТБ/с" };
			int suffixIndex = 0;

			while (bytesPerSecond >= 1024 && suffixIndex < suffixes.Length - 1)
			{
				bytesPerSecond /= 1024;
				suffixIndex++;
			}

			return $"{bytesPerSecond:0.##} {suffixes[suffixIndex]}";
		}
		private void Send<T>()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			int RSTStopwatch = 0;
			InfoProgress = "Отправка пакета...";

			try
			{
				foreach (var i in Client.SendAndReceiveInfo<T>((T)SendPacket))
				{
					if (i.Packet != null)
					{
						AnswerPacket = i.Packet;
						Window.Dispatcher.Invoke(() => { Window.Close(); });
						return;
					}
					else if (i.ReceiveFromServer)
					{
						InfoProgress = $"Получено: {FormatBytesPerSecond(i.Info.Receive)} \\ {FormatBytesPerSecond(i.Info.TotalNeedReceive)} ({Math.Round(stopwatch.Elapsed.TotalSeconds, 1)} сек.)";

					}
					else
					{
						InfoProgress = $"Отправлено: {FormatBytesPerSecond(i.Info.Receive)} \\ {FormatBytesPerSecond(i.Info.TotalNeedReceive)} ({Math.Round(stopwatch.Elapsed.TotalSeconds, 1)} сек.)";
					}
				}
			} catch (Exception ex)
			{
				MessageBoxHelper.WarningShow($"Не удалось получить ответ от сервера!\n{ex.Message}");
				Window.Dispatcher.Invoke(() => { Window.Close(); });
			}

			//var info = Connection.SendAndWaitUnlimited(SendPacket);
			//InfoProgress = "Пакет отправлен!";
			//while (UserCancel == false)
			//{
			//	if (info.Packet != null)
			//	{
			//		AnswerPacket = info.Packet;
			//		Connection.RemoveWaitPacket(info.Packet.UID);
			//		Window.Dispatcher.Invoke(() => { Window.Close(); });
			//		break;
			//	}
			//	if (info.RSTStopwatch)
			//	{
			//		info.RSTStopwatch = false;
			//		RSTStopwatch++;
			//	}
			//	Thread.Sleep(1);
			//	if (RSTStopwatch > 0)
			//		InfoProgress = $"Осталось совсем немного ({Math.Round(stopwatch.Elapsed.TotalSeconds, 1)} сек.)...";
			//	else
			//		InfoProgress = $"Ожидайте ответа ({Math.Round(stopwatch.Elapsed.TotalSeconds, 1)} сек.)...";
			//}
		}
		#endregion
	}
}
