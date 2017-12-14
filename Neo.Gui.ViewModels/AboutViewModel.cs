using System;
using System.Reflection;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;

namespace Neo.Gui.ViewModels
{
    public class AboutViewModel : ViewModelBase, IDialogViewModel<AboutDialogResult>
    {
        #region Private Fields 
        private Version assemblyVersion;
        #endregion

        #region Public Properties 
        public Version AssemblyVersion
        {
            get => this.assemblyVersion;
            set
            {
                if (this.assemblyVersion == value) return;

                this.assemblyVersion = value;

                RaisePropertyChanged();
            }
        }

        public ICommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
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
