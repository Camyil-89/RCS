using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace RCS.Service.UI
{
	public class ToolTipTextBlock : TextBlock
	{
		protected override void OnToolTipOpening(ToolTipEventArgs e)
		{
			if (TextTrimming != TextTrimming.None)
			{
				e.Handled = !IsTextTrimmed();
			}
		}

		private bool IsTextTrimmed()
		{
			var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
			var formattedText = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection, typeface, FontSize, Foreground);
			return formattedText.Width > ActualWidth;
		}
	}
}
