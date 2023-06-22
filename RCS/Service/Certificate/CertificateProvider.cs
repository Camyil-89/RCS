using RCS.Models.Certificates;
using RCS.Models.Certificates.Russian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
				var cert = new Models.Certificates.Russian.CertificateSecret();
				cert.Certificate.Info = settings.Info;
				cert.Init(settings.Name, settings.SizeKey);
				cert.SignSelf();
				return cert;
			}
			else
			{
				var cert = new Models.Certificates.Russian.CertificateSecret();
				cert.Certificate.Info = settings.Info;
				cert.Init(settings.MasterCertificate, settings.Name, settings.SizeKey);
				return cert;
			}
			
		}
		public static bool VerifyCertificate(Models.Certificates.Russian.Certificate master, Models.Certificates.Russian.Certificate slave)
		{
			return master.Verify(slave.Info.RawByte(), slave.Sign);
		}

		public static void RCSSignZipFile(string path_to_zip, Models.Certificates.Russian.CertificateSecret certificate)
		{
			try
			{
				//var cert_raw = XmlProvider.LoadInzip<Models.Certificates.Russian.Certificate>(path_to_zip, "RCS_Certificate_metadata.сертификат");
				XmlProvider.DeleteEntryzip(path_to_zip, "RCS_Certificate_metadata.сертификат");
				XmlProvider.DeleteEntryzip(path_to_zip, "RCS_Certificate_metadata.подпись");

				var stream = new FileStream(path_to_zip, FileMode.Open);
				var sign = certificate.Sign(stream);
				stream.Close();
				XmlProvider.SaveInzip<Models.Certificates.Russian.Certificate>(path_to_zip, "RCS_Certificate_metadata.сертификат", certificate.Certificate);
				XmlProvider.WriteInZip(path_to_zip, "RCS_Certificate_metadata.подпись", sign);
			} catch (Exception ex) { Console.WriteLine(ex); }
		}
		public static void Test()
		{


			var cert_1 = RCSCreateCertificate(new CreateSettingsCertificate());

			cert_1.SaveToFile("root_private.txt");
			cert_1.Certificate.SaveToFile("root.txt");

			var x = cert_1.Certificate.Info.Raw();
			var cert_2 = RCSCreateCertificate(new CreateSettingsCertificate() { MasterCertificate = cert_1});

			var cert_3 = RCSCreateCertificate(new CreateSettingsCertificate() { MasterCertificate = cert_2 });


			RCSSignZipFile("test.zip", cert_3);

			byte[] array = { 0x1, 0x2, 0x3, 0x4, };
			var sing = cert_3.Sign(array);

			Console.WriteLine(cert_3.Certificate.Verify(array, sing));
			array[0] = 0;
			Console.WriteLine(cert_3.Certificate.Verify(array, sing));
		}
	}

}
