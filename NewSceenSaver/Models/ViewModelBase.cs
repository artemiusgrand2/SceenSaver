using System.Collections.Generic;
using System.ComponentModel;

namespace NewScreenSaver.Models
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private readonly IList<string> awaitingNotifyChangeProperties = new List<string>();
        private bool handlePropertyChanged;

        protected ViewModelBase()
        {
            handlePropertyChanged = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void StopPropertyChanged()
        {
            handlePropertyChanged = false;
        }

        protected void ReleaseProperties()
        {
            handlePropertyChanged = true;
            foreach (string property in awaitingNotifyChangeProperties)
            {
                OnPropertyChanged(property);
            }
            awaitingNotifyChangeProperties.Clear();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (handlePropertyChanged)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            else
            {
                if (!awaitingNotifyChangeProperties.Contains(propertyName))
                {
                    awaitingNotifyChangeProperties.Add(propertyName);
                }
            }
        }
    }
}
