using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Wallets
{
    public class TradeVerificationViewModel : ViewModelBase,
        ILoadableDialogViewModel<TradeVerificationDialogResult, TradeVerificationLoadParameters>
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        private readonly IDispatchService dispatchService;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionOutputItem> Items { get; }

        public ICommand AcceptCommand => new RelayCommand(this.Accept);

        public ICommand RefuseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public TradeVerificationViewModel(
            IWalletController walletController,
            IDispatchService dispatchService)
        {
            this.walletController = walletController;
            this.dispatchService = dispatchService;

            this.Items = new ObservableCollection<TransactionOutputItem>();
        }
        #endregion

        #region ILoadableDialogViewModel implementation 
        public TradeVerificationDialogResult DialogResult { get; set; }

        public event EventHandler Close;

        public event EventHandler<TradeVerificationDialogResult> SetDialogResultAndClose;

        public void OnDialogLoad(TradeVerificationLoadParameters parameters)
        {
            if (parameters?.TransactionOutputs == null) return;

            // Set outputs
            this.dispatchService.InvokeOnMainUIThread(() =>
            {
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
            });
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