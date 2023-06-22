using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Models.Certificates
{
	public enum ValidType: byte
	{
		NotValid = 0,
		Valid = 1,
		SelfValid = 2,
	}
	public class StoreItem: Base.ViewModel.BaseViewModel
	{

		#region Certificate: Description
		/// <summary>Description</summary>
		private Russian.CertificateSecret _Certificate;
		/// <summary>Description</summary>
		public Russian.CertificateSecret Certificate { get => _Certificate; set => Set(ref _Certificate, value); }
		#endregion



		#region ValidType: Description
		/// <summary>Description</summary>
		private ValidType _ValidType = ValidType.NotValid;
		/// <summary>Description</summary>
		public ValidType ValidType { get => _ValidType; set => Set(ref _ValidType, value); }
		#endregion

	}
}
