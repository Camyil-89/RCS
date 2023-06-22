using Microsoft.Extensions.DependencyInjection;
using RCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Service
{
	public class Settings : Base.ViewModel.BaseViewModel
	{
		public static Settings Instance => App.Host.Services.GetRequiredService<Settings>();

		#region Parametrs: Description
		/// <summary>Description</summary>
		private SettingsParametrs _Parametrs = new SettingsParametrs();
		/// <summary>Description</summary>
		public SettingsParametrs Parametrs { get => _Parametrs; set => Set(ref _Parametrs, value); }
		#endregion
	}
}
