using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RCS.Service.UI
{
	public static class CertificateUI
	{
		public static ObservableCollection<StackPanel> RCSGenerateUIElements(Models.Certificates.Russian.Certificate certificate, bool IsReadOnly)
		{
			ObservableCollection<StackPanel> panels = new ObservableCollection<StackPanel>();

			foreach (var i in ((Models.Certificates.Russian.CertificateInfo)certificate.Info).Attributes)
			{
				panels.Add(RCSGenerateAttribute(i, IsReadOnly));
			}
			return panels;
		}

		public static StackPanel RCSGenerateAttribute(Models.Certificates.Russian.CertificateAttribute attribute, bool IsReadOnly)
		{
			StackPanel panel = new StackPanel();
			panel.Margin = new System.Windows.Thickness(0, 0, 0, 3);
			Grid grid = new Grid();


			var col_1 = new ColumnDefinition();
			col_1.Width = new System.Windows.GridLength(65, System.Windows.GridUnitType.Star);
			var col_2 = new ColumnDefinition();
			col_2.Width = new System.Windows.GridLength(35, System.Windows.GridUnitType.Star);

			grid.ColumnDefinitions.Add(col_1);
			grid.ColumnDefinitions.Add(col_2);

			TextBlock text = new TextBlock();
			text.FontSize = 14;
			text.Text = attribute.Name;

			if (attribute.Type == Models.Certificates.Russian.TypeAttribute.Double ||
				attribute.Type == Models.Certificates.Russian.TypeAttribute.String)
			{
				TextBox box = new TextBox();
				box.Text = attribute.Data.ToString();
				box.FontSize = 14;
				box.IsReadOnly = IsReadOnly;

				grid.Children.Add(box);
				Grid.SetColumn(box, 1);
			}

			grid.Children.Add(text);
			Grid.SetColumn(text, 0);

			panel.Children.Add(grid);

			return panel;
		}
	}
}
