using RCS.Certificates.Store;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RCSServer.Service.UI
{
	public class ValidTypeDescriptionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ValidType validType)
			{
				DescriptionAttribute[] attributes = (DescriptionAttribute[])validType.GetType().GetField(validType.ToString())
					.GetCustomAttributes(typeof(DescriptionAttribute), false);

				return attributes.Length > 0 ? attributes[0].Description : validType.ToString();
			}

			return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
