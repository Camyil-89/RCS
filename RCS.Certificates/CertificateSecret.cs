using RCS.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RCS.Certificates
{
	[XmlType(TypeName = "Секретная_информация")]
	[Serializable]
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

		public void SignZipFile(string path)
		{
			XmlProvider.DeleteEntryzip(path, XmlProvider.NameFileCertificateInZip);
			XmlProvider.DeleteEntryzip(path, XmlProvider.NameFileCertificateSignInZip);

			var stream = new FileStream(path, FileMode.Open);
			var sign = Sign(stream);
			stream.Close();
			XmlProvider.SaveInzip<Certificates.Certificate>(path, XmlProvider.NameFileCertificateInZip, Certificate);
			XmlProvider.WriteInZip(path, XmlProvider.NameFileCertificateSignInZip, sign);
		}
		public void SignSelf()
		{
			Certificate.Info.Master = Certificate.Info.Name;
			Certificate.Info.MasterUID = Certificate.Info.UID;
			var bytes = Encoding.UTF8.GetBytes(Certificate.Info.Raw());
			Certificate.Sign = Sign(bytes);
		}
		public void SaveToFile(string path)
		{
			if (path.EndsWith(".ссертификат") == false)
				path += ".ссертификат";
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
            using (RSA rsa = RSA.Create(Certificate.LengthKey))
            {
                rsa.ImportRSAPrivateKey(PrivateKey, out _);
                byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return signature;
            }
        }
		public byte[] Sign(Stream data)
		{
			using (RSA rsa = RSA.Create(Certificate.LengthKey))
			{
				rsa.ImportRSAPrivateKey(PrivateKey, out _);
				byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				return signature;
			}
		}
		public void Init(string Name, int KeySize = 2048)
		{
			Certificate.Info.Name = Name;
			Certificate.LengthKey= KeySize;
			using (RSA rsa = RSA.Create(Certificate.LengthKey))
			{
				RSAParameters key = rsa.ExportParameters(true);
				key.Exponent = new byte[] { 1, 0, 1 }; // 65537

				rsa.ImportParameters(key);

				Certificate.Info.PublicKey = rsa.ExportRSAPublicKey();
				PrivateKey = rsa.ExportRSAPrivateKey();
			}
		}
		public void Init(CertificateSecret certificateSecret, int KeySize = 2048)
		{
			Certificate.LengthKey = KeySize;
			using (RSA rsa = RSA.Create(Certificate.LengthKey))
			{
				RSAParameters key = rsa.ExportParameters(true);
				key.Exponent = new byte[] { 1, 0, 1 }; // 65537

				rsa.ImportParameters(key);

				Certificate.Info.PublicKey = rsa.ExportRSAPublicKey();
				PrivateKey = rsa.ExportRSAPrivateKey();
			}
			Certificate.Sign = certificateSecret.Sign(Certificate.Info.RawByte());
		}

	}
}
