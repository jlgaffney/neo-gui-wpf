namespace Neo.Gui.Wpf.DialogResults
{
    public class YesOrNoDialogResult
    {
        public bool Yes { get; private set; }

        public YesOrNoDialogResult(bool yes)
        {
            this.Yes = yes;
        }
    }
}
