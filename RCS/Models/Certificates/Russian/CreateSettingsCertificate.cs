using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Models.Certificates.Russian
{
    public class CreateSettingsCertificate: Base.ViewModel.BaseViewModel
    {

		#region MasterCertificate: Description
		/// <summary>Description</summary>
		private CertificateSecret _MasterCertificate;
		/// <summary>Description</summary>
		public CertificateSecret MasterCertificate { get => _MasterCertificate; set => Set(ref _MasterCertificate, value); }
		#endregion


		#region RSAParametrs: Description
		/// <summary>Description</summary>
		private RSAParameters _RSAParametrs;
		/// <summary>Description</summary>
		public RSAParameters RSAParametrs { get => _RSAParametrs; set => Set(ref _RSAParametrs, value); }
		#endregion


		#region SizeKey: Description
		/// <summary>Description</summary>
		private int _SizeKey = 2048;
		/// <summary>Description</summary>
		public int SizeKey { get => _SizeKey; set => Set(ref _SizeKey, value); }
		#endregion


		#region Name: Description
		/// <summary>Description</summary>
		private string _Name = "RCSRootCertificate";
		/// <summary>Description</summary>
		public string Name { get => _Name; set => Set(ref _Name, value); }
		#endregion


		#region Info: Description
		/// <summary>Description</summary>
		private BaseCertificateInfo _Info = new CertificateInfo();
		/// <summary>Description</summary>
		public BaseCertificateInfo Info { get => _Info; set => Set(ref _Info, value); }
		#endregion
	}
}
