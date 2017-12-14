using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Wallets
{
    public class TradeVerificationViewModel : ViewModelBase, IDialogViewModel<TradeVerificationDialogResult>, ILoadable
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

        #region IDialogViewModel implementation 
        public TradeVerificationDialogResult DialogResult { get; set; }

        public event EventHandler Close;

        public event EventHandler<TradeVerificationDialogResult> SetDialogResultAndClose;
        #endregion

        #region ILoadable implementation 
        public void OnLoad(params object[] parameters)
        {
            if (!parameters.Any())
            {
                return;
            }

            var tradeVerificationLoadParameters = parameters[0] as TradeVerificationLoadParameters;

            this.SetOutputs(tradeVerificationLoadParameters.TransactionOutputs);

        }
        #endregion

        #region Private Methods 
        private void Accept()
        {
            if (this.SetDialogResultAndClose == null) return;

            var dialogResult = new TradeVerificationDialogResult(true);

            this.SetDialogResultAndClose(this, dialogResult);
        }

        private void SetOutputs(IEnumerable<TransactionOutput> outputs)
        {
            this.dispatchService.InvokeOnMainUIThread(() =>
            {
                foreach (var output in outputs)
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
    }
}