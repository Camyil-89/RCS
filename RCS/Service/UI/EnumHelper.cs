using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Service.UI
{
	public class ValueDescription : Base.ViewModel.BaseViewModel
	{

		#region Value: Description
		/// <summary>Description</summary>
		private object _Value;
		/// <summary>Description</summary>
		public object Value { get => _Value; set => Set(ref _Value, value); }
		#endregion


		#region Description: Description
		/// <summary>Description</summary>
		private string _Description;
		/// <summary>Description</summary>
		public string Description { get => _Description; set => Set(ref _Description, value); }
		#endregion
	}
	public static class EnumHelper
	{
		public static string Description(this Enum value)
		{
			var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (attributes.Any())
				return (attributes.First() as DescriptionAttribute).Description;

			// If no description is found, the least we can do is replace underscores with spaces
			// You can add your own custom default formatting logic here
			TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
			return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
		}

		public static IEnumerable<KeyValuePair<string, string>> GetAllValuesAndDescriptions<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new ArgumentException("TEnum must be an Enumeration type");
			}

			return from e in Enum.GetValues(typeof(TEnum)).Cast<Enum>()
				   select new KeyValuePair<string, string>(e.ToString(), e.Description());
		}
	}
}
