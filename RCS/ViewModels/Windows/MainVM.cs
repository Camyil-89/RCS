using Microsoft.Extensions.DependencyInjection;
using RCS.Base.Command;
using RCS.Models.Certificates.Russian;
using RCS.Service;
using RCS.Service.UI;
using RCS.ViewModels.Pages.Main;
using RCS.Views.Pages.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace RCS.ViewModels.Windows
{
	public class MainVM : Base.ViewModel.BaseViewModel
	{
		public MainVM()
		{
			#region Commands
			#endregion
			Startup.Init();
		}

		#region Parametrs


		#region SelectedPageCheckCertificate: Description
		/// <summary>Description</summary>
		private string _SelectedPageCheckCertificate;
		/// <summary>Description</summary>
		public string SelectedPageCheckCertificate { get => _SelectedPageCheckCertificate; set => Set(ref _SelectedPageCheckCertificate, value); }
		#endregion

		#region SelectedPageCreateCertificate: Description
		/// <summary>Description</summary>
		private string _SelectedPageCreateCertificate;
		/// <summary>Description</summary>
		public string SelectedPageCreateCertificate { get => _SelectedPageCreateCertificate; set => Set(ref _SelectedPageCreateCertificate, value); }
		#endregion


		#region SelectedPageTrustedCertificates: Description
		/// <summary>Description</summary>
		private string _SelectedPageTrustedCertificates;
		/// <summary>Description</summary>
		public string SelectedPageTrustedCertificates { get => _SelectedPageTrustedCertificates; set => Set(ref _SelectedPageTrustedCertificates, value); }
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
			Navigate.SelectMenu((Page)e);
		}
		#endregion
		#endregion

		#region Functions
		#endregion
	}
}
