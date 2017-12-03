namespace Neo.Gui.Base.Messages
{
    public class UpdateApplicationMessage
    {
        public UpdateApplicationMessage(string updateScriptPath)
        {
            this.UpdateScriptPath = updateScriptPath;
        }

        public string UpdateScriptPath { get; }
    }
}