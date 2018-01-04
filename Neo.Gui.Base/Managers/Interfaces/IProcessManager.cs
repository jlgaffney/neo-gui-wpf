namespace Neo.Gui.Base.Managers.Interfaces
{
    public interface IProcessManager
    {
        void Run(string path);

        void OpenInExternalBrowser(string url);

        void Restart();
    }
}