using Microsoft.WindowsAPICodePack.Dialogs;
using RCS.Base.Command;
using RCS.Service;
using RCS.Service.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RCS.ViewModels.Windows
{

	public class AddAttributeWindowVM : Base.ViewModel.BaseViewModel
	{
		public AddAttributeWindowVM()
		{
			#region Commands
			#endregion
		}

		#region Parametrs

		public Window Window;

		#region IsConfirm: Description
		/// <summary>Description</summary>
		private bool _IsConfirm = false;
		/// <summary>Description</summary>
		public bool IsConfirm { get => _IsConfirm; set => Set(ref _IsConfirm, value); }
		#endregion


		#region PathFile: Description
		/// <summary>Description</summary>
		private string _PathFile;
		/// <summary>Description</summary>
		public string PathFile { get => _PathFile; set => Set(ref _PathFile, value); }
		#endregion

		#region VisibilityStringPanel: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityStringPanel = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityStringPanel { get => _VisibilityStringPanel; set => Set(ref _VisibilityStringPanel, value); }
		#endregion


		#region VisibilityByteArrayPanel: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityByteArrayPanel = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityByteArrayPanel { get => _VisibilityByteArrayPanel; set => Set(ref _VisibilityByteArrayPanel, value); }
		#endregion


		#region VisibilityDatePanel: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityDatePanel = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityDatePanel { get => _VisibilityDatePanel; set => Set(ref _VisibilityDatePanel, value); }
		#endregion

		#region VisibilityDoublePanel: Description
		/// <summary>Description</summary>
		private Visibility _VisibilityDoublePanel = Visibility.Collapsed;
		/// <summary>Description</summary>
		public Visibility VisibilityDoublePanel { get => _VisibilityDoublePanel; set => Set(ref _VisibilityDoublePanel, value); }
		#endregion


		#region Attribute: Description
		/// <summary>Description</summary>
		private Models.Certificates.Russian.CertificateAttribute _CertificateAttribute = new Models.Certificates.Russian.CertificateAttribute();
		/// <summary>Description</summary>
		public Models.Certificates.Russian.CertificateAttribute CertificateAttribute { get => _CertificateAttribute; set => Set(ref _CertificateAttribute, value); }
		#endregion
		#endregion

		#region Commands

		#region ConfirmCommand: Description
		private ICommand _ConfirmCommand;
		public ICommand ConfirmCommand => _ConfirmCommand ??= new LambdaCommand(OnConfirmCommandExecuted, CanConfirmCommandExecute);
		private bool CanConfirmCommandExecute(object e) => true;
		private void OnConfirmCommandExecuted(object e)
		{
			if (CertificateAttribute.Data == null)
			{
				MessageBoxHelper.WarningShow("У вас не указано значение поля!");
				return;
			}
			if (string.IsNullOrEmpty(CertificateAttribute.Name))
			{
				MessageBoxHelper.WarningShow("У вас не указано имя поля!");
				return;
			}

			if (CertificateAttribute.Type == Models.Certificates.Russian.TypeAttribute.Double)
			{
				try
				{
					CertificateAttribute.Data = double.Parse(CertificateAttribute.Data.ToString());
				} catch
				{
					MessageBoxHelper.WarningShow("Поле должно содержать только число! (если число дробное разделять \",\")");
					return;
				}
			}

			IsConfirm = true;
			Window.Close();
		}
		#endregion

		#region CloseCommand: Description
		private ICommand _CloseCommand;
		public ICommand CloseCommand => _CloseCommand ??= new LambdaCommand(OnCloseCommandExecuted, CanCloseCommandExecute);
		private bool CanCloseCommandExecute(object e) => true;
		private void OnCloseCommandExecuted(object e)
		{
			Window.Close();
		}
		#endregion

		#region SelectedFileCommand: Description
		private ICommand _SelectedFileCommand;
		public ICommand SelectedFileCommand => _SelectedFileCommand ??= new LambdaCommand(OnSelectedFileCommandExecuted, CanSelectedFileCommandExecute);
		private bool CanSelectedFileCommandExecute(object e) => true;
		private void OnSelectedFileCommandExecuted(object e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = false;
			dialog.InitialDirectory = XmlProvider.PathToTrustedCertificates;
			dialog.Multiselect = false;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				try
				{
					CertificateAttribute.Data = File.ReadAllBytes(dialog.FileName);
					CertificateAttribute.FileName = new FileInfo(dialog.FileName).Name;
					PathFile = dialog.FileName;
					Window.Topmost = true;
					Window.Focus();
					Window.Topmost = false;
				}
				catch (Exception ex) { MessageBoxHelper.WarningShow($"Не удалось загрузить файл!\n\n{dialog.FileName}"); }
			}
		}
		#endregion
		#endregion

		#region Functions
		public void Init(Models.Certificates.Russian.TypeAttribute Type)
		{
			VisibilityStringPanel = Type == Models.Certificates.Russian.TypeAttribute.String ? Visibility.Visible : Visibility.Collapsed;
			VisibilityDatePanel = Type == Models.Certificates.Russian.TypeAttribute.Date ? Visibility.Visible : Visibility.Collapsed;
			VisibilityDoublePanel = Type == Models.Certificates.Russian.TypeAttribute.Double ? Visibility.Visible : Visibility.Collapsed;
			VisibilityByteArrayPanel = Type == Models.Certificates.Russian.TypeAttribute.ByteArray ? Visibility.Visible : Visibility.Collapsed;

			CertificateAttribute.Type = Type;
		}
		#endregion
	}
}
