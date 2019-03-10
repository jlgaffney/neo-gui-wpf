using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            var name = data.GetType().FullName.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type);
            }
            else
            {
                // TODO Replace with exception
                return new TextBlock { Text = "View Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}