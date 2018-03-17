using System;
using GalaSoft.MvvmLight;
using Neo.UI.Core.Globalization.Resources;

namespace Neo.Gui.ViewModels.Data
{
    public class UiTransactionSummary : ObservableObject
    {
        private uint? confirmations;

        public string Id { get; }

        public DateTime Time { get; }

        public uint? Height { get; }

        public string Type { get; }

        public string Confirmations => this.confirmations != null && this.confirmations.Value > 0 ? this.confirmations.ToString() : Strings.Unconfirmed;

        public uint? ConfirmationsValue
        {
            get => this.confirmations;
            set
            {
                if (this.confirmations == value) return;

                this.confirmations = value;
                
                RaisePropertyChanged();
                RaisePropertyChanged(this.Confirmations);
            }
        }

        public UiTransactionSummary(string id, DateTime time, uint? height, string type)
        {
            this.Id = id;
            this.Time = time;
            this.Height = height;
            this.Type = type;

            if (height.HasValue)
            {
                this.ConfirmationsValue = 0;
            }
        }
    }
}
