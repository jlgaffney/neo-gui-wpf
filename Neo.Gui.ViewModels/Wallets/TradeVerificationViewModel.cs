using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Dialogs.Results.Wallets;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Wallets
{
    public class TradeVerificationViewModel : ViewModelBase,
        IResultDialogViewModel<TradeVerificationLoadParameters, TradeVerificationDialogResult>
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionOutputItem> Items { get; }

        public ICommand AcceptCommand => new RelayCommand(this.Accept);

        public ICommand RefuseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public TradeVerificationViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.Items = new ObservableCollection<TransactionOutputItem>();
        }
        #endregion

        #region ILoadableDialogViewModel implementation

        public event EventHandler Close;

        public event EventHandler<TradeVerificationDialogResult> SetDialogResultAndClose;

        public void OnDialogLoad(TradeVerificationLoadParameters parameters)
        {
            if (parameters?.TransactionOutputs == null) return;

            // Set outputs
            foreach (var output in parameters.TransactionOutputs)
            {
                var asset = this.walletController.GetAssetState(output.AssetId);

                this.Items.Add(new TransactionOutputItem
                {
                    AssetName = $"{asset.GetName()} ({asset.Owner})",
                    AssetId = output.AssetId,
                    Value = new BigDecimal(output.Value.GetData(), 8),
                    ScriptHash = output.ScriptHash
                });
            }
        }
        #endregion

        #region Private Methods 
        private void Accept()
        {
            if (this.SetDialogResultAndClose == null) return;

            var dialogResult = new TradeVerificationDialogResult(true);

            this.SetDialogResultAndClose(this, dialogResult);
        }
        #endregion
    }
}