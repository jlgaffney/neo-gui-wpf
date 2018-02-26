namespace Neo.UI.Core.Services.Interfaces
{
    public interface IProcessManager
    {
        void Run(string path);

        void OpenInExternalBrowser(string url);

        void Exit();

        void Restart();
    }
}