using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Updater;

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