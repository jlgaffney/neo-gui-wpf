namespace Neo.Gui.Base.Updating
{
    public class ReleaseInfo
    {
        public ReleaseInfo(string downloadUrl, string changes)
        {
            this.DownloadUrl = downloadUrl;
            this.Changes = changes;
        }

        public string DownloadUrl { get; }

        public string Changes { get; }
    }
}
