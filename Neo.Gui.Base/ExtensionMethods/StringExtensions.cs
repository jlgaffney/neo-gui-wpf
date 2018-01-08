using Neo.Cryptography.ECC;

namespace Neo.Gui.Base.ExtensionMethods
{
    public static class StringExtensions
    {
        public static ECPoint ToECPoint(this string publicKey)
        {
            return ECPoint.DecodePoint(publicKey.HexToBytes(), ECCurve.Secp256r1);
        }
    }
}
