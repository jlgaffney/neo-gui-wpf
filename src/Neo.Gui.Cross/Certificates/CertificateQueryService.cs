using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Neo.SmartContract;
using Neo.Wallets;
using ECCurve = Neo.Cryptography.ECC.ECCurve;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Gui.Cross.Certificates
{
    internal class CertificateQueryService : ICertificateQueryService
    {
        private readonly Dictionary<UInt160, CertificateQueryResult> results = new Dictionary<UInt160, CertificateQueryResult>();

        private readonly ISettings settings;

        public CertificateQueryService(
            ISettings settings)
        {
            this.settings = settings;

            Directory.CreateDirectory(settings.Paths.CertCache);
        }

        public CertificateQueryResult Query(ECPoint pubkey)
        {
            return Query(Contract.CreateSignatureRedeemScript(pubkey).ToScriptHash());
        }

        public CertificateQueryResult Query(UInt160 hash)
        {
            lock (results)
            {
                if (results.ContainsKey(hash)) return results[hash];
                results[hash] = new CertificateQueryResult { Type = CertificateQueryResultType.Querying };
            }

            var address = hash.ToAddress();
            var path = Path.Combine(settings.Paths.CertCache, $"{address}.cer");

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
                web.DownloadDataCompleted += Web_DownloadDataCompleted;
                web.DownloadDataAsync(new Uri(url), hash);
            }

            // TODO Fix synchronisation warning
            return results[hash];
        }

        private void UpdateResultFromFile(UInt160 hash)
        {
            var address = hash.ToAddress();
            X509Certificate2 cert;
            try
            {
                cert = new X509Certificate2(Path.Combine(settings.Paths.CertCache, $"{address}.cer"));
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
                    var address = hash.ToAddress();
                    var path = Path.Combine(settings.Paths.CertCache, $"{address}.cer");
                    File.WriteAllBytes(path, e.Result);
                    lock (results)
                    {
                        UpdateResultFromFile(hash);
                    }
                }
            }
        }
    }
}
