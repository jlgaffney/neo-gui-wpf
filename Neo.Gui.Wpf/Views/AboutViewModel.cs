using System;
using System.Reflection;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views
{
    public class AboutViewModel : ViewModelBase, IDialogViewModel<AboutDialogResult>
    {
        #region Private Fields 
        private Version assemblyVersion;
        #endregion

        #region Public Properties 
        public Version AssemblyVersion
        {
            get
            {
                return this.assemblyVersion;
            }
            set
            {
                this.assemblyVersion = value;
                this.NotifyPropertyChanged();
            }
        }

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public AboutViewModel()
        {
            this.AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<AboutDialogResult> SetDialogResultAndClose;

        public AboutDialogResult DialogResult { get; private set; }
        #endregion
    }
}
