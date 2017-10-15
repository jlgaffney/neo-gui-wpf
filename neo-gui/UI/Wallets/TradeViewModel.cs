using Neo.Core;
using Neo.IO.Json;
using Neo.Properties;
using Neo.SmartContract;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Neo.UI.Base.Controls;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Wallets
{
    internal class TradeViewModel : ViewModelBase
    {
        private TradeView view;

        private string payToAddress;
        private string myRequest;
        private string counterPartyRequest;
        
        private bool mergeEnabled;

        private UInt160 scriptHash;

        public TradeViewModel()
        {
            this.Items = new ObservableCollection<TxOutListBoxItem>();
        }

        public ObservableCollection<TxOutListBoxItem> Items { get; }

        public string PayToAddress
        {
            get => this.payToAddress;
            set
            {
                if (this.payToAddress == value) return;

                this.payToAddress = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.ScriptHash));
                
                try
                {
                    this.ScriptHash = Wallet.ToScriptHash(this.PayToAddress);
                }
                catch (FormatException)
                {
                    this.ScriptHash = null;
                }

                this.Items.Clear();
            }
        }

        public UInt160 ScriptHash
        {
            get => this.scriptHash;
            set
            {
                if (this.scriptHash == value) return;

                this.scriptHash = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ItemsListEnabled));
            }
        }

        public bool ItemsListEnabled => this.ScriptHash != null;

        public string MyRequest
        {
            get => this.myRequest;
            set
            {
                if (this.myRequest == value) return;

                this.myRequest = value;

                NotifyPropertyChanged();
            }
        }

        public string CounterPartyRequest
        {
            get => this.counterPartyRequest;
            set
            {
                if (this.counterPartyRequest == value) return;

                this.counterPartyRequest = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ValidateEnabled));
            }
        }

        public bool InitiateEnabled => this.Items.Count > 0;

        public bool ValidateEnabled => !string.IsNullOrEmpty(this.CounterPartyRequest) && !string.IsNullOrEmpty(this.MyRequest);

        public bool MergeEnabled
        {
            get => this.mergeEnabled;
            set
            {
                if (this.mergeEnabled == value) return;

                this.mergeEnabled = value;

                NotifyPropertyChanged();
            }
        }

        public ICommand InitiateCommand => new RelayCommand(this.Initiate);

        public ICommand ValidateCommand => new RelayCommand(this.Validate);

        public ICommand MergeCommand => new RelayCommand(this.Merge);


        public override void OnWindowAttached(NeoWindow window)
        {
            this.view = window as TradeView;
        }

        public void UpdateInitiateButtonEnabled()
        {
            NotifyPropertyChanged(nameof(this.InitiateEnabled));
        }

        private void Initiate()
        {
            var tx = App.CurrentWallet.MakeTransaction(new ContractTransaction
            {
                Outputs = this.Items.Select(p => p.ToTxOutput()).ToArray()
            }, fee: Fixed8.Zero);

            this.MyRequest = RequestToJson(tx).ToString();

            InformationBox.Show(this.MyRequest, Strings.TradeRequestCreatedMessage, Strings.TradeRequestCreatedCaption);

            this.view?.SetSelectedTab(1);
        }

        private async void Validate()
        {
            IEnumerable<CoinReference> inputs;
            IEnumerable<TransactionOutput> outputs;
            var json = JObject.Parse(this.CounterPartyRequest);
            if (json.ContainsProperty("hex"))
            {
                var txMine = JsonToRequest(JObject.Parse(this.MyRequest));
                var txOthers = (ContractTransaction)ContractParametersContext.FromJson(json).Verifiable;
                inputs = txOthers.Inputs.Except(txMine.Inputs);
                var outputsOthers = new List<TransactionOutput>(txOthers.Outputs);
                foreach (var outputMine in txMine.Outputs)
                {
                    var outputOthers = outputsOthers.FirstOrDefault(p => p.AssetId == outputMine.AssetId && p.Value == outputMine.Value && p.ScriptHash == outputMine.ScriptHash);
                    if (outputOthers == null)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(this, Strings.Failed, Strings.TradeFailedFakeDataMessage);
                        return;
                    }
                    outputsOthers.Remove(outputOthers);
                }
                outputs = outputsOthers;
            }
            else
            {
                var txOthers = JsonToRequest(json);
                inputs = txOthers.Inputs;
                outputs = txOthers.Outputs;
            }

            try
            {
                if (inputs.Select(p => Blockchain.Default.GetTransaction(p.PrevHash).Outputs[p.PrevIndex].ScriptHash).Distinct().Any(p => App.CurrentWallet.ContainsAddress(p)))
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(this, Strings.Failed, Strings.TradeFailedInvalidDataMessage);
                    return;
                }
            }
            catch
            {
                await DialogCoordinator.Instance.ShowMessageAsync(this, Strings.Failed, Strings.TradeFailedNoSyncMessage);
                return;
            }

            outputs = outputs.Where(p => App.CurrentWallet.ContainsAddress(p.ScriptHash));

            var verificationView = new TradeVerificationView(outputs);
            verificationView.ShowDialog();

            this.MergeEnabled = verificationView.TradeAccepted;
        }

        private void Merge()
        {
            ContractParametersContext context;
            var json1 = JObject.Parse(this.CounterPartyRequest);
            if (json1.ContainsProperty("hex"))
            {
                context = ContractParametersContext.FromJson(json1);
            }
            else
            {
                var tx1 = JsonToRequest(json1);
                var tx2 = JsonToRequest(JObject.Parse(this.MyRequest));
                context = new ContractParametersContext(new ContractTransaction
                {
                    Attributes = new TransactionAttribute[0],
                    Inputs = tx1.Inputs.Concat(tx2.Inputs).ToArray(),
                    Outputs = tx1.Outputs.Concat(tx2.Outputs).ToArray()
                });
            }

            App.CurrentWallet.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();
                var tx = (ContractTransaction)context.Verifiable;
                App.CurrentWallet.SaveTransaction(tx);
                Program.LocalNode.Relay(tx);
                InformationBox.Show(tx.Hash.ToString(), Strings.TradeSuccessMessage, Strings.TradeSuccessCaption);
            }
            else
            {
                InformationBox.Show(context.ToString(), Strings.TradeNeedSignatureMessage, Strings.TradeNeedSignatureCaption);
            }
        }

        private static ContractTransaction JsonToRequest(JObject json)
        {
            return new ContractTransaction
            {
                Inputs = ((JArray)json["vin"]).Select(p => new CoinReference
                {
                    PrevHash = UInt256.Parse(p["txid"].AsString()),
                    PrevIndex = (ushort)p["vout"].AsNumber()
                }).ToArray(),
                Outputs = ((JArray)json["vout"]).Select(p => new TransactionOutput
                {
                    AssetId = UInt256.Parse(p["asset"].AsString()),
                    Value = Fixed8.Parse(p["value"].AsString()),
                    ScriptHash = Wallet.ToScriptHash(p["address"].AsString())
                }).ToArray()
            };
        }

        private JObject RequestToJson(ContractTransaction tx)
        {
            var json = new JObject
            {
                ["vin"] = tx.Inputs.Select(p => p.ToJson()).ToArray(),
                ["vout"] = tx.Outputs.Select((p, i) => p.ToJson((ushort)i)).ToArray(),
                ["change_address"] = Wallet.ToAddress(App.CurrentWallet.GetChangeAddress())
            };
            return json;
        }
    }
}