using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Certificates.Store
{
	public enum ValidType: byte
	{
		[Description("Недействителен")]
		NotValid = 0,
		[Description("Действителен")]
		Valid = 1,
		[Description("Самоподписанный")]
		SelfValid = 2,
	}
	public class StoreItem: Base.ViewModel.BaseViewModel
	{

		#region Certificate: Description
		/// <summary>Description</summary>
		private Certificate _Certificate;
		/// <summary>Description</summary>
		public Certificate Certificate { get => _Certificate; set => Set(ref _Certificate, value); }
		#endregion



		#region ValidType: Description
		/// <summary>Description</summary>
		private ValidType _ValidType = ValidType.NotValid;
		/// <summary>Description</summary>
		public ValidType ValidType { get => _ValidType; set => Set(ref _ValidType, value); }
		#endregion

	}
}
