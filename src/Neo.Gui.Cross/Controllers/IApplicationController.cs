namespace Neo.Gui.Cross.Controllers
{
    public interface IApplicationController
    {
        bool IsRunning { get; }

        void Start();

        void Stop();
    }
}
