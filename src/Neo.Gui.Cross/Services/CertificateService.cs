using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Neo.Wallets;

namespace Neo.Gui.Cross.Services
{
    public class CertificateService : ICertificateService
    {
        public IEnumerable<X509Certificate2> GetStoreCertificates()
        {
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                return store.Certificates
                    .Cast<X509Certificate2>()
                    .ToList();
            }
        }

        public byte[] CreateCertificate(KeyPair keyPair, string cn, string c, string s, string serialNumber)
        {
            /*var publicKey = key.PublicKey.EncodePoint(false).Skip(1).ToArray();

            const int ECDSA_PRIVATE_P256_MAGIC = 0x32534345;
            var certPrivateKey = BitConverter.GetBytes(ECDSA_PRIVATE_P256_MAGIC).Concat(BitConverter.GetBytes(32)).Concat(publicKey).Concat(key.PrivateKey).ToArray();
            
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

            x509Key.Import("ECCPRIVATEBLOB", Convert.ToBase64String(certPrivateKey));

            Array.Clear(certPrivateKey, 0, certPrivateKey.Length);

            var request = new CX509CertificateRequestPkcs10();

            request.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, x509Key, null);
            request.Subject = new CX500DistinguishedName();
            request.Subject.Encode($"CN={cn},C={c},S={s},SERIALNUMBER={serialNumber}");
            request.Encode();

            var certificateText = "-----BEGIN NEW CERTIFICATE REQUEST-----\r\n" + request.RawData + "-----END NEW CERTIFICATE REQUEST-----\r\n";
            var certificateBytes = Encoding.UTF8.GetBytes(certificateText);

            return certificateBytes;*/

            // TODO Implement

            /*var key = ECDsa.Create();

            var subject = new X500DistinguishedName($"CN={cn},C={c},S={s},SERIALNUMBER={serialNumber}");

            var request = new CertificateRequest(subject, key, Ha);*/

            return new byte[0];
        }
    }
}
