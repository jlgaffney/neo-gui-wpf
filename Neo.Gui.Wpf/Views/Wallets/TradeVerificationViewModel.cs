using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neo.Core;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class TradeVerificationViewModel : ViewModelBase, IDialogViewModel<TradeVerificationDialogResult>, ILoadable
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        private readonly IDispatchHelper dispatchHelper;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionOutputItem> Items { get; }

        public RelayCommand AcceptCommand => new RelayCommand(this.Accept);

        public RelayCommand RefuseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        public bool TradeAccepted { get; set; }
        #endregion

        #region Constructor 
        public TradeVerificationViewModel(
            IWalletController walletController,
            IDispatchHelper dispatchHelper)
        {
            this.walletController = walletController;
            this.dispatchHelper = dispatchHelper;

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
            this.TradeAccepted = true;
            this.SetDialogResultAndClose(this, new TradeVerificationDialogResult(this.TradeAccepted));
        }

        private void SetOutputs(IEnumerable<TransactionOutput> outputs)
        {
            this.dispatchHelper.InvokeOnMainUIThread(() =>
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