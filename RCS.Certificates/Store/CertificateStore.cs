using RCS.Certificates;
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

namespace RCS.Certificates.Store
{
	public enum StatusSearch: byte
	{
		Find = 1,
		NotFoundParent = 2,
		TimeDead = 3,
		NotValid = 4,
		ParentTimeDead = 5,
	}
	public class SearchCertificateInfo: Base.ViewModel.BaseViewModel
	{

		#region Status: Description
		/// <summary>Description</summary>
		private StatusSearch _Status = StatusSearch.NotFoundParent;
		/// <summary>Description</summary>
		public StatusSearch Status { get => _Status; set => Set(ref _Status, value); }
		#endregion


		#region Certificate: Description
		/// <summary>Description</summary>
		private Certificate _Certificate = null;
		/// <summary>Description</summary>
		public Certificate Certificate { get => _Certificate; set => Set(ref _Certificate, value); }
		#endregion


		#region LastParent: Description
		/// <summary>Description</summary>
		private Certificate _LastParent = null;
		/// <summary>Description</summary>
		public Certificate LastParent { get => _LastParent; set => Set(ref _LastParent, value); }
		#endregion
	}
    public class CertificateStore : Base.ViewModel.BaseViewModel
    {
        public string PathStore = XmlProvider.PathToTrustedCertificates;
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
        private StoreItem GetItemWithoutValidating(Guid uid)
		{
			return Certificates.FirstOrDefault((i) => i.Certificate.Info.UID == uid);
		}
        public StoreItem GetItem(Guid uid)
        {
            return Certificates.FirstOrDefault((i) => i.ValidType != ValidType.NotValid && i.Certificate.Info.UID == uid);
        }
        public List<string> Load()
        {
            Certificates.Clear();
            CertificatesView.Clear();
            List<string> corrupt = new List<string>();
            foreach (var path in Directory.GetFiles(PathStore))
            {
                if (path.EndsWith(".сертификат") == false)
                    continue;
                try
                {
                    Certificates.Add(new StoreItem() { Certificate = XmlProvider.Load<Certificate>(path) });
                }
                catch { corrupt.Add(path); }
            }

			for (int i = 0; i < Certificates.Count; i++)
			{
				Validate();
			}
			foreach (var i in Certificates)
			{
				var root = FindMasterCertificate(i.Certificate).Certificate;
				if (root == null)
					i.ValidType = ValidType.NotValid;
				CertificatesView.Add(i);
			}
			//foreach (var i in Certificates)
			//{
			//	try
			//	{
			//		var info = FindMasterCertificate(i.Certificate);
			//		Console.WriteLine($"{info.Certificate};{info.Status}");
			//		if (info.Status == StatusSearch.NotFoundParent)
			//			i.ValidType = CertificateManager.RCSCheckValidCertificate(info.LastParent) == true ? ValidType.Valid : ValidType.NotValid;
			//	} catch (Exception ex) { Console.WriteLine(ex); }
			//	CertificatesView.Add(i);
			//}
            List<StoreItem> remove = new List<StoreItem>();
            foreach (var item in Certificates)
            {
                if (item.ValidType == ValidType.NotValid)
                    remove.Add(item);
            }
            foreach (var item in remove)
                Certificates.Remove(item);
			return corrupt;
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

        public ValidType Valid(StoreItem item)
        {
            if (item.Certificate.Info.DateDead < DateTime.Now)
                return ValidType.NotValid;
            if (item.Certificate.Info.MasterUID == item.Certificate.Info.UID)
                return ValidType.SelfValid;
            foreach (var i in Certificates)
            {
                if (i.Certificate.Info.UID == item.Certificate.Info.MasterUID)
                {
                    return ValidType.Valid;
                }
            }
            return ValidType.NotValid;
        }
        public SearchCertificateInfo FindMasterCertificate(Certificates.Certificate certificate, Certificate last_parent = null)
        {
            if (certificate == null)
				return new SearchCertificateInfo() { Status = StatusSearch.NotFoundParent, LastParent = last_parent == null ? certificate: last_parent };
			if (certificate.Info.DateDead < DateTime.Now)
				return new SearchCertificateInfo() { Status = StatusSearch.TimeDead };
            if (certificate.Info.UID == certificate.Info.MasterUID)
            {
                if (certificate.Verify(certificate.Info.RawByte(), certificate.Sign) == false)
                     return new SearchCertificateInfo() { Status = StatusSearch.NotValid };
				return new SearchCertificateInfo() { Status = StatusSearch.Find, Certificate = certificate };
            }
            var item = GetItemWithoutValidating(certificate.Info.MasterUID);
            if (item == null)
				return new SearchCertificateInfo() { Status = StatusSearch.NotFoundParent, LastParent = last_parent == null ? certificate : last_parent };
			var cert = item.Certificate;
            if (cert.Info.DateDead < DateTime.Now)
				return new SearchCertificateInfo() { Status = StatusSearch.ParentTimeDead };
			if (cert.Verify(certificate.Info.RawByte(), certificate.Sign) == false)
				return new SearchCertificateInfo() { Status = StatusSearch.NotValid };
			return FindMasterCertificate(cert, certificate);
        }
    }
}
