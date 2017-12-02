namespace Neo.Gui.Base.Helpers.Interfaces
{
    public interface IProcessHelper
    {
        void Run(string path);

        void OpenInExternalBrowser(string url);

        void Restart();
    }
}