using Neo.Core;
using Neo.Properties;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

using Neo.UI.Controls;
using Neo.UI.Dialogs;

namespace Neo.UI
{
    internal static class Helper
    {
        private static Dictionary<Type, Form> toolForms = new Dictionary<Type, Form>();

        private static Dictionary<Type, NeoWindow> toolWindows = new Dictionary<Type, NeoWindow>();

        private static void Helper_FormClosing(object sender, FormClosingEventArgs e)
        {
            toolForms.Remove(sender.GetType());
        }

        public static void ShowForm<T>() where T : Form, new()
        {
            Type t = typeof(T);
            if (!toolForms.ContainsKey(t))
            {
                toolForms.Add(t, new T());
                toolForms[t].FormClosing += Helper_FormClosing;
            }
            toolForms[t].Show();
            toolForms[t].Activate();
        }

        private static void Helper_WindowClosing(object sender, CancelEventArgs e)
        {
            toolWindows.Remove(sender.GetType());
        }

        public static void Show<T>() where T : NeoWindow, new()
        {
            Type t = typeof(T);
            if (!toolWindows.ContainsKey(t))
            {
                toolWindows.Add(t, new T());
                toolWindows[t].Closing += Helper_WindowClosing;
            }
            toolWindows[t].Show();
            toolWindows[t].Activate();
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
