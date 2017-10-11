using System;
using System.Collections.Generic;
using System.ComponentModel;
using Neo.UI.Base.Controls;

namespace Neo.UI.Base.Helpers
{
    public static class WindowHelper
    {
        private static readonly Dictionary<Type, NeoWindow> Windows = new Dictionary<Type, NeoWindow>();

        private static void Helper_WindowClosing(object sender, CancelEventArgs e)
        {
            Windows.Remove(sender.GetType());
        }

        public static void Show<T>() where T : NeoWindow, new()
        {
            var type = typeof(T);
            if (!Windows.ContainsKey(type))
            {
                Windows.Add(type, new T());
                Windows[type].Closing += Helper_WindowClosing;
            }
            Windows[type].Show();
            Windows[type].Activate();
        }
    }
}