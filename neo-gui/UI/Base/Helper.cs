using System;
using System.Collections.Generic;
using System.ComponentModel;
using Neo.Core;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Base.Controls;
using Neo.UI.Base.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace Neo.UI.Base
{
    internal static class Helper
    {
        private static readonly Dictionary<Type, NeoWindow> windows = new Dictionary<Type, NeoWindow>();
        
        private static void Helper_WindowClosing(object sender, CancelEventArgs e)
        {
            windows.Remove(sender.GetType());
        }

        public static void Show<T>() where T : NeoWindow, new()
        {
            var type = typeof(T);
            if (!windows.ContainsKey(type))
            {
                windows.Add(type, new T());
                windows[type].Closing += Helper_WindowClosing;
            }
            windows[type].Show();
            windows[type].Activate();
        }

        public static void SignAndShowInformation(Transaction tx)
        {
            if (tx == null)
            {
                MessageBox.Show(Strings.InsufficientFunds);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(tx);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(Strings.UnsynchronizedBlock);
                return;
            }

            App.CurrentWallet.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();
                App.CurrentWallet.SaveTransaction(tx);
                Program.LocalNode.Relay(tx);
                InformationBox.Show(tx.Hash.ToString(), Strings.SendTxSucceedMessage, Strings.SendTxSucceedTitle);
            }
            else
            {
                InformationBox.Show(context.ToString(), Strings.IncompletedSignatureMessage, Strings.IncompletedSignatureTitle);
            }
        }
    }
}