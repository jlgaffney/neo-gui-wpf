using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Neo.Core;
using Neo.SmartContract;
using Neo.Wallets;
using ECCurve = Neo.Cryptography.ECC.ECCurve;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Gui.Base.Certificates
{
    internal class CertificateQueryService : ICertificateQueryService
    {
        private readonly Dictionary<UInt160, CertificateQueryResult> results = new Dictionary<UInt160, CertificateQueryResult>();

        private string certCachePath;
        private bool initialized;

        public void Initialize(string certificateCachePath)
        {
            this.certCachePath = certificateCachePath;

            Directory.CreateDirectory(this.certCachePath);

            this.initialized = true;
        }

        public CertificateQueryResult Query(ECPoint pubkey)
        {
            return Query(Contract.CreateSignatureRedeemScript(pubkey).ToScriptHash());
        }

        public CertificateQueryResult Query(UInt160 hash)
        {
            if (!this.initialized)
            {
                throw new Exception("Service has not been initialized!");
            }

            lock (results)
            {
                if (results.ContainsKey(hash)) return results[hash];
                results[hash] = new CertificateQueryResult { Type = CertificateQueryResultType.Querying };
            }
            var address = Wallet.ToAddress(hash);
            var path = Path.Combine(this.certCachePath, $"{address}.cer");
            if (File.Exists(path))
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

        #region Private methods

        private void UpdateResultFromFile(UInt160 hash)
        {
            var address = Wallet.ToAddress(hash);
            X509Certificate2 cert;
            try
            {
                cert = new X509Certificate2(Path.Combine(this.certCachePath, $"{address}.cer"));
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
            if (!hash.Equals(Contract.CreateSignatureRedeemScript(ECPoint.DecodePoint(cert.PublicKey.EncodedKeyValue.RawData, ECCurve.Secp256r1)).ToScriptHash()))
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
                    var address = Wallet.ToAddress(hash);
                    var path = Path.Combine(this.certCachePath, $"{address}.cer");
                    File.WriteAllBytes(path, e.Result);
                    lock (results)
                    {
                        this.UpdateResultFromFile(hash);
                    }
                }
            }
        }

        #endregion
    }
}
