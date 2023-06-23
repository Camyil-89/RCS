using RCS.Models.Certificates.Russian;
using RCS.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace RCS.Models.Certificates
{
	public class CertificateStore : Base.ViewModel.BaseViewModel
	{

		#region Certificates: Description
		/// <summary>Description</summary>
		private ObservableCollection<StoreItem> _Certificates = new ObservableCollection<StoreItem>();
		/// <summary>Description</summary>
		public ObservableCollection<StoreItem> Certificates { get => _Certificates; set => Set(ref _Certificates, value); }
		#endregion

		#region CertificatesView: Description
		/// <summary>Description</summary>
		private ObservableCollection<StoreItem> _CertificatesView = new ObservableCollection<StoreItem>();
		/// <summary>Description</summary>
		public ObservableCollection<StoreItem> CertificatesView { get => _CertificatesView; set => Set(ref _CertificatesView, value); }
		#endregion
		public void Add(Russian.CertificateSecret certificate)
		{
			Certificates.Add(new StoreItem() { Certificate = certificate });
		}
		public StoreItem GetItem(Guid uid)
		{
			return Certificates.FirstOrDefault((i) => i.ValidType != ValidType.NotValid && i.Certificate.Certificate.Info.UID == uid);
		}
		public void Load()
		{
			Certificates.Clear();
			CertificatesView.Clear();
			List<string> corrupt = new List<string>();
			foreach (var path in Directory.GetFiles(XmlProvider.PathToTrustedCertificates))
			{
				if (path.EndsWith(".ссертификат") == false)
					continue;
				try
				{
					Certificates.Add(new StoreItem() { Certificate = XmlProvider.Load<CertificateSecret>(path) });
				}
				catch { corrupt.Add(path); }
			}

			for (int i = 0; i < Certificates.Count; i++)
			{
				Validate();
				CertificatesView.Add(Certificates[i]);
			}
			List<StoreItem> remove = new List<StoreItem>();
			foreach (var item in Certificates)
			{
				if (item.ValidType == ValidType.NotValid)
					remove.Add(item);
			}
			foreach (var item in remove)
				Certificates.Remove(item);
			if (corrupt.Count > 0)
			{
				Service.UI.MessageBoxHelper.WarningShow($"Не удалось загрузить некотрые сертификаты! возможно они повреждены!\n\n{string.Join("\n", corrupt)}");
			}
		}
		public void Validate()
		{
			foreach (var i in Certificates)
			{
				if (i.ValidType == ValidType.NotValid)
				{
					i.ValidType = Valid(i);
				}
			}
		}

		private ValidType Valid(StoreItem item)
		{
			if (item.Certificate.Certificate.Info.DateDead < DateTime.Now)
				return ValidType.NotValid;
			if (item.Certificate.Certificate.Info.MasterUID == item.Certificate.Certificate.Info.UID)
				return ValidType.SelfValid;
			foreach (var i in Certificates)
			{
				if (i.Certificate.Certificate.Info.UID == item.Certificate.Certificate.Info.MasterUID)
				{
					return ValidType.Valid;
				}
			}
			return ValidType.NotValid;
		}
		public Certificate FindMasterCertificate(Models.Certificates.Russian.Certificate certificate)
		{
			if (certificate == null)
				return null;
			if (certificate.Info.UID == certificate.Info.MasterUID)
			{
				if (certificate.Verify(certificate.Info.RawByte(), certificate.Sign) == false)
					return null;
				return certificate;
			}

			var cert = GetItem(certificate.Info.MasterUID).Certificate.Certificate;
			if (cert.Verify(certificate.Info.RawByte(), certificate.Sign) == false)
				return null;
			return FindMasterCertificate(cert);
		}
		//public bool CheckValidMaster(Models.Certificates.Russian.Certificate certificate)
		//{
		//	if (certificate.Info.DateDead < DateTime.Now)
		//		return false;
		//
		//	if (certificate.Info.MasterUID == certificate.Info.UID)
		//		return true;
		//
		//	foreach (var cert_valid in Certificates.Where((i) =>i.ValidType != ValidType.NotValid && i.Certificate.Certificate.Info.UID != certificate.Info.MasterUID))
		//	{
		//		StoreItem item = cert_valid;
		//		while (item.ValidType != ValidType.SelfValid)
		//		{
		//
		//		}
		//	}
		//}
	}
}
