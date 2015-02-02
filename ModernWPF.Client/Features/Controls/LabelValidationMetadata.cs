using System.ComponentModel;

namespace ModernWPF.Client.Features.Controls
{
    internal class LabelValidationMetadata : INotifyPropertyChanged
    {
        private string _caption;
        private string _name;
        private bool _isRequired;

        public bool IsRequired
        {
            get
            {
                return this._isRequired;
            }
            set
            {
                if (this._isRequired == value)
                    return;
                this._isRequired = value;
                this.NotifyPropertyChanged("IsRequired");
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (!(this._name != value))
                    return;
                this._name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        public string Caption
        {
            get
            {
                return this._caption;
            }
            set
            {
                if (!(this._caption != value))
                    return;
                this._caption = value;
                this.NotifyPropertyChanged("Caption");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }
    }
}