using Microsoft.Extensions.DependencyInjection;
using RCSServer.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.ViewModels
{
	public class Locator
	{
		public MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();
	}
}
