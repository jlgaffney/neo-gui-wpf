namespace Neo.UI.Messages
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