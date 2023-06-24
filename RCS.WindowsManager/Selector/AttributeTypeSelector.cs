using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RCS.WindowsManager.Selector
{
    public class AttributeTypeSelector : DataTemplateSelector
	{
		public DataTemplate TemplateString { get; set; }
		public DataTemplate TemplateNumber { get; set; }
		public DataTemplate TemplateDate { get; set; }
		public DataTemplate TemplateByteArray { get; set; }
		// Добавьте другие DataTemplate, если необходимо

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Certificates.CertificateAttribute attribute = (item as AttriburteView).Attribute;

			if (attribute.Type == Certificates.TypeAttribute.String)
				return TemplateString;
			else if (attribute.Type == Certificates.TypeAttribute.Double)
				return TemplateNumber;
			else if (attribute.Type == Certificates.TypeAttribute.Date)
				return TemplateDate;
			else if (attribute.Type == Certificates.TypeAttribute.ByteArray)
				return TemplateByteArray;

			return base.SelectTemplate(item, container);
		}
	}
}
