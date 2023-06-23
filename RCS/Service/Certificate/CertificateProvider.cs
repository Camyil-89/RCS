using RCS.Models.Certificates;
using RCS.Models.Certificates.Russian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCS.Service.Certificate
{
	public static class CertificateProvider
	{
		public static CertificateSecret RCSCreateCertificate(CreateSettingsCertificate settings)
		{
			if (settings.MasterCertificate == null)
			{
				Log.WriteLine("RCSCreateCertificate.SELF", LogLevel.Error);
				var cert = new Models.Certificates.Russian.CertificateSecret();
				cert.Certificate.Info = settings.Info;
				cert.Init(settings.Name, settings.SizeKey);
				cert.SignSelf();
				return cert;
			}
			else
			{
				Log.WriteLine("RCSCreateCertificate.MASTER", LogLevel.Error);
				var cert = new Models.Certificates.Russian.CertificateSecret();
				cert.Certificate.Info = settings.Info;
				cert.Init(settings.MasterCertificate, settings.SizeKey);
				return cert;
			}

		}
		public static bool RCSCheckSignFile(string path)
		{
			Models.Certificates.Russian.Certificate cert;
			byte[] sign;
			try
			{
				cert = XmlProvider.LoadInzip<Models.Certificates.Russian.Certificate>(path, XmlProvider.NameFileCertificateInZip);
				sign = XmlProvider.ReadInZip(path, XmlProvider.NameFileCertificateSignInZip);

			}
			catch (NullReferenceException) { return false; }
			XmlProvider.DeleteEntryzip(path, XmlProvider.NameFileCertificateInZip);
			XmlProvider.DeleteEntryzip(path, XmlProvider.NameFileCertificateSignInZip);

			FileStream stream = new FileStream(path, FileMode.Open);

			var check = cert.Verify(stream, sign);
			stream.Close();

			XmlProvider.WriteInZip(path, XmlProvider.NameFileCertificateSignInZip, sign);
			XmlProvider.SaveInzip(path, XmlProvider.NameFileCertificateInZip, cert);
			return check;
		}
		public static Models.Certificates.Russian.Certificate RCSLoadCertificateFromZip(string path)
		{
			return XmlProvider.LoadInzip<Models.Certificates.Russian.Certificate>(path, XmlProvider.NameFileCertificateInZip);
		}
		public static Models.Certificates.Russian.Certificate RCSLoadCertificate(string path)
		{
			return XmlProvider.Load<Models.Certificates.Russian.Certificate>(path);
		}
		public static CertificateSecret RCSLoadCertificateSecret(string path)
		{
			return XmlProvider.Load<CertificateSecret>(path);
		}
		public static bool VerifyCertificate(Models.Certificates.Russian.Certificate master, Models.Certificates.Russian.Certificate slave)
		{
			if (slave.Info.DateDead > DateTime.Now)
				return false;
			return master.Verify(slave.Info.RawByte(), slave.Sign);
		}

		public static void Test()
		{
			//var g = RCSLoadCertificateSecret("test.секретный.сертификат");
			//foreach (var i in ((CertificateInfo)g.Certificate.Info).Attributes)
			//{
			//	Console.WriteLine($">{i.Type};{i.Name};{i.Data}");
			//	if (i.Type == TypeAttribute.ByteArray)
			//	{
			//		Console.WriteLine($"{string.Join(",", (byte[])i.Data)}");
			//	}
			//	if (i.Type == TypeAttribute.Date)
			//	{
			//		Console.WriteLine($"{(DateTime)i.Data}");
			//	}
			//}

			var x = new CreateSettingsCertificate();
			var info = new CertificateInfo();

			info.Attributes.Add(new CertificateAttribute() { Data = "Жуков", Name = "Фамилия", Type = TypeAttribute.String });
			info.Attributes.Add(new CertificateAttribute() { Data = "Кирилл", Name = "Имя", Type = TypeAttribute.String });
			info.Attributes.Add(new CertificateAttribute() { Data = "Дмитриевич", Name = "Отчество", Type = TypeAttribute.String });
			info.Attributes.Add(new CertificateAttribute() { Data = 2.1, Name = "Число", Type = TypeAttribute.Double });
			info.Attributes.Add(new CertificateAttribute() { Data = 1, Name = "Число", Type = TypeAttribute.Double });
			info.Attributes.Add(new CertificateAttribute() { Data = DateTime.Now, Name = "3", Type = TypeAttribute.Date });
			info.Attributes.Add(new CertificateAttribute() { Data = new byte[] { 0, 1, 2, 3 }, Name = "2", Type = TypeAttribute.ByteArray });

			x.Info = info;
			var cert_1 = RCSCreateCertificate(x);
			//cert_1.SaveToFile("test");
			cert_1.Certificate.SaveToFile("test");

			var cert_2 = RCSCreateCertificate(new CreateSettingsCertificate() { MasterCertificate = cert_1 });
			Console.WriteLine(cert_1.Certificate.Verify(cert_2.Certificate.Info.RawByte(), cert_2.Certificate.Sign));

			return;
			//var cert_1 = RCSCreateCertificate(new CreateSettingsCertificate());
			//
			//cert_1.SaveToFile("root");
			//cert_1.Certificate.SaveToFile("root");
			//
			//var x = cert_1.Certificate.Info.Raw();
			//var cert_2 = RCSCreateCertificate(new CreateSettingsCertificate() { MasterCertificate = cert_1});
			//
			//var cert_3 = RCSCreateCertificate(new CreateSettingsCertificate() { MasterCertificate = cert_2 });
			//
			//
			//cert_3.SignZipFile("test.zip");
			//
			//byte[] array = { 0x1, 0x2, 0x3, 0x4, };
			//var sing = cert_3.Sign(array);
			//
			//Console.WriteLine(cert_3.Certificate.Verify(array, sing));
			//array[0] = 0;
			//Console.WriteLine(cert_3.Certificate.Verify(array, sing));
		}
	}

}
