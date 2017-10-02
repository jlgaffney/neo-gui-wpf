using System.Linq;
using System.Windows.Input;
using Neo.Properties;
using Neo.UI.Controls;
using Neo.UI.MVVM;

namespace Neo.UI.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        private NeoWindow view;

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

        private string[] NEP5ContractsLines
        {
            get
            {
                if (string.IsNullOrEmpty(this.NEP5ContractsList)) return new string[0];

                var lines = this.NEP5ContractsList.Split('\n');

                // Remove \r character from end of line if present
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line[line.Length - 1] == '\r')
                    {
                        line = line.Substring(0, line.Length - 1);
                    }

                    lines[i] = line;
                }

                return lines;
            }
        }

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ICommand ApplyCommand => new RelayCommand(this.Apply);

        public override void OnViewAttached(object view)
        {
            this.view = view as NeoWindow;

            var nep5ContractsLines = Settings.Default.NEP5Watched.OfType<string>().ToArray();

            // Concatenate lines
            var nep5ContractsList = string.Empty;

            foreach (var line in nep5ContractsLines)
            {
                nep5ContractsList += line + "\n";
            }

            this.NEP5ContractsList = nep5ContractsList;
        }

        public void Ok()
        {
            // TODO Should this do more than just close?

            this.view?.Close();
        }

        public void Cancel()
        {
            this.view?.Close();
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