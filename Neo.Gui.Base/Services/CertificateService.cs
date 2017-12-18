using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

using Neo.Core;
using Neo.SmartContract;
using Neo.Wallets;

using Neo.Gui.Base.Certificates;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;

using ECCurve = Neo.Cryptography.ECC.ECCurve;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Gui.Base.Services
{
    internal class CertificateService : ICertificateService
    {
        private readonly IDirectoryManager directoryManager;
        private readonly IFileManager fileManager;
        private readonly IProcessHelper processHelper;

        private readonly Dictionary<UInt160, CertificateQueryResult> results = new Dictionary<UInt160, CertificateQueryResult>();

        private string certCachePath;
        private bool initialized;

        public CertificateService(
            IDirectoryManager directoryManager,
            IFileManager fileManager,
            IProcessHelper processHelper)
        {
            this.directoryManager = directoryManager;
            this.fileManager = fileManager;
            this.processHelper = processHelper;
        }

        public void Initialize(string certificateCachePath)
        {
            this.certCachePath = certificateCachePath;

            if (!this.directoryManager.DirectoryExists(certificateCachePath))
            {
                this.directoryManager.Create(certificateCachePath);
            }

            this.initialized = true;
        }

        public CertificateQueryResult GetCertificate(ECPoint publickey)
        {
            if (!this.initialized)
            {
                throw new Exception("Service has not been initialized!");
            }

            var hash = GetRedeemScriptHashFromPublicKey(publickey);

            lock (results)
            {
                if (results.ContainsKey(hash)) return results[hash];
                results[hash] = new CertificateQueryResult { Type = CertificateQueryResultType.Querying };
            }

            var path = this.GetCachedCertificatePathFromScriptHash(hash);
            var address = Wallet.ToAddress(hash);

            if (this.fileManager.FileExists(path))
            {
                lock (results)
                {
                    UpdateResultFromFile(hash);
                }
            }
            else
            {
                var url = $"http://cert.onchain.com/antshares/{address}.cer";
                var web = new WebClient();
                web.DownloadDataCompleted += this.Web_DownloadDataCompleted;
                web.DownloadDataAsync(new Uri(url), hash);
            }
            return results[hash];
        }

        public bool ViewCertificate(ECPoint publicKey)
        {
            if (!this.initialized)
            {
                throw new Exception("Service has not been initialized!");
            }

            var hash = GetRedeemScriptHashFromPublicKey(publicKey);

            var path = this.GetCachedCertificatePathFromScriptHash(hash);

            if (!this.fileManager.FileExists(path)) return false;

            this.processHelper.Run(path);

            return true;
        }

        public X509Certificate2 GenerateCertificate(KeyPair key, string cn, string c, string s)
        {
            var certificateGenerator = new X509V3CertificateGenerator();

            // Generate random serial number
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);
            
            // Set subject and issuer as same name
            var distinguishedName = new X509Name($"CN={cn},C={c},ST={s}");
            certificateGenerator.SetIssuerDN(distinguishedName);
            certificateGenerator.SetSubjectDN(distinguishedName);

            // Get key parameters
            var publicKey = key.PublicKey.EncodePoint(false).Skip(1).ToArray();
            byte[] privateKey;
            using (key.Decrypt())
            {
                privateKey = key.PrivateKey;
            }
            var publicParameters = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(publicKey);
            var privateParameters = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(privateKey);

            // Set public key
            certificateGenerator.SetPublicKey(publicParameters);

            // Generate certificate
            var signatureFactory = new Asn1SignatureFactory("SHA256WITHECDSA", privateParameters);
            var certificate = certificateGenerator.Generate(signatureFactory);

            var certificateBytes = certificate.GetEncoded();

            return new X509Certificate2(certificateBytes);

            
            



            /*var x509Key = new CX509PrivateKey();

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
            request.Subject.Encode($"CN={this.CN},C={this.C},S={this.S},SERIALNUMBER={this.SerialNumber}");
            request.Encode();

            var certificateText = "-----BEGIN NEW CERTIFICATE REQUEST-----\r\n" + request.RawData + "-----END NEW CERTIFICATE REQUEST-----\r\n";
            var certificateBytes = Encoding.UTF8.GetBytes(certificateText);*/
        }

        #region Private methods

        private void UpdateResultFromFile(UInt160 hash)
        {
            var path = this.GetCachedCertificatePathFromScriptHash(hash);

            X509Certificate2 cert;
            try
            {
                cert = new X509Certificate2(path);
            }
            catch (CryptographicException)
            {
                results[hash].Type = CertificateQueryResultType.Missing;
                return;
            }

            if (cert.PublicKey.Oid.Value != "1.2.840.10045.2.1")
            {
                results[hash].Type = CertificateQueryResultType.Missing;
                return;
            }

            // Compare hash with cached value
            var decodedPublicKey = ECPoint.DecodePoint(cert.PublicKey.EncodedKeyValue.RawData, ECCurve.Secp256r1);
            var decodedHash = GetRedeemScriptHashFromPublicKey(decodedPublicKey);

            if (!hash.Equals(decodedHash))
            {
                results[hash].Type = CertificateQueryResultType.Missing;
                return;
            }

            using (var chain = new X509Chain())
            {
                results[hash].Certificate = cert;
                if (chain.Build(cert))
                {
                    results[hash].Type = CertificateQueryResultType.Good;
                }
                else if (chain.ChainStatus.Length == 1 && chain.ChainStatus[0].Status == X509ChainStatusFlags.NotTimeValid)
                {
                    results[hash].Type = CertificateQueryResultType.Expired;
                }
                else
                {
                    results[hash].Type = CertificateQueryResultType.Invalid;
                }
            }
        }

        private void Web_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!this.initialized) return;

            using ((WebClient)sender)
            {
                var hash = (UInt160)e.UserState;
                if (e.Cancelled || e.Error != null)
                {
                    lock (results)
                    {
                        results[hash].Type = CertificateQueryResultType.Missing;
                    }
                }
                else
                {
                    var path = this.GetCachedCertificatePathFromScriptHash(hash);

                    this.fileManager.WriteAllBytes(path, e.Result);

                    lock (results)
                    {
                        this.UpdateResultFromFile(hash);
                    }
                }
            }
        }

        private static UInt160 GetRedeemScriptHashFromPublicKey(ECPoint publicKey)
        {
            return Contract.CreateSignatureRedeemScript(publicKey).ToScriptHash();
        }

        private string GetCachedCertificatePathFromScriptHash(UInt160 scriptHash)
        {
            var address = Wallet.ToAddress(scriptHash);

            return Path.Combine(this.certCachePath, $"{address}.cer");
        }

        #endregion
    }
}
