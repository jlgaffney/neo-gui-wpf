using System.IO;
using Neo.Network;

namespace Neo
{
    public static class PeerState
    {
        private const string PeerStatePath = "peers.dat";

        public static void TryLoad()
        {
            if (!File.Exists(PeerStatePath)) return;

            using (var fileStream = new FileStream(PeerStatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LocalNode.LoadState(fileStream);
            }
        }

        public static void Save()
        {
            using (var fileStream = new FileStream(PeerStatePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                LocalNode.SaveState(fileStream);
            }
        }
    }
}
