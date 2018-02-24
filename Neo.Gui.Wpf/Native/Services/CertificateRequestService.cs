using System;
using System.Linq;
using System.Text;
using CERTENROLLLib;
using Neo.UI.Core.Services.Interfaces;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Native.Services
{
    public class CertificateRequestService : ICertificateRequestService
    {
        public byte[] Request(KeyPair key, string cn, string c, string s, string serialNumber)
        {
            var publicKey = key.PublicKey.EncodePoint(false).Skip(1).ToArray();

            byte[] privateKey;
            using (key.Decrypt())
            {
                const int ECDSA_PRIVATE_P256_MAGIC = 0x32534345;
                privateKey = BitConverter.GetBytes(ECDSA_PRIVATE_P256_MAGIC).Concat(BitConverter.GetBytes(32)).Concat(publicKey).Concat(key.PrivateKey).ToArray();
            }

            var x509Key = new CX509PrivateKey();

            // Set property using Reflection so this project can compile if this property isn't available
            var property = typeof(CX509PrivateKey).GetProperty("AlgorithmName");

            if (property == null)
            {
                // TODO Find a way to generate a certificate without setting this property
            }
            else
            {
                property.SetValue(x509Key, "ECDSA_P256", null);
            }

            x509Key.Import("ECCPRIVATEBLOB", Convert.ToBase64String(privateKey));

            Array.Clear(privateKey, 0, privateKey.Length);

            var request = new CX509CertificateRequestPkcs10();

            request.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, x509Key, null);
            request.Subject = new CX500DistinguishedName();
            request.Subject.Encode($"CN={cn},C={c},S={s},SERIALNUMBER={serialNumber}");
            request.Encode();

            var certificateText = "-----BEGIN NEW CERTIFICATE REQUEST-----\r\n" + request.RawData + "-----END NEW CERTIFICATE REQUEST-----\r\n";
            var certificateBytes = Encoding.UTF8.GetBytes(certificateText);

            return certificateBytes;
        }
    }
}
