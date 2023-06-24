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
        public void Add(Certificate certificate)
        {
            Certificates.Add(new StoreItem() { Certificate = certificate });
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
                var root = FindMasterCertificate(i.Certificate);
                if (root == null)
                    i.ValidType = ValidType.NotValid;
                CertificatesView.Add(i);
            }
            List<StoreItem> remove = new List<StoreItem>();
            foreach (var item in Certificates)
            {
                if (item.ValidType == ValidType.NotValid)
                    remove.Add(item);
            }
            foreach (var item in remove)
                Certificates.Remove(item);
			//if (corrupt.Count > 0)
			//{
			//    Service.UI.MessageBoxHelper.WarningShow($"Не удалось загрузить некотрые сертификаты! возможно они повреждены!\n\n{string.Join("\n", corrupt)}");
			//}
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
        public Certificate FindMasterCertificate(Certificates.Certificate certificate)
        {
            if (certificate == null || certificate.Info.DateDead < DateTime.Now)
                return null;
            if (certificate.Info.UID == certificate.Info.MasterUID)
            {
                if (certificate.Verify(certificate.Info.RawByte(), certificate.Sign) == false)
                    return null;
                return certificate;
            }
            var item = GetItem(certificate.Info.MasterUID);
            if (item == null)
                return null;
            var cert = item.Certificate;
            if (cert.Info.DateDead < DateTime.Now)
                return null;
            if (cert.Verify(certificate.Info.RawByte(), certificate.Sign) == false)
                return null;
            return FindMasterCertificate(cert);
        }
    }
}
