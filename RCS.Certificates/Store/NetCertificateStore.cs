using RCS.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RCS.Certificates.Store
{
	public enum TypeStoreItem : byte
	{
		[Description("Локальное хранилище")]
		Local = 0,
		[Description("Сетевое хранилище")]
		Net = 1,
	}
	public class NetStoreItem : Base.ViewModel.BaseViewModel
	{

		#region Name: Description
		/// <summary>Description</summary>
		private string _Name;
		/// <summary>Description</summary>
		public string Name { get => _Name; set => Set(ref _Name, value); }
		#endregion


		#region MasterName: Description
		/// <summary>Description</summary>
		private string _MasterName;
		/// <summary>Description</summary>
		public string MasterName { get => _MasterName; set => Set(ref _MasterName, value); }
		#endregion


		#region UID: Description
		/// <summary>Description</summary>
		private Guid _UID;
		/// <summary>Description</summary>
		public Guid UID { get => _UID; set => Set(ref _UID, value); }
		#endregion


		#region Status: Description
		/// <summary>Description</summary>
		private ValidType _Status = ValidType.NotValid;
		/// <summary>Description</summary>
		public ValidType Status { get => _Status; set => Set(ref _Status, value); }
		#endregion


		#region Type: Description
		/// <summary>Description</summary>
		private TypeStoreItem _Type = TypeStoreItem.Local;
		/// <summary>Description</summary>
		public TypeStoreItem Type { get => _Type; set => Set(ref _Type, value); }
		#endregion
	}
	public class NetCertificateStore : Base.ViewModel.BaseViewModel
	{
		private static object _lock = new object();
		public NetCertificateStore()
		{
			BindingOperations.EnableCollectionSynchronization(Items, _lock);
		}
		public string PathStore = XmlProvider.PathToTrustedCertificates;

		#region Items: Description
		/// <summary>Description</summary>
		private ObservableCollection<NetStoreItem> _Items = new ObservableCollection<NetStoreItem>();
		/// <summary>Description</summary>
		public ObservableCollection<NetStoreItem> Items { get => _Items; set => Set(ref _Items, value); }
		#endregion


		#region LocalStore: Description
		/// <summary>Description</summary>
		private CertificateStore _LocalStore;
		/// <summary>Description</summary>
		public CertificateStore LocalStore { get => _LocalStore; set => Set(ref _LocalStore, value); }
		#endregion



		public IEnumerable<NetStoreItem> GetItems()
		{
			return Items;
		}

		public void Add(NetStoreItem certificate)
		{
			Items.Add(certificate);
		}
		public void Add(Certificate certificate, bool is_net = false)
		{
			var item = new NetStoreItem();
			item.Name = certificate.Info.Name;
			item.MasterName = certificate.Info.Master;
			item.UID = certificate.Info.UID;
			item.Status = ValidType.NotValid;
			item.Type = is_net == true ? TypeStoreItem.Net : TypeStoreItem.Local;
			Add(item);
		}

		public void Load()
		{
			Items.Clear();
			foreach (var path in Directory.GetFiles(PathStore))
			{
				if (path.EndsWith(".сертификат") == false)
					continue;
				var cert = XmlProvider.Load<Certificate>(path);
				Add(cert);
			}
			foreach (var cert in RequestNetCertificate())
			{
				if (cert == null)
					break;

				Add(cert);
			}


		}
		public IEnumerable<NetStoreItem> RequestNetCertificate()
		{
			try
			{
				return CertificateManager.GetNetCertificates();
			}
			catch
			{
				return null;
			}
		}
	}
}
