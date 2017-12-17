namespace Neo.Gui.Base.Helpers
{
    public interface IProcessHelper
    {
        void Run(string path);

        void OpenInExternalBrowser(string url);

        void Restart();
    }
}