using System;
using GalaSoft.MvvmLight;
using Neo.UI.Core.Globalization.Resources;

namespace Neo.Gui.ViewModels.Data
{
    public class UiTransactionSummary : ViewModelBase
    {
        private uint confirmations;

        public string Id { get; }

        public DateTime Time { get; }

        public string Type { get; }

        public string Confirmations => this.confirmations > 0 ? this.confirmations.ToString() : Strings.Unconfirmed;

        public void SetConfirmations(uint confirmation)
        {
            this.confirmations = confirmation;

            RaisePropertyChanged(this.Confirmations);
        }

        public UiTransactionSummary(string id, DateTime time, string type)
        {
            this.Id = id;
            this.Time = time;
            this.Type = type;
        }
    }
}
