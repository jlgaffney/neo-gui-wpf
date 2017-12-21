using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Neo.Core;
using Neo.SmartContract;
using Neo.Wallets;
using Neo.Gui.Base.Managers;

using ECCurve = Neo.Cryptography.ECC.ECCurve;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Gui.Base.Certificates
{
    internal class CertificateService : ICertificateService
    {
        #region Private Fields 
        private readonly IDirectoryManager directoryManager;
        private readonly IFileManager fileManager;

        private readonly Dictionary<UInt160, CertificateQueryResult> results = new Dictionary<UInt160, CertificateQueryResult>();

        private string certCachePath;
        private bool initialized;
        #endregion

        #region Constructor 
        public CertificateService(
            IDirectoryManager directoryManager,
            IFileManager fileManager)
        {
            this.directoryManager = directoryManager;
            this.fileManager = fileManager;
        }
        #endregion

        #region Public Methods 
        public void Initialize(string certificateCachePath)
        {
            this.certCachePath = certificateCachePath;

            if (!this.directoryManager.DirectoryExists(certificateCachePath))
            {
                this.directoryManager.Create(certificateCachePath);
            }

            this.initialized = true;
        }

        public CertificateQueryResult Query(ECPoint publickey)
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

        public string GetCachedCertificatePath(ECPoint publicKey)
        {
            if (!this.initialized)
            {
                throw new Exception("Service has not been initialized!");
            }

            var hash = GetRedeemScriptHashFromPublicKey(publicKey);

            var path = this.GetCachedCertificatePathFromScriptHash(hash);

            if (!this.fileManager.FileExists(path)) return null;
            return path;
        }
        #endregion

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
