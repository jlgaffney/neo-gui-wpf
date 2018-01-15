namespace Neo.UI.Core.Managers.Interfaces
{
    public interface IProcessManager
    {
        void Run(string path);

        void OpenInExternalBrowser(string url);

        void Exit();

        void Restart();
    }
}