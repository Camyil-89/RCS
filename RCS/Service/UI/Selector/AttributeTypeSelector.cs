using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RCS.Service.UI.Selector
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
			Models.Certificates.Russian.CertificateAttribute attribute = (item as Service.UI.Selector.AttriburteView).Attribute;

			if (attribute.Type == Models.Certificates.Russian.TypeAttribute.String)
				return TemplateString;
			else if (attribute.Type == Models.Certificates.Russian.TypeAttribute.Double)
				return TemplateNumber;
			else if (attribute.Type == Models.Certificates.Russian.TypeAttribute.Date)
				return TemplateDate;
			else if (attribute.Type == Models.Certificates.Russian.TypeAttribute.ByteArray)
				return TemplateByteArray;

			return base.SelectTemplate(item, container);
		}
	}
}
