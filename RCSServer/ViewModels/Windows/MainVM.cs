using RCSServer.Base.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace RCSServer.ViewModels.Windows
{
	public class MainVM : Base.ViewModel.BaseViewModel
	{
		public MainVM()
		{
			#region Commands
			#endregion
			Service.Startup.Init();
		}

		#region Parametrs

		#region TagSelectClients: Description
		/// <summary>Description</summary>
		private string _TagSelectClients;
		/// <summary>Description</summary>
		public string TagSelectClients { get => _TagSelectClients; set => Set(ref _TagSelectClients, value); }
		#endregion

		#region SelectedPage: Description
		/// <summary>Description</summary>
		private Page _SelectedPage;
		/// <summary>Description</summary>
		public Page SelectedPage { get => _SelectedPage; set => Set(ref _SelectedPage, value); }
		#endregion
		#endregion

		#region Commands

		#region SelectPageCommand: Description
		private ICommand _SelectPageCommand;
		public ICommand SelectPageCommand => _SelectPageCommand ??= new LambdaCommand(OnSelectPageCommandExecuted, CanSelectPageCommandExecute);
		private bool CanSelectPageCommandExecute(object e) => true;
		private void OnSelectPageCommandExecuted(object e)
		{

		}
		#endregion
		#endregion

		#region Functions
		#endregion
	}
}
