using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace RCS.Certificates
{
	public enum TypeAttribute : byte
	{
		[XmlEnum("Число")]
		[Description("Число")]
		Double = 0,
		[XmlEnum("Строка")]
		[Description("Строка")]
		String = 1,
		[XmlEnum("Дата")]
		[Description("Дата")]
		Date = 2,
		[XmlEnum("Массив_байт")]
		[Description("Файл")]
		ByteArray = 3,
	}
	[XmlType(TypeName = "Поле")]
	public class CertificateAttribute : Base.ViewModel.BaseViewModel
	{

		#region Name: Description
		/// <summary>Description</summary>
		private string _Name;
		/// <summary>Description</summary>
		[XmlElement("Имя_поля", Order = 1)]
		public string Name { get => _Name; set => Set(ref _Name, value); }
		#endregion


		#region FileName: Description
		/// <summary>Description</summary>
		private string _FileName;
		/// <summary>Description</summary>
		[XmlElement("Имя_файла", Order = 2)]
		public string FileName { get => _FileName; set => Set(ref _FileName, value); }
		#endregion

		#region Type: Description
		/// <summary>Description</summary>
		private TypeAttribute _Type;
		/// <summary>Description</summary>
		[XmlElement("Тип_поля", Order = 3)]
		public TypeAttribute Type { get => _Type; set => Set(ref _Type, value); }
		#endregion

		[XmlElement("Значение", Order = 4)]
		public XmlCDataSection AttributeValueCData
		{
			get
			{
				if (Data is string)
				{
					var document = new XmlDocument();
					return document.CreateCDataSection(Data.ToString());
				}
				else if (Data is byte[])
				{
					return GetByteDataSection((byte[])Data);
				}
				else if (Data is DateTime)
				{
					return GetDateTimeDataSection((DateTime)Data);
				}
				else if (Data is double || Data is int)
				{
					return GetDoubleDataSection(double.Parse(Data.ToString()));
				}
				else
				{
					return null;
				}
				return null;
			}
			set
			{
				if (Type == TypeAttribute.String)
				{
					Data = value.Value;
				}
				else if (Type == TypeAttribute.ByteArray)
				{
					Data = GetByteDataFromSection(value);
				}
				else if (Type == TypeAttribute.Date)
				{
					Data = GetDateTimeDataFromSection(value);
				}
				else if (Type == TypeAttribute.Double)
				{
					Data = GetDoubleDataFromSection(value);
				}
			}
		}
		#region Attribute: Description
		/// <summary>Description</summary>
		private object _Data;
		/// <summary>Description</summary>
		[XmlIgnore]
		public object Data { get => _Data; set => Set(ref _Data, value); }
		#endregion


		private XmlCDataSection GetByteDataSection(byte[] data)
		{
			var document = new XmlDocument();
			return document.CreateCDataSection(Convert.ToBase64String(data));
		}

		private byte[] GetByteDataFromSection(XmlCDataSection section)
		{
			return Convert.FromBase64String(section.Value);
		}

		private XmlCDataSection GetDateTimeDataSection(DateTime dateTime)
		{
			var document = new XmlDocument();
			return document.CreateCDataSection(dateTime.ToString("yyyy-MM-ddTHH:mm:ss"));
		}

		private DateTime GetDateTimeDataFromSection(XmlCDataSection section)
		{
			return DateTime.Parse(section.Value);
		}

		private XmlCDataSection GetDoubleDataSection(double number)
		{
			var document = new XmlDocument();
			return document.CreateCDataSection(number.ToString());
		}

		private double GetDoubleDataFromSection(XmlCDataSection section)
		{
			return double.Parse(section.Value);
		}
	}
	[XmlType(TypeName = "Базовая")]
	public class CertificateInfo : Base.ViewModel.BaseViewModel
	{
		#region MasterUID: Description
		/// <summary>Description</summary>
		private Guid _MasterUID;
		/// <summary>Description</summary>
		[XmlElement("Идентификатор_родителя", Order = 1)]
		public Guid MasterUID { get => _MasterUID; set => Set(ref _MasterUID, value); }
		#endregion

		#region UID: Description
		/// <summary>Description</summary>
		private Guid _UID = Guid.NewGuid();
		/// <summary>Description</summary>
		[XmlElement("Идентификатор", Order = 2)]
		public Guid UID { get => _UID; set => Set(ref _UID, value); }
		#endregion
		#region Version: Description
		/// <summary>Description</summary>
		private int _Version = 1;
		/// <summary>Description</summary>
		[XmlElement("Версия_сертификата", Order = 3)]
		public int Version { get => _Version; set => Set(ref _Version, value); }
		#endregion


		#region DateCreate: Description
		/// <summary>Description</summary>
		private DateTime _DateCreate = DateTime.Now;
		/// <summary>Description</summary>
		[XmlElement("Время_создания", Order = 4)]
		public DateTime DateCreate { get => _DateCreate; set => Set(ref _DateCreate, value); }
		#endregion


		#region DateDead: Description
		/// <summary>Description</summary>
		private DateTime _DateDead = DateTime.Now + new TimeSpan(180, 0, 0, 0, 0);
		/// <summary>Description</summary>
		[XmlElement("Время_окончания", Order = 5)]
		public DateTime DateDead { get => _DateDead; set => Set(ref _DateDead, value); }
		#endregion

		#region PublicKey: Description
		/// <summary>Description</summary>
		private byte[] _PublicKey = new byte[0];
		/// <summary>Description</summary>
		[XmlElement("Публичный_ключ", Order = 6)]
		public byte[] PublicKey { get => _PublicKey; set => Set(ref _PublicKey, value); }
		#endregion


		#region Master: Description
		/// <summary>Description</summary>
		private string _Master;
		/// <summary>Description</summary>
		[XmlElement("Кем_выдан", Order = 7)]
		public string Master { get => _Master; set => Set(ref _Master, value); }
		#endregion


		#region Name: Description
		/// <summary>Description</summary>
		private string _Name;
		/// <summary>Description</summary>
		[XmlElement("Кому_выдан", Order = 8)]
		public string Name { get => _Name; set => Set(ref _Name, value); }
		#endregion

		#region Attributes: Description
		/// <summary>Description</summary>
		private ObservableCollection<CertificateAttribute> _Attributes = new ObservableCollection<CertificateAttribute>();
		/// <summary>Description</summary>
		[XmlArray("Доступная_информация", Order = 9)]
		public ObservableCollection<CertificateAttribute> Attributes { get => _Attributes; set => Set(ref _Attributes, value); }
		#endregion
		public virtual byte[] RawByte()
		{
			XmlSerializer serializer = new XmlSerializer(this.GetType());
			MemoryStream memoryStream = new MemoryStream();
			serializer.Serialize(memoryStream, this);
			var data = Encoding.UTF8.GetString(memoryStream.ToArray());
			memoryStream.Close();
			return Encoding.UTF8.GetBytes(data);
		}
		public virtual string Raw()
		{
			XmlSerializer serializer = new XmlSerializer(this.GetType());
			MemoryStream memoryStream = new MemoryStream();
			serializer.Serialize(memoryStream, this);
			var data = Encoding.UTF8.GetString(memoryStream.ToArray());
			memoryStream.Close();
			return data;
		}

		public void AddAttribute(CertificateAttribute attribute)
		{
			if (ContainsAttribute(attribute))
				throw new Exception("Такое поле уже существует!");
			Attributes.Add(attribute);
		}
		public bool ContainsAttribute(CertificateAttribute attribute)
		{
			return Attributes.FirstOrDefault((i) => i.Name == attribute.Name) != null;
		}
	}
	[XmlType(TypeName = "Сертификат")]
	public class Certificate : Base.ViewModel.BaseViewModel
	{

		#region Info: Description
		/// <summary>Description</summary>
		private CertificateInfo _Info = new CertificateInfo();
		/// <summary>Description</summary>	
		[XmlElement("Информация", Order = 1)]
		public CertificateInfo Info { get => _Info; set => Set(ref _Info, value); }
		#endregion

		#region Sign: Description
		/// <summary>Description</summary>
		private byte[] _Sign = new byte[0];
		/// <summary>Description</summary>
		[XmlElement("Подпись", Order = 2)]
		public byte[] Sign { get => _Sign; set => Set(ref _Sign, value); }
		#endregion

		#region LengthKey: Description
		/// <summary>Description</summary>
		private int _LengthKey = 2048;
		/// <summary>Description</summary>
		[XmlElement("Длина_ключа", Order = 3)]
		public int LengthKey { get => _LengthKey; set => Set(ref _LengthKey, value); }
		#endregion
		public void SaveToFile(string path)
		{
			if (path.EndsWith(".сертификат") == false)
				path += ".сертификат";
			using (StreamWriter sw = new StreamWriter(path))
			{
				XmlSerializer xmls = new XmlSerializer(this.GetType());
				xmls.Serialize(sw, this);
			}
		}
		public string Raw()
		{
			XmlSerializer serializer = new XmlSerializer(this.GetType());
			MemoryStream memoryStream = new MemoryStream();
			serializer.Serialize(memoryStream, this);
			var data = Encoding.UTF8.GetString(memoryStream.ToArray());
			memoryStream.Close();
			return data;
		}
		public byte[] RawByte()
		{
			XmlSerializer serializer = new XmlSerializer(this.GetType());
			MemoryStream memoryStream = new MemoryStream();
			serializer.Serialize(memoryStream, this);
			var data = Encoding.UTF8.GetString(memoryStream.ToArray());
			memoryStream.Close();
			return Encoding.UTF8.GetBytes(data);
		}
		public bool Verify(byte[] message, byte[] sign)
		{
			using (RSA rsa = RSA.Create(LengthKey))
			{
				rsa.ImportRSAPublicKey(Info.PublicKey, out _);

				bool isSignatureValid = rsa.VerifyData(message, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return isSignatureValid;
			}
		}
		public bool Verify(Stream message, byte[] sign)
		{
			using (RSA rsa = RSA.Create(LengthKey))
			{
				rsa.ImportRSAPublicKey(Info.PublicKey, out _);

				bool isSignatureValid = rsa.VerifyData(message, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return isSignatureValid;
			}
		}
	}
}
