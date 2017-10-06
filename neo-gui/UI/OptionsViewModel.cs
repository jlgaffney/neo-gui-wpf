using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Neo.Extensions;
using Neo.Properties;
using Neo.UI.Controls;
using Neo.UI.MVVM;

namespace Neo.UI.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        private string nep5ContractsList;

        public string NEP5ContractsList
        {
            get => this.nep5ContractsList;
            set
            {
                if (this.nep5ContractsList == value) return;

                this.nep5ContractsList = value;

                NotifyPropertyChanged();
            }
        }

        private IEnumerable<string> NEP5ContractsLines
        {
            get
            {
                if (string.IsNullOrEmpty(this.NEP5ContractsList)) return new string[0];

                return this.NEP5ContractsList.ToLines();
            }
        }

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ICommand ApplyCommand => new RelayCommand(this.Apply);

        public override void OnWindowAttached(NeoWindow window)
        {
            base.OnWindowAttached(window);

            var nep5ContractsLines = Settings.Default.NEP5Watched.OfType<string>().ToArray();

            // Concatenate lines
            var contractsList = string.Empty;

            foreach (var line in nep5ContractsLines)
            {
                contractsList += line + "\n";
            }

            this.NEP5ContractsList = contractsList;
        }

        public void Ok()
        {
            // TODO Should this do more than just close?

            this.TryClose();
        }

        public void Cancel()
        {
            this.TryClose();
        }

        public void Apply()
        {
            Settings.Default.NEP5Watched.Clear();
            Settings.Default.NEP5Watched.AddRange(this.NEP5ContractsLines.Where(p =>
                !string.IsNullOrWhiteSpace(p) && UInt160.TryParse(p, out _)).ToArray());

            Settings.Default.Save();
        }
    }
}