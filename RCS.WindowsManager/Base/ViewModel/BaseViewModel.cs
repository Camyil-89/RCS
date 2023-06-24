using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace RCS.WindowsManager.Base.ViewModel
{
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void CallOnPropertyChanged([CallerMemberName] string PropertyName = null) => OnPropertyChanged(PropertyName);

		protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
		{
			//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
			var handlers = PropertyChanged;
			if (handlers is null) return;

			var invocation_list = handlers.GetInvocationList();

			var arg = new PropertyChangedEventArgs(PropertyName);
			foreach (var action in invocation_list)
				if (action.Target is DispatcherObject disp_object)
					disp_object.Dispatcher.Invoke(action, this, arg);
				else
					action.DynamicInvoke(this, arg);
		}
		protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
		{
			if (Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(PropertyName);
			return true;
		}
	}
}
