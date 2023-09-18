using Microsoft.VisualBasic;
using RCS.Certificates;
using RCS.Certificates.Store;
using RCS.Net.Packets;
using RCS.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCS.Certificates
{
	public enum SignStatus : byte
	{ 
		Changed = 0,
		Valid = 1,
		NotTrusted = 2,
		NotSign = 3,
	}
	public class SignInfo : Base.ViewModel.BaseViewModel
	{

		#region Status: Description
		/// <summary>Description</summary>
		private SignStatus _Status;
		/// <summary>Description</summary>
		public SignStatus Status { get => _Status; set => Set(ref _Status, value); }
		#endregion
	}
	public static class CertificateManager
	{
		public static Certificates.Store.CertificateStore Store { get; set; } = new Certificates.Store.CertificateStore();
		//public static Net.Tcp.RCSTCPClient RCSTCPClient { get; set; } = new Net.Tcp.RCSTCPClient();
		public static EasyTCP.Client Client = new EasyTCP.Client();
		public static ITCPSender TCPSender { get; set; } = null;
		public static CertificateSecret RCSCreateCertificate(CreateSettingsCertificate settings)
		{
			if (settings.MasterCertificate == null)
			{
				Log.WriteLine("RCSCreateCertificate.SELF", LogLevel.Error);
				var cert = new CertificateSecret();
				cert.Certificate.Info = settings.Info;
				cert.Init(settings.Name, settings.SizeKey);
				cert.SignSelf();
				return cert;
			}
			else
			{
				Log.WriteLine("RCSCreateCertificate.MASTER", LogLevel.Error);
				var cert = new Certificates.CertificateSecret();
				cert.Certificate.Info = settings.Info;
				cert.Init(settings.MasterCertificate, settings.SizeKey);
				return cert;
			}

		}
		public static SignInfo RCSCheckSignFile(string path)
		{
			Certificates.Certificate cert;
			SignInfo signInfo = new SignInfo();
			signInfo.Status = SignStatus.NotSign;
			byte[] sign;
			try
			{
				cert = XmlProvider.LoadInzip<Certificates.Certificate>(path, XmlProvider.NameFileCertificateInZip);
				sign = XmlProvider.ReadInZip(path, XmlProvider.NameFileCertificateSignInZip);
				if (RCSValidatingCertificate(cert) == false)
				{
					signInfo.Status = SignStatus.NotTrusted;
					return signInfo;
				}
			}
			catch (NullReferenceException) { return signInfo; }
			XmlProvider.DeleteEntryzip(path, XmlProvider.NameFileCertificateInZip);
			XmlProvider.DeleteEntryzip(path, XmlProvider.NameFileCertificateSignInZip);

			FileStream stream = new FileStream(path, FileMode.Open);

			var check = cert.Verify(stream, sign);
			stream.Close();

			signInfo.Status = check == true ? SignStatus.Valid : SignStatus.Changed;

			XmlProvider.WriteInZip(path, XmlProvider.NameFileCertificateSignInZip, sign);
			XmlProvider.SaveInzip(path, XmlProvider.NameFileCertificateInZip, cert);
			return signInfo;
		}
		public static bool RCSCheckValidCertificate(Certificates.Certificate certificate)
		{
			
			if (Client.CheckConnectionWithServer() == false)
				return false;
			PacketCertificate packet = new PacketCertificate();
			packet.CertificateObj = new Certificate().FromRaw(certificate.Raw());
			return TCPSender.SendAndWait<PacketCertificate>(packet, Client).IsValid;
		}
		public static Certificates.Certificate RequestCertificate(Guid guid)
		{
			return (Certificate)TCPSender.SendAndWait<PacketRequestCertificate>(new PacketRequestCertificate() { RequestPacket = guid}, Client).Certificate;
		}
		public static Certificates.Certificate[] GetLastCertificates()
		{
			var answer = TCPSender.SendAndWait<PacketRequestCertificates>(new PacketRequestCertificates(), Client);
			return answer.Certificates.ConvertAll(i => (Certificate)i).ToArray();
		}
		public static IEnumerable<Store.NetStoreItem> GetNetCertificates()
		{
			var answer = TCPSender.SendAndWait<PacketRequestNetCertificates>(new PacketRequestNetCertificates(), Client);
			foreach (var i in answer.Certificates)
				yield return (NetStoreItem)i;
		}
		public static bool RCSValidatingCertificate(Certificate certificate)
		{

			var info_valid = Store.FindMasterCertificate(certificate);

			if (info_valid.Status == Certificates.Store.StatusSearch.Find)
				return true;
			else if (info_valid.Status == Certificates.Store.StatusSearch.TimeDead ||
					info_valid.Status == Certificates.Store.StatusSearch.NotValid ||
					info_valid.Status == Certificates.Store.StatusSearch.ParentTimeDead)
				return false;
			else if (info_valid.Status == Certificates.Store.StatusSearch.NotFoundParent && Client.CheckConnectionWithServer())
			{
				PacketCertificate packet = new PacketCertificate();
				packet.CertificateObj = info_valid.LastParent;
				return TCPSender.SendAndWait<PacketCertificate>(packet, Client).IsValid;
			}
			return false;
		}
		public static Certificates.Certificate RCSLoadCertificateFromZip(string path)
		{
			return XmlProvider.LoadInzip<Certificates.Certificate>(path, XmlProvider.NameFileCertificateInZip);
		}
		public static Certificates.Certificate RCSLoadCertificate(string path)
		{
			return XmlProvider.Load<Certificates.Certificate>(path);
		}
		public static CertificateSecret RCSLoadCertificateSecret(string path)
		{
			return XmlProvider.Load<CertificateSecret>(path);
		}
		public static bool VerifyCertificate(Certificates.Certificate master, Certificates.Certificate slave)
		{
			if (slave.Info.DateDead > DateTime.Now)
				return false;
			return master.Verify(slave.Info.RawByte(), slave.Sign);
		}
	}

}
