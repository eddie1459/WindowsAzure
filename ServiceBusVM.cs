using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace WindowsAzure.ServiceBus
{
    public class ServiceBusVM : ViewModelBase
    {
        private ObservableCollection<string> _topics = new ObservableCollection<string>();
        private ObservableCollection<string> _subscriptions = new ObservableCollection<string>();
        private ObservableCollection<string> _messages = new ObservableCollection<string>();

        
        public ObservableCollection<string> Topics
        {
            get { return _topics; }
            set {
                _topics = value;
                RaisePropertyChanged("Topics");
            }
        }

        public ObservableCollection<string> Subscriptions
        {
            get { return _subscriptions; }
            set
            {
                _subscriptions = value;
                RaisePropertyChanged("Subscriptions");
            }
        }

        public ObservableCollection<string> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                RaisePropertyChanged("Messages");
            }
        }

    }
}
