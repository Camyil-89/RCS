using Microsoft.Win32;
using RCS.Models.Certificates.Russian;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RCS.Service.UI
{
	public static class CertificateUI
	{
		public static ObservableCollection<StackPanel> RCSGenerateUIElements(Models.Certificates.Russian.CertificateInfo info, bool IsReadOnly, bool IsDeleteButton)
		{
			ObservableCollection<StackPanel> panels = new ObservableCollection<StackPanel>();

			foreach (var i in info.Attributes)
			{
				panels.Add(RCSGenerateAttribute(i, IsReadOnly, IsDeleteButton));
			}
			return panels;
		}
		public static ObservableCollection<StackPanel> RCSGenerateUIElements(Models.Certificates.Russian.Certificate certificate, bool IsReadOnly)
		{
			ObservableCollection<StackPanel> panels = new ObservableCollection<StackPanel>();

			foreach (var i in ((Models.Certificates.Russian.CertificateInfo)certificate.Info).Attributes)
			{
				panels.Add(RCSGenerateAttribute(i, IsReadOnly, false));
			}
			return panels;
		}

		public static StackPanel RCSGenerateAttribute(Models.Certificates.Russian.CertificateAttribute attribute, bool IsReadOnly, bool IsDeleteButton)
		{
			StackPanel panel = new StackPanel();
			panel.Margin = new System.Windows.Thickness(0, 0, 0, 3);
			Grid grid = new Grid();


			var col_1 = new ColumnDefinition();
			col_1.Width = new System.Windows.GridLength(150, System.Windows.GridUnitType.Star);
			var col_2 = new ColumnDefinition();
			col_2.Width = new System.Windows.GridLength(130, System.Windows.GridUnitType.Star);

			grid.ColumnDefinitions.Add(col_1);
			grid.ColumnDefinitions.Add(col_2);

			var col_3 = new ColumnDefinition();
			if (IsDeleteButton)
			{
				col_3.Width = new System.Windows.GridLength(25, System.Windows.GridUnitType.Pixel);
				grid.ColumnDefinitions.Add(col_3);

				Button button = new Button();
				button.FontSize = 12;
				button.Tag = attribute;
				button.Height = 20;
				button.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
				button.Width = 20;
				button.Content = "X";
				grid.Children.Add(button);
				Grid.SetColumn(button, 2);
			}

			ToolTipTextBlock text = new ToolTipTextBlock();
			text.FontSize = 14;
			text.Text = attribute.Name;
			text.ToolTip = attribute.Name;
			text.TextTrimming = System.Windows.TextTrimming.CharacterEllipsis;
			text.MaxWidth = 190;
			text.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			text.VerticalAlignment = System.Windows.VerticalAlignment.Center;

			if (attribute.Type == Models.Certificates.Russian.TypeAttribute.Double ||
				attribute.Type == Models.Certificates.Russian.TypeAttribute.String)
			{
				TextBox box = new TextBox();
				box.Text = attribute.Data.ToString();
				box.FontSize = 14;
				box.IsReadOnly = IsReadOnly;
				box.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
				box.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				grid.Children.Add(box);
				Grid.SetColumn(box, 1);
			}
			if (attribute.Type == Models.Certificates.Russian.TypeAttribute.Date)
			{
				DatePicker datePicker = new DatePicker();
				datePicker.SelectedDate = (DateTime)attribute.Data;
				datePicker.FontSize = 14;
				grid.Children.Add(datePicker);
				Grid.SetColumn(datePicker, 1);
			}
			if (attribute.Type == Models.Certificates.Russian.TypeAttribute.ByteArray)
			{
				StackPanel panel1 = new StackPanel();
				Button button = new Button();
				button.Content = "Выгрузить";
				button.Click += Button_Click;
				button.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
				button.Tag = attribute;
				text.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				text.Text = $"[Файл] {attribute.Name}\n{attribute.FileName}";
				text.ToolTip = text.Text;
				try
				{
					using (MemoryStream stream = new MemoryStream((byte[])attribute.Data))
					{
						// Создаем новый экземпляр BitmapImage
						BitmapImage bitmapImage = new BitmapImage();

						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = stream;
						bitmapImage.EndInit();
						bitmapImage.Freeze();

						// Создаем новый объект Image
						Image image = new Image();
						image.Source = bitmapImage;
						image.Height = 130;

						// Добавляем Image в контейнер (например, в Grid)
						panel1.Children.Add(image);
					}
				}
				catch (Exception ex) { Console.WriteLine(ex); }

				panel1.Children.Add(button);
				grid.Children.Add(panel1);
				Grid.SetColumn(panel1, 1);
			}

			grid.Children.Add(text);
			Grid.SetColumn(text, 0);
			panel.Children.Add(grid);
			return panel;
		}

		private static void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				var att = ((sender as Button).Tag as Models.Certificates.Russian.CertificateAttribute);
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Title = "Выберите место сохранения";
				dialog.FileName = att.FileName;

				if (dialog.ShowDialog() == true)
				{
					File.WriteAllBytes(dialog.FileName, (byte[])att.Data);
				}
			}
			catch (Exception ex)
			{
				MessageBoxHelper.WarningShow("Не удалось выгрузить!");
			}
		}
	}
}
