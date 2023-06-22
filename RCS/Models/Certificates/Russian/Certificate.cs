using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RCS.Models.Certificates.Russian
{
	[XmlType(TypeName ="Минимальная")]
	[XmlInclude(typeof(CertificateInfo))]
	public abstract class BaseCertificateInfo: Base.ViewModel.BaseViewModel
	{

		#region UID: Description
		/// <summary>Description</summary>
		private Guid _UID = Guid.NewGuid();
		/// <summary>Description</summary>
		[XmlAttribute("Идентификатор")]
		public Guid UID { get => _UID; set => Set(ref _UID, value); }
		#endregion
		#region Version: Description
		/// <summary>Description</summary>
		private int _Version = 1;
		/// <summary>Description</summary>
		[XmlAttribute("Версия_сертификата")]
		public int Version { get => _Version; set => Set(ref _Version, value); }
		#endregion


		#region DateCreate: Description
		/// <summary>Description</summary>
		private DateTime _DateCreate = DateTime.Now;
		/// <summary>Description</summary>
		[XmlAttribute("Время_создания")]
		public DateTime DateCreate { get => _DateCreate; set => Set(ref _DateCreate, value); }
		#endregion


		#region DateDead: Description
		/// <summary>Description</summary>
		private DateTime _DateDead = DateTime.Now + new TimeSpan(0,0,5);
		/// <summary>Description</summary>
		[XmlAttribute("Время_окончания")]
		public DateTime DateDead { get => _DateDead; set => Set(ref _DateDead, value); }
		#endregion

		#region PublicKey: Description
		/// <summary>Description</summary>
		private byte[] _PublicKey = new byte[0];
		/// <summary>Description</summary>
		[XmlAttribute("Публичный_ключ")]
		public byte[] PublicKey { get => _PublicKey; set => Set(ref _PublicKey, value); }
		#endregion


		#region Master: Description
		/// <summary>Description</summary>
		private string _Master;
		/// <summary>Description</summary>
		[XmlAttribute("Кем_выдан")]
		public string Master { get => _Master; set => Set(ref _Master, value); }
		#endregion


		#region Name: Description
		/// <summary>Description</summary>
		private string _Name;
		/// <summary>Description</summary>
		[XmlAttribute("Кому_выдан")]
		public string Name { get => _Name; set => Set(ref _Name, value); }
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
	}
	[XmlType(TypeName = "Базовая")]
	public class CertificateInfo : BaseCertificateInfo
	{
		#region Data: Description
		/// <summary>Description</summary>
		private string _Data = "Test";
		/// <summary>Description</summary>
		[XmlAttribute("Информация_о_сертификате")]
		public string Data { get => _Data; set => Set(ref _Data, value); }
		#endregion
	}
	[XmlType(TypeName="Сертификат")]
	public class Certificate : Base.ViewModel.BaseViewModel
    {

		#region Info: Description
		/// <summary>Description</summary>
		private BaseCertificateInfo _Info = new CertificateInfo();
		/// <summary>Description</summary>	
		[XmlElement("Информация")]		
		public BaseCertificateInfo Info { get => _Info; set => Set(ref _Info, value); }
		#endregion

		#region Sign: Description
		/// <summary>Description</summary>
		private byte[] _Sign = new byte[0];
        /// <summary>Description</summary>
        [XmlAttribute("Подпись")]
        public byte[] Sign { get => _Sign; set => Set(ref _Sign, value); }
        #endregion
		public void SaveToFile(string path)
		{
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
			using (RSA rsa = RSA.Create())
			{
				rsa.ImportRSAPublicKey(Info.PublicKey, out _);

				bool isSignatureValid = rsa.VerifyData(message, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return isSignatureValid;
			}
		}
    }
}
