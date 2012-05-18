

using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
namespace WindowsAzure.ServiceBus
{
    public class DeviceVM : ViewModelBase
    {
        private ObservableCollection<string> _messages = new ObservableCollection<string>();
        public ObservableCollection<string> Messages {
            get { return _messages; }
            set {
                _messages = value;
                RaisePropertyChanged("Messages");
            }
        }
    }
}
