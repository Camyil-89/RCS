using RCS.Base.Command;
using RCS.Service;
using RCS.Service.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RCS.ViewModels.Pages.Main
{

	public class CenterCertificationsPageVM : Base.ViewModel.BaseViewModel
	{
		public CenterCertificationsPageVM()
		{
			#region Commands
			#endregion
		}

		#region Parametrs
		public Settings Settings => Settings.Instance;
		#region VisibilityConnectionMenu: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityConnectionMenu = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityConnectionMenu { get => _VisibilityConnectionMenu; set => Set(ref _VisibilityConnectionMenu, value); }
		#endregion
		#endregion


		#region TagSelectMenuConnections: Description
		/// <summary>Description</summary>
		private string _TagSelectMenuConnections;
		/// <summary>Description</summary>
		public string TagSelectMenuConnections { get => _TagSelectMenuConnections; set => Set(ref _TagSelectMenuConnections, value); }
		#endregion


		#region ClientStatusText: Description
		/// <summary>Description</summary>
		private string _ClientStatusText;
		/// <summary>Description</summary>
		public string ClientStatusText { get => _ClientStatusText; set => Set(ref _ClientStatusText, value); }
		#endregion


		#region EnableConnectButton: Description
		/// <summary>Description</summary>
		private bool _EnableConnectButton = true;
		/// <summary>Description</summary>
		public bool EnableConnectButton { get => _EnableConnectButton; set => Set(ref _EnableConnectButton, value); }
		#endregion


		#region EnableDisconnectButton: Description
		/// <summary>Description</summary>
		private bool _EnableDisconnectButton = false;
		/// <summary>Description</summary>
		public bool EnableDisconnectButton { get => _EnableDisconnectButton; set => Set(ref _EnableDisconnectButton, value); }
		#endregion
		#region Commands

		#region SelectMenuCommand: Description
		private ICommand _SelectMenuCommand;
		public ICommand SelectMenuCommand => _SelectMenuCommand ??= new LambdaCommand(OnSelectMenuCommandExecuted, CanSelectMenuCommandExecute);
		private bool CanSelectMenuCommandExecute(object e) => true;
		private void OnSelectMenuCommandExecuted(object e)
		{
			Navigate.SelectCenterCertificationsMenu(e.ToString());
		}
		#endregion


		#region DisconnectCommand: Description
		private ICommand _DisconnectCommand;
		public ICommand DisconnectCommand => _DisconnectCommand ??= new LambdaCommand(OnDisconnectCommandExecuted, CanDisconnectCommandExecute);
		private bool CanDisconnectCommandExecute(object e) => true;
		private void OnDisconnectCommandExecuted(object e)
		{
			Service.UI.Client.ClientManager.Disconnect();
		}
		#endregion

		#region ConnectCommand: Description
		private ICommand _ConnectCommand;
		public ICommand ConnectCommand => _ConnectCommand ??= new LambdaCommand(OnConnectCommandExecuted, CanConnectCommandExecute);
		private bool CanConnectCommandExecute(object e) => true;
		private void OnConnectCommandExecuted(object e)
		{
			Service.UI.Client.ClientManager.Connect();
		}
		#endregion


		#region TestCommand: Description
		private ICommand _TestCommand;
		public ICommand TestCommand => _TestCommand ??= new LambdaCommand(OnTestCommandExecuted, CanTestCommandExecute);
		private bool CanTestCommandExecute(object e) => true;
		private void OnTestCommandExecuted(object e)
		{
			try
			{
				foreach (var i in Service.UI.Client.ClientManager.GetLastCertificates())
				{
					Console.WriteLine(i);
					i.SaveToFile($"{XmlProvider.PathToTrustedCertificates}\\{i.Info.UID}");
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
		}
		#endregion
		#endregion

		#region Functions
		#endregion
	}
}
