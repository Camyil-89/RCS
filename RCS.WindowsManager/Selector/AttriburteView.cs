using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RCS.WindowsManager.Selector
{
	public class AttriburteView : Base.ViewModel.BaseViewModel
	{

		#region Number: Description
		/// <summary>Description</summary>
		private double _Number;
		/// <summary>Description</summary>
		public double Number
		{
			get => _Number; set
			{
				Set(ref _Number, value);
				if (IsChange)
					this.Attribute.Data = value;
			}
		}
		#endregion


		#region Str: Description
		/// <summary>Description</summary>
		private string _Str;
		/// <summary>Description</summary>
		public string Str
		{
			get => _Str; set
			{
				Set(ref _Str, value);
				if (IsChange)
					this.Attribute.Data = value;
			}
		}
		#endregion


		#region Date: Description
		/// <summary>Description</summary>
		private DateTime _Date;
		/// <summary>Description</summary>
		public DateTime Date
		{
			get => _Date; set
			{
				Set(ref _Date, value);
				if (IsChange)
					this.Attribute.Data = value;
			}
		}
		#endregion

		public bool IsChange = true;


		#region Image: Description
		/// <summary>Description</summary>
		private BitmapImage _Image = new BitmapImage();
		/// <summary>Description</summary>
		public BitmapImage Image { get => _Image; set => Set(ref _Image, value); }
		#endregion


		#region Attribute: Description
		/// <summary>Description</summary>
		private Certificates.CertificateAttribute _Attribute;
		/// <summary>Description</summary>
		public Certificates.CertificateAttribute Attribute
		{
			get => _Attribute; set
			{
				Set(ref _Attribute, value);

				if (this.Attribute.Type == Certificates.TypeAttribute.Double)
					Number = (double)this.Attribute.Data;
				else if (this.Attribute.Type == Certificates.TypeAttribute.String)
					Str = (string)this.Attribute.Data;
				else if (this.Attribute.Type == Certificates.TypeAttribute.Date)
					Date = (DateTime)this.Attribute.Data;
				else if (this.Attribute.Type == Certificates.TypeAttribute.ByteArray)
				{
					try
					{
						Image = new BitmapImage();
						Image.BeginInit();
						Image.StreamSource = new MemoryStream((byte[])(this.Attribute.Data));
						Image.EndInit();
					}
					catch (Exception ex) { Image = new BitmapImage(); }
				}
			}
		}
		#endregion
	}
}
