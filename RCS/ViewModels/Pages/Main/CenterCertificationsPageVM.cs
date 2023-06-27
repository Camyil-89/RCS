﻿using RCS.Base.Command;
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


		#region VisibilityMenuCertificate: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityMenuCertificate = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityMenuCertificate { get => _VisibilityMenuCertificate; set => Set(ref _VisibilityMenuCertificate, value); }
		#endregion


		#region VisibilityCheckCertificate: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityCheckCertificate = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityCheckCertificate { get => _VisibilityCheckCertificate; set => Set(ref _VisibilityCheckCertificate, value); }
		#endregion


		#region VisibilityRequestCertificate: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityRequestCertificate = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityRequestCertificate { get => _VisibilityRequestCertificate; set => Set(ref _VisibilityRequestCertificate, value); }
		#endregion



		#region TagReqCert: Description
		/// <summary>Description</summary>
		private string _TagReqCert;
		/// <summary>Description</summary>
		public string TagReqCert { get => _TagReqCert; set => Set(ref _TagReqCert, value); }
		#endregion

		#region TagCheckCert: Description
		/// <summary>Description</summary>
		private string _TagCheckCert;
		/// <summary>Description</summary>
		public string TagCheckCert { get => _TagCheckCert; set => Set(ref _TagCheckCert, value); }
		#endregion

		#region LastUpdateKeysText: Description
		/// <summary>Description</summary>
		private string _LastUpdateKeysText;
		/// <summary>Description</summary>
		public string LastUpdateKeysText { get => _LastUpdateKeysText; set => Set(ref _LastUpdateKeysText, value); }
		#endregion
		#region PingText: Description
		/// <summary>Description</summary>
		private string _PingText;
		/// <summary>Description</summary>
		public string PingText { get => _PingText; set => Set(ref _PingText, value); }
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


		#region OpenMenuCertificateCommand: Description
		private ICommand _OpenMenuCertificateCommand;
		public ICommand OpenMenuCertificateCommand => _OpenMenuCertificateCommand ??= new LambdaCommand(OnOpenMenuCertificateCommandExecuted, CanOpenMenuCertificateCommandExecute);
		private bool CanOpenMenuCertificateCommandExecute(object e) => true;
		private void OnOpenMenuCertificateCommandExecuted(object e)
		{
			VisibilityMenuCertificate = VisibilityMenuCertificate == Visibility.Collapsed ? Visibility.Visible: Visibility.Collapsed;
		}
		#endregion

		#endregion

		#region Functions
		#endregion
	}
}
