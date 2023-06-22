using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RCS.Models.Certificates.Russian
{
	[XmlType(TypeName = "Секретная_информация")]
	public class CertificateSecret : Base.ViewModel.BaseViewModel
    {
        #region PrivateKey: Description
        /// <summary>Description</summary>
        private byte[] _PrivateKey = new byte[0];
		/// <summary>Description</summary>
		[XmlElement("Приватный_ключ")]
        public byte[] PrivateKey { get => _PrivateKey; set => Set(ref _PrivateKey, value); }
        #endregion


        #region Certificate: Description
        /// <summary>Description</summary>
        private Certificate _Certificate = new Certificate();
		/// <summary>Description</summary>
		[XmlElement("Сертификат")]
        public Certificate Certificate { get => _Certificate; set => Set(ref _Certificate, value); }
        #endregion


		public void SignSelf()
		{
			Certificate.Info.Master = Certificate.Info.Name;
			var bytes = Encoding.UTF8.GetBytes(Certificate.Info.Raw());
			Certificate.Sign = Sign(bytes);
		}
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
		public byte[] Sign(byte[] data)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(PrivateKey, out _);
                byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return signature;
            }
        }
		public byte[] Sign(Stream data)
		{
			using (RSA rsa = RSA.Create())
			{
				rsa.ImportRSAPrivateKey(PrivateKey, out _);
				byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return signature;
			}
		}
		public void Init(string Name, int KeySize = 2048)
		{
			Certificate.Info.Name = Name;
			using (RSA rsa = RSA.Create(KeySize))
			{
				Certificate.Info.PublicKey = rsa.ExportRSAPublicKey();
				PrivateKey = rsa.ExportRSAPrivateKey();
			}
		}
		public void Init(CertificateSecret certificateSecret, string Name, int KeySize = 2048)
		{
			Certificate.Info.Name = Name;
			using (RSA rsa = RSA.Create(KeySize))
			{
				Certificate.Info.PublicKey = rsa.ExportRSAPublicKey();
				PrivateKey = rsa.ExportRSAPrivateKey();
			}
			Certificate.Info.Master = certificateSecret.Certificate.Info.Name;
			Certificate.Sign = certificateSecret.Sign(Certificate.Info.RawByte());
		}

	}
}
