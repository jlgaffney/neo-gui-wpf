using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Updater;

namespace Neo.Gui.Wpf.Views.Updater
{
    public partial class UpdateView : IDialog<UpdateLoadParameters>
    {
        public UpdateView()
        {
            InitializeComponent();
        }
    }
}