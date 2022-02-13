using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using LightningLib.lndrpc;
using System.Threading;
using System.Net.Http;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using QRCoder;
using System.Drawing;
using System.Configuration;

namespace LightningNetworkTests
{
    [TestClass]
    public class lndTests
    {
        // These are sensitive: load from config
        static Dictionary<bool, string> MacaroonAdmin = new Dictionary<bool, string>()
        {
            {true, ConfigurationManager.AppSettings["LnTestnetMacaroonAdmin"] },
            {false, ConfigurationManager.AppSettings["LnMainnetMacaroonAdmin"] },
        };

        static Dictionary<bool, string> MacaroonInvoice = new Dictionary<bool, string>()
        {
            {true, ConfigurationManager.AppSettings["LnTestnetMacaroonInvoice"] },
            {false, ConfigurationManager.AppSettings["LnTestnetMacaroonInvoice"] },
        };

        // true=testnet
        static Dictionary<bool, string> MacaroonRead = new Dictionary<bool, string>()
        {
            {true, ConfigurationManager.AppSettings["LnTestnetMacaroonRead"] },
            {false, ConfigurationManager.AppSettings["LnMainnetMacaroonRead"] },
        };

        [TestMethod]
        public void VerifyAppDomainHasConfigurationSettings()
        {
            string value = ConfigurationManager.AppSettings["UseTestNet"];
            Assert.IsFalse(String.IsNullOrEmpty(value), "No App.Config found.");
        }

        /// <summary>
        /// This test checks that the QRCode library is creating bitmaps
        /// </summary>
        [TestMethod]
        public void LN_QR_Create()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            Assert.IsNotNull(qrCodeImage);
        }

        /// <summary>
        /// This method checks that the getnodeinfo method returns a response over the API
        /// </summary>
        [TestMethod]
        public void Test_CallGetnodeinfoAsStringMainnet()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/graph/node/{pub_key}";

            //This may not exist
            string pubkey = "032925faba461d86f51c2e019fce9f8929795bf974b5114a73b5c8ad263d6a2c5e";

            string response = LndApiGet(
                host: host,
                restpath: restpath,
                urlParameters: new Dictionary<string, string>() {{"pub_key", pubkey}},
                macaroonRead: MacaroonRead[false]);

            Console.WriteLine(response);
        }

        [TestMethod]
        public void TestAPI_Mainnet_Node_CallGetnodeinfo_AsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];

            string pubkey = "032925faba461d86f51c2e019fce9f8929795bf974b5114a73b5c8ad263d6a2c5e";

            var client = new LndRpcClient(host, macaroonRead: MacaroonRead[false]);
            var ni = client.GetNodeInfo(pubkey);

            Console.WriteLine(ni.node.alias);
        }

        [TestMethod]
        public void TestAPI_Mainnet_GetForwardingEvents()
        {
            bool useTestnet = false;
            var lndclient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetHost" : "LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonAdmin" : "LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonRead" : "LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonInvoice" : "LnMainnetMacaroonInvoice"]);

            var fwd = lndclient.GetForwardingEvents();
            Console.WriteLine(fwd.ToString());
        }

        [TestMethod]
        public void TestAPI_Mainnet_GetInvoice_URLpath()
        {
            bool useTestnet = false;
            var lndclient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetHost" : "LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonAdmin" : "LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonRead" : "LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonInvoice" : "LnMainnetMacaroonInvoice"]);

            string rhash = "GgImlNqPxTPNbmr6b/YnEYHA9G0y4GWDhrvZwj97bhw="; // "E2UuciQ9iUiXLRY/BgHIa+sCaim3o24RbP+iwWQTX7Y=";// "EjMZEQRjlPf /f7aVmpjQBUFCZg/o1dH1GUQiu99Hm7s=";// "NlorlO9GxWXuPfkFsuPh7oaW5HHtDVePMj4YOtGpr2E=";

            var inv = lndclient.GetInvoice(rhash: rhash, useQuery: false);
            Console.WriteLine(inv.ToString());
        }

        [TestMethod]
        public void TestAPI_Mainnet_GetInvoice_URLquery()
        {
            bool useTestnet = false;
            var lndclient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetHost" : "LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonAdmin" : "LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonRead" : "LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonInvoice" : "LnMainnetMacaroonInvoice"]);

            string rhash = "GgImlNqPxTPNbmr6b/YnEYHA9G0y4GWDhrvZwj97bhw="; // "E2UuciQ9iUiXLRY/BgHIa+sCaim3o24RbP+iwWQTX7Y=";// "EjMZEQRjlPf /f7aVmpjQBUFCZg/o1dH1GUQiu99Hm7s=";// "NlorlO9GxWXuPfkFsuPh7oaW5HHtDVePMj4YOtGpr2E=";

            var inv = lndclient.GetInvoice(rhash: rhash, useQuery: true);
            Console.WriteLine(inv.ToString());
        }

        /// <summary>
        /// Test method which returns all of the invoices on the LND node
        /// </summary>
        [TestMethod]
        public void TestAPI_Mainnet_GetInvoices()
        {
            bool useTestnet = false;
            var lndclient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetHost" : "LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonAdmin" : "LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonRead" : "LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonInvoice" : "LnMainnetMacaroonInvoice"]);

            var invs = lndclient.GetInvoices(
                pendingOnly: false);
            Console.WriteLine(invs.ToString());
        }

        /// <summary>
        /// Test method which returns all of the invoices on the LND node
        /// </summary>
        [TestMethod]
        public void TestAPI_Mainnet_GetPayments()
        {
            bool useTestnet = false;
            var lndclient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetHost" : "LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonAdmin" : "LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonRead" : "LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonInvoice" : "LnMainnetMacaroonInvoice"]);

            var invs = lndclient.GetPayments(max_payments: 10);
            Console.WriteLine(invs.payments.Count);
        }

        [TestMethod]
        public void TestAPI_Mainnet_Invoice_CreateInvoice()
        {
            //request a new invoice
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/invoices";
            var invoice = new Invoice()
            {
                value = "1000",
                memo = "Testing",
                expiry = "432000",
            };

            //admin
            string responseStr = LndRpcClient.LndApiPostStr(host, restpath, invoice, adminMacaroon: MacaroonAdmin[false]);
            Console.WriteLine(responseStr);
        }

        [TestMethod]
        public void TestAPI_Mainnet_Invoice_DecodeInvoice()
        {
            bool useTestnet = false;
            var lndclient = new LndRpcClient(
                    host: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetHost" : "LnMainnetHost"],
                    macaroonAdmin: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonAdmin" : "LnMainnetMacaroonAdmin"],
                    macaroonRead: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonRead" : "LnMainnetMacaroonRead"],
                    macaroonInvoice: System.Configuration.ConfigurationManager.AppSettings[useTestnet ? "LnTestnetMacaroonInvoice" : "LnMainnetMacaroonInvoice"]);

            //string payreq = "lnbc90n1pdd4xzqpp5j69daa4ep5nfzgx0pdfajcfn96ysjts7ekdhlcw4kt6g8w2ff79qdqc235xjueqd9ejqmteypkk2mt0cqzys2vk4ecwl0lf0dhwplrphvznpmkw6ehv6p3w5rtfux7u9963azu0hmg3fhn4w85qugxapecqf7dmehajxtk9c5zvxw22l77vr2j645qcqs8yrq6";

            string payreq = "lnbc10n1p3qs0nmpp5rgpzd9x63lzn8ntwdtaxla38zxqupardxtsxtquxh0vuy0mmdcwqdphxvu8wnn4w4mhsem8232hy6zd2su85kp5tpux66rxdecrwarsgd4hs6ccqzpgxqyd9uqsp5ndfnn3t523cmzkg7qt3gp9y5ed939q8xxhklmpu8tgpsnjjskqgs9qyyssqk2gdhd8a0nwa8fr74aa4p8n7grrw7kvqelf97ktn33cfjetvfglnj9z2cljd7vyn2cg49crkatyamz628gmawu7qwm68zefymrcdcuqpafc9u2";

            var response = lndclient.DecodePayment(payreq);

            Assert.IsTrue(response != null);
        }

        [TestMethod]
        public void TestAPI_Mainnet_Invoice_DecodeInvoice_AsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/payreq/{pay_req}";

            // This one includes a memo
            string payreq = "lnbc90n1pdd4xzqpp5j69daa4ep5nfzgx0pdfajcfn96ysjts7ekdhlcw4kt6g8w2ff79qdqc235xjueqd9ejqmteypkk2mt0cqzys2vk4ecwl0lf0dhwplrphvznpmkw6ehv6p3w5rtfux7u9963azu0hmg3fhn4w85qugxapecqf7dmehajxtk9c5zvxw22l77vr2j645qcqs8yrq6";

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {"pay_req",  payreq},
            };

            string responseStr = LndRpcClient.LndApiGetStr(host, restpath, urlParameters: parameters, adminMacaroon: MacaroonRead[false]);
            Console.WriteLine(responseStr);
            //string expected = "{\"destination\":\"03a9d79bcfab7feb0f24c3cd61a57f0f00de2225b6d31bce0bc4564efa3b1b5aaf\",\"payment_hash\":\"968adef6b90d269120cf0b53d961332e89092e1ecd9b7fe1d5b2f483b9494f8a\",\"num_satoshis\":\"9\",\"timestamp\":\"1524275264\",\"expiry\":\"3600\",\"description\":\"This is my memo\",\"cltv_expiry\":\"144\"}";
            string expected = "{\"destination\":\"03a9d79bcfab7feb0f24c3cd61a57f0f00de2225b6d31bce0bc4564efa3b1b5aaf\", \"payment_hash\":\"968adef6b90d269120cf0b53d961332e89092e1ecd9b7fe1d5b2f483b9494f8a\", \"num_satoshis\":\"9\", \"timestamp\":\"1524275264\", \"expiry\":\"3600\", \"description\":\"This is my memo\", \"description_hash\":\"\", \"fallback_addr\":\"\", \"cltv_expiry\":\"144\", \"route_hints\":[], \"payment_addr\":\"\", \"num_msat\":\"9000\", \"features\":{}}";
            Assert.AreEqual(expected, responseStr);
        }

        #region testnet

        [TestMethod]
        public void TestAPIDecodeInvoiceTestnetAsString()
        {
            string host = ConfigurationManager.AppSettings["LnTestnetHost"];
            string restpath = "/v1/payreq/{pay_req}";

            string payreq = "lntb4m1pdv9jf4pp5dnk8sq4d0hg0rwwge4l09zjg7mwkz2kdy8z6qynq332l300405rsdqqcqzysz376stul9zuxersermhtedgga3dzq0pmzh7zddz3wvd0kuzsldmzl4aefcn6ph8d4hlfxlesgn9h4j0m2zl2kajc2kn4yv3nyv96n0cpfv2kuw";
            //"lnbc90n1pdd4xzqpp5j69daa4ep5nfzgx0pdfajcfn96ysjts7ekdhlcw4kt6g8w2ff79qdqc235xjueqd9ejqmteypkk2mt0cqzys2vk4ecwl0lf0dhwplrphvznpmkw6ehv6p3w5rtfux7u9963azu0hmg3fhn4w85qugxapecqf7dmehajxtk9c5zvxw22l77vr2j645qcqs8yrq6";
            //

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {"pay_req",  payreq},
            };

            string responseStr = LndRpcClient.LndApiGetStr(host, restpath, urlParameters:parameters, adminMacaroon: MacaroonRead[true]);
            Console.WriteLine(responseStr);
        }

        [TestMethod]
        public void TestAPIPayInvoiceTestnetAsString()
        {
            string host = ConfigurationManager.AppSettings["LnTestnetHost"];
            string payreq = "lntb50n1pdv9nlppp5unpfcqu88d07g7raaqs9dgpc47vznsz63pa8l4nmf5yuancxutmqdqqcqzysgrkr424w8s3kgpvn3xzcg7xth3ax4uuu5vduha3z5ullv522e56qetr6hmlvqmdydxdawrszwa52hfntmajghf7u5nppvw93keqhsecpqsydu6";

            //Need to first decode the payment request
            var client = new LndRpcClient(host, macaroonAdmin: MacaroonAdmin[true], macaroonRead: MacaroonRead[true]);
            var payment = client.DecodePayment(payreq);

            string restpath = "/v1/channels/transactions";

            var payreqParam = new { payment_request = payreq };

            string responseStr = LndRpcClient.LndApiPostStr(host, restpath, payreqParam, adminMacaroon: MacaroonAdmin[true]);
            Console.WriteLine(responseStr);
        }

        [TestMethod]
        public void TestLndRpcClientTestnet()
        {
            bool useTestnet = true;
            var LndClient = new LndRpcClient(
                macaroonAdmin: MacaroonAdmin[useTestnet]);

        }

        #endregion

        [TestMethod]
        public void TestAPIGetInfo()
        {
            string macaroon = ConfigurationManager.AppSettings["LnMainnetMacaroonRead"];

            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/getinfo";
            GetInfoResponse info = LndApiGetObj<GetInfoResponse>(host, restpath, mac: macaroon);
            Console.WriteLine(info.alias);
        }

        [TestMethod]
        public void TestParseMacaroons()
        {
            string readfile = ConfigurationManager.AppSettings["LnMainnetMacaroonReadFile"];
            string invoicefile = ConfigurationManager.AppSettings["LnMainnetMacaroonInvoiceFile"];
            string adminfile = ConfigurationManager.AppSettings["LnMainnetMacaroonAdminFile"];
            var m = System.IO.File.ReadAllBytes(readfile);
            DataEncoders.HexEncoder h = new DataEncoders.HexEncoder();
            var macaroon = h.EncodeData(m);

            Console.WriteLine("Invoice: " + h.EncodeData(System.IO.File.ReadAllBytes(invoicefile)));
            Console.WriteLine("Admin: " + h.EncodeData(System.IO.File.ReadAllBytes(adminfile)));
            Console.WriteLine("Readonly: " + macaroon);
        }

        private static T LndApiGetObj<T>(string host, string restpath, int port = 8080, string mac = "") where T: new()
        {
            string macaroon = mac;
            if (mac == "")
            {
                //var m = System.IO.File.ReadAllBytes("readonly.macaroon");
                //HexEncoder h = new HexEncoder();
                //macaroon = h.EncodeData(m);
            }
            //string TLSFilename = "tls.cert";
            //X509Certificate2 certificates = new X509Certificate2();
            //certificates.Import(System.IO.File.ReadAllBytes(TLSFilename));

            var client = new RestClient("https://" + host + ":" + Convert.ToString(port));
            //client.ClientCertificates = new X509CertificateCollection() { certificates };

            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadWrite);
            //store.Add(certificates);

            //client.RemoteCertificateValidationCallback =
            //    delegate (object s, X509Certificate certificate,
            //              X509Chain chain, SslPolicyErrors sslPolicyErrors)
            //    {
            //        //TODO: fix later
            //        return true;
            //    };

            var request = new RestRequest(restpath, Method.Get);
            request.AddHeader("Grpc-Metadata-macaroon", macaroon);

            var response = client.ExecuteAsync<T>(request).Result;
            T info = response.Data;
            return info;
        }

        [TestMethod]
        public void LN_APICall_Switch_AsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/switch";
            var reqObj = new FwdRequest()
            {
                start_time = "0",
                end_time = "999999999999",
                index_offset = 0,
                num_max_events = 50000,
            };

            string responseStr = LndRpcClient.LndApiPostStr(host, restpath, reqObj, 
                adminMacaroon: ConfigurationManager.AppSettings["LnMainnetMacaroonRead"]);
            Console.WriteLine(responseStr);
        }

        [TestMethod]
        public void TestAPILookupInvoiceAsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v2/invoices/lookup/";

            string paymentHashB64 = "lA5j2IPt9JWUIIObf5kiDlIhYbSnRTLbJKNa7LBogHs=";
            //string paymentHash = "1a022694da8fc533cd6e6afa6ff6271181c0f46d32e0658386bbd9c23f7b6e1c";
            string paymentHash = "b37a542d1c68b94e843f84b53076927c7d9c45753db4a83b62058703a8f3a840";

            Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    {"payment_hash",  paymentHash},
                };

            string responseStr = LndRpcClient.LndApiGetStr(
                host: host,
                restpath: restpath,
                urlParameters: null,
                queryParameters: parameters,
                adminMacaroon: ConfigurationManager.AppSettings["LnMainnetMacaroonAdmin"]);
            Console.WriteLine(responseStr);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAPIGetInvoiceAsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/invoice";

            //DataEncoders.HexEncoder h = new DataEncoders.HexEncoder();
            //string rhash = "lA5j2IPt9JWUIIObf5kiDlIhYbSnRTLbJKNa7LBogHs=";// "E2UuciQ9iUiXLRY/BgHIa+sCaim3o24RbP+iwWQTX7Y=";// "NlorlO9GxWXuPfkFsuPh7oaW5HHtDVePMj4YOtGpr2E=";
            //var rhash_bytes = Convert.FromBase64String(rhash);
            //var rhash_hex = h.EncodeData(rhash_bytes);

            var rhash_hex = "1a022694da8fc533cd6e6afa6ff6271181c0f46d32e0658386bbd9c23f7b6e1c";
            //var r_hash_bytes = h.DecodeData(rhash_hex);
            //var r_hash_b64 = Convert.ToBase64String(r_hash_bytes);

            string responseStr = LndRpcClient.LndApiGetStr(
                host: host, 
                restpath: restpath + "/" + rhash_hex,
                urlParameters: null, 
                adminMacaroon: ConfigurationManager.AppSettings["LnMainnetMacaroonAdmin"]);
            Console.WriteLine(responseStr);
        }
        // "{\"memo\":\"38wNuuwxggTUrhMT8zX4Xxmhfnp7tpCkxk\",
        // \"r_preimage\":\"ErsaUiucSGLyTGqS0c7Svn55tIHNLlHM97MU64hC5f8=\",
        // \"r_hash\":\"GgImlNqPxTPNbmr6b/YnEYHA9G0y4GWDhrvZwj97bhw=\",
        // \"value\":\"1\",
        // \"value_msat\":\"1000\",
        // \"settled\":true,
        // \"creation_date\":\"1644707451\",
        // \"settle_date\":\"1644707486\",
        // \"payment_request\":\"lnbc10n1p3qs0nmpp5rgpzd9x63lzn8ntwdtaxla38zxqupardxtsxtquxh0vuy0mmdcwqdphxvu8wnn4w4mhsem8232hy6zd2su85kp5tpux66rxdecrwarsgd4hs6ccqzpgxqyd9uqsp5ndfnn3t523cmzkg7qt3gp9y5ed939q8xxhklmpu8tgpsnjjskqgs9qyyssqk2gdhd8a0nwa8fr74aa4p8n7grrw7kvqelf97ktn33cfjetvfglnj9z2cljd7vyn2cg49crkatyamz628gmawu7qwm68zefymrcdcuqpafc9u2\",
        // \"description_hash\":\"\",
        // \"expiry\":\"432000\",
        // \"fallback_addr\":\"\",
        // \"cltv_expiry\":\"40\",
        // \"route_hints\":[],
        // \"private\":false,
        // \"add_index\":\"8437\",
        // \"settle_index\":\"4598\",
        // \"amt_paid\":\"1000\",
        // \"amt_paid_sat\":\"1\",
        // \"amt_paid_msat\":\"1000\",
        // \"state\":\"SETTLED\", \"htlcs\":[{\"chan_id\":\"772380530381422593\", \"htlc_index\":\"809\", \"amt_msat\":\"1000\", \"accept_height\":723017, \"accept_time\":\"1644707486\", \"resolve_time\":\"1644707486\", \"expiry_height\":723060, \"state\":\"SETTLED\", \"custom_records\":{}, \"mpp_total_amt_msat\":\"1000\", \"amp\":null}], \"features\":{\"9\":{\"name\":\"tlv-onion\", \"is_required\":false, \"is_known\":true}, \"14\":{\"name\":\"payment-addr\", \"is_required\":true, \"is_known\":true}, \"17\":{\"name\":\"multi-path-payments\", \"is_required\":false, \"is_known\":true}}, \"is_keysend\":false, \"payment_addr\":\"m1M5xXRUcbFZHgLigJSUy0sSgOY17f2Hh1oDCcpQsBE=\", \"is_amp\":false, \"amp_invoice_state\":{}}"

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestAPIGetPaymentsAsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/payments";

            Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    {"include_incomplete",  "true"},
                    {"max_payments",  "10"},
                    {"reversed",  "true"},
                };

            string responseStr = LndRpcClient.LndApiGetStr(
                host: host,
                restpath: restpath,
                urlParameters: null,
                queryParameters: parameters,
                adminMacaroon: ConfigurationManager.AppSettings["LnMainnetMacaroonAdmin"]);

            Console.WriteLine(responseStr);
        }
        //"{\"payments\":[{
        //\"payment_hash\":\"87a9bcb328c3612b576b26663ef63337f675da4fbcea62b52ed1800ce78cc950\",
        //\"value\":\"50\",
        //\"creation_date\":\"1644619793\",
        //\"fee\":\"2\",
        //\"payment_preimage\":\"32c58dfe239e3e9d51f3b2fa8eab87660f252ea6a209c03757ba6632083962fb\",
        //\"value_sat\":\"50\",
        //\"value_msat\":\"50000\",
        //\"payment_request\":\"lnbc500n1p3qdeazpp5s75mevegcdsjk4mtyenraa3nxlm8tkj0hn4x9dfw6xqqeeuve9gqdqu2askcmr9wssx7e3q2dshgmmndp5scqzpgxqyz5vqsp5ce4z5su5urxwpm5kds8rqwnulcnl2us0ttmlj9myegxfm8fku9aq9qyyssqr4fwm7lzrw8n3g0fmeknen9k67ckg0csnfxtku4fw2gretrk3z7qzm9eh8fram802zkek2cgx4qu54gd0ymkx4jkqjk7l5k6nqdejmsq8wuzz4\",
        //\"status\":\"SUCCEEDED\",
        //\"fee_sat\":\"2\",
        //\"fee_msat\":\"2097\",
        //\"creation_time_ns\":\"1644619793715827850\",
        //\"htlcs\":[{\"attempt_id\":\"130054\", \"status\":\"FAILED\", \"route\":{\"total_time_lock\":722908,
        //\"total_fees\":\"1\", \"total_amt\":\"51\",
        //\"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"50\", \"fee\":\"1\", \"expiry\":722868, \"amt_to_forward_msat\":\"50000\", \"fee_msat\":\"1050\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"787317395838795776\", \"chan_capacity\":\"50000000\", \"amt_to_forward\":\"50\", \"fee\":\"0\", \"expiry\":722868, \"amt_to_forward_msat\":\"50000\", \"fee_msat\":\"0\", \"pub_key\":\"035e4ff418fc8b5554c5d9eea66396c227bd429a3251c8cbc711002ba215bfc226\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"xmoqQ5TgzODulmwOMDp8/if1cg9a9/kXZMoMnZ024Xo=\", \"total_amt_msat\":\"50000\"}, \"amp_record\":null, \"custom_records\":{}}],
        //\"total_fees_msat\":\"1050\", \"total_amt_msat\":\"51050\"},
        //\"attempt_time_ns\":\"1644619793820771608\", \"resolve_time_ns\":\"1644619794364157596\",
        //\"failure\":{\"code\":\"TEMPORARY_CHANNEL_FAILURE\",
        //\"channel_update\":{\"signature\":\"6/GDiNc2EDU3P6s79oyxlS8PLQeYSuh6j/Kjf/FCzIZ2w7JfPMaZJN1Wh2xJ0Mw5/JQnq4+gW1efMR8Boq+QVg==\", \"chain_hash\":\"b+KMCrbxs3LBpqJGrmP3T5Meg2XhWgicaNYZAAAAAAA=\",
        //\"chan_id\":\"787317395838795776\", \"timestamp\":1643765189, \"message_flags\":1, \"channel_flags\":1, \"time_lock_delta\":40, \"htlc_minimum_msat\":\"1000\", \"base_fee\":1000,
        //\"fee_rate\":1000, \"htlc_maximum_msat\":\"49500000000\", \"extra_opaque_data\":\"\"}, \"htlc_msat\":\"0\", \"onion_sha_256\":\"\", \"cltv_expiry\":0, \"flags\":0, \"failure_source_index\":1,
        //\"height\":0}, \"preimage\":\"\"}, {\"attempt_id\":\"130055\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":723052, \"total_fees\":\"2\", \"total_amt\":\"52\", \"hops\":[{\"chan_id\":\"787851758494941185\",
        //\"chan_capacity\":\"8000000\", \"amt_to_forward\":\"51\", \"fee\":\"1\", \"expiry\":723012, \"amt_to_forward_msat\":\"51024\", \"fee_msat\":\"1073\",
        //\"pub_key\":\"03bb88ccc444534da7b5b64b4f7b15e1eccb18e102db0e400d4b9cfe93763aa26d\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}},
        //{\"chan_id\":\"754025283212935168\", \"chan_capacity\":\"41902328\", \"amt_to_forward\":\"50\", \"fee\":\"1\", \"expiry\":722868, \"amt_to_forward_msat\":\"50000\", \"fee_msat\":\"1024\",
        //\"pub_key\":\"03864ef025fde8fb587d989186ce6a4a186895ee44a926bfc370e2c366597a3f8f\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"787470227900661760\",
        //\"chan_capacity\":\"200000000\", \"amt_to_forward\":\"50\", \"fee\":\"0\", \"expiry\":722868, \"amt_to_forward_msat\":\"50000\", \"fee_msat\":\"0\", \"pub_key\":\"035e4ff418fc8b5554c5d9eea66396c227bd429a3251c8cbc711002ba215bfc226\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"xmoqQ5TgzODulmwOMDp8/if1cg9a9/kXZMoMnZ024Xo=\", \"total_amt_msat\":\"50000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"2097\", \"total_amt_msat\":\"52097\"}, \"attempt_time_ns\":\"1644619794475607288\", \"resolve_time_ns\":\"1644619795222989060\", \"failure\":null, \"preimage\":\"MsWN/iOePp1R87L6jquHZg8lLqaiCcA3V7pmMgg5Yvs=\"}], \"payment_index\":\"65180\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"92eb9114b0e680cc04553eea8207b2991128a48947ce8cfe5e7f4944d374364b\", \"value\":\"417\", \"creation_date\":\"1644640038\", \"fee\":\"0\", \"payment_preimage\":\"0000000000000000000000000000000000000000000000000000000000000000\", \"value_sat\":\"417\", \"value_msat\":\"417000\", \"payment_request\":\"lnbc4170n1p3qwdclpp5jt4ez99su6qvcpz48m4gypajnygj3fyfgl8gelj70ay5f5m5xe9sdqqcqzpgxqy9gcqtrw4k8cuvhsqq5estpn2yp67kwlsrejn0en5jfu825g2wcz2awepjhs8pq7w0c6kdyw6kx6p60565685ahdyw2eqxv99z63ytawpgyspdv3shm\", \"status\":\"FAILED\", \"fee_sat\":\"0\", \"fee_msat\":\"0\", \"creation_time_ns\":\"1644640038840697572\", \"htlcs\":[], \"payment_index\":\"65181\", \"failure_reason\":\"FAILURE_REASON_NO_ROUTE\"}, {\"payment_hash\":\"26f7cf36246acb17596b07490df5498f1afe39ae8665a0db5fdf4d938cbd84e9\", \"value\":\"417\", \"creation_date\":\"1644644019\", \"fee\":\"0\", \"payment_preimage\":\"0000000000000000000000000000000000000000000000000000000000000000\", \"value_sat\":\"417\", \"value_msat\":\"417000\", \"payment_request\":\"lnbc4170n1p3qw34tpp5ymmu7d3ydt93wkttqaysma2f3ud0uwdwsej6pk6lmaxe8r9asn5sdqqcqzpgxqy9gcqk3ayrnz5mutnpw688lkvumwe26jr0jqe9gfgtcxenqz4n8h0svfyw0azzeumzy426pf9xz8shsca5u46fjcmttjgqgnpkurergsty2spgpwr9w\", \"status\":\"FAILED\", \"fee_sat\":\"0\", \"fee_msat\":\"0\", \"creation_time_ns\":\"1644644019506488012\", \"htlcs\":[], \"payment_index\":\"65182\", \"failure_reason\":\"FAILURE_REASON_NO_ROUTE\"}, {\"payment_hash\":\"f6ac8bf870414bb67647fed23207c34a1ecf0e115388202647dc5dc2fb04b337\", \"value\":\"547\", \"creation_date\":\"1644656638\", \"fee\":\"1\", \"payment_preimage\":\"33a66cf6d4fcd088abc9ecd7166a2c4067a691540442908f9e82fa3f1659ac5f\", \"value_sat\":\"547\", \"value_msat\":\"547000\", \"payment_request\":\"lnbc5470n1p3qwakfpp576kgh7rsg99mvaj8lmfryp7rfg0v7rs32wyzqfj8m3wu97cykvmsdqqcqzpgxqyz5vqsp57866dh7yqyz6c6cek65fyl4naldz2uxd80gk5r4mh2e97zcejxes9qyyssqjfqszg367akhehz0hzarmxaw6aa2fkmf9er3zns2prkdgwj742k5nrtma07v2sp7vtmnqr8lvdyu9v65cddvzaqmpjjw2hgv79n07ggqyasn5z\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"1\", \"fee_msat\":\"1929\", \"creation_time_ns\":\"1644656638701186747\", \"htlcs\":[{\"attempt_id\":\"130056\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":722985, \"total_fees\":\"1\", \"total_amt\":\"548\", \"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"547\", \"fee\":\"1\", \"expiry\":722945, \"amt_to_forward_msat\":\"547000\", \"fee_msat\":\"1929\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"762456338409586689\", \"chan_capacity\":\"100000000\", \"amt_to_forward\":\"547\", \"fee\":\"0\", \"expiry\":722945, \"amt_to_forward_msat\":\"547000\", \"fee_msat\":\"0\", \"pub_key\":\"037cc5f9f1da20ac0d60e83989729a204a33cc2d8e80438969fadf35c1c5f1233b\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"8fWm38QBBaxrGbaokn6z79olcM070WoOu7qyXwsZkbM=\", \"total_amt_msat\":\"547000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"1929\", \"total_amt_msat\":\"548929\"}, \"attempt_time_ns\":\"1644656639150685054\", \"resolve_time_ns\":\"1644656642795338143\", \"failure\":null, \"preimage\":\"M6Zs9tT80IiryezXFmosQGemkVQEQpCPnoL6PxZZrF8=\"}], \"payment_index\":\"65183\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"a0b63b9a4b76da4ad6605cc8bd5c34f60bc37cae95cc43f2f3a25d58b7e97df4\", \"value\":\"7050\", \"creation_date\":\"1644659847\", \"fee\":\"5\", \"payment_preimage\":\"d550621b92befba0775854ca8d0e42a61ba0958f671636af1f5d7b1e451c9291\", \"value_sat\":\"7050\", \"value_msat\":\"7050000\", \"payment_request\":\"lnbc70500n1p3q0prupp55zmrhxjtwmdy44nqtnyt6hp57c9uxl9wjhxy8uhn5fw43dlf0h6qdzvgd5xzmn8v5s8g6r9yr3f4g00hz8jqcn4w36x7m3qw3hjq6npweshxcmjd9c8gwnpd3jhyapgxy5scqzpgxqzjcsp5q8x5egd0mzy9ymdzhdpj7usqza7qsfgdrpwmjha9c6rhug3pl5ts9qyyssq8apnqcvu33jfmtl3tn5ga5wte73e7yuztq5vtm3ap3zwrwc7xxfkr3humcrz5ptqqzh98mah43wy8v77f6x0g4qkuaz6nt4ntapnduqp6dl8ax\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"5\", \"fee_msat\":\"5279\", \"creation_time_ns\":\"1644659847998580579\", \"htlcs\":[{\"attempt_id\":\"130057\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":722996, \"total_fees\":\"5\", \"total_amt\":\"7055\", \"hops\":[{\"chan_id\":\"787851758494941185\", \"chan_capacity\":\"8000000\", \"amt_to_forward\":\"7050\", \"fee\":\"5\", \"expiry\":722956, \"amt_to_forward_msat\":\"7050000\", \"fee_msat\":\"5279\", \"pub_key\":\"03bb88ccc444534da7b5b64b4f7b15e1eccb18e102db0e400d4b9cfe93763aa26d\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"761938468346462208\", \"chan_capacity\":\"5000000\", \"amt_to_forward\":\"7050\", \"fee\":\"0\", \"expiry\":722956, \"amt_to_forward_msat\":\"7050000\", \"fee_msat\":\"0\", \"pub_key\":\"02f44b34a2afb094202a8e184c04c1cf75d8809d77a33ffed68eb3645527057d22\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"Ac1Moa/YiFJtortDL3IAF3wIJQ0YXblfpcaHfiIh/Rc=\", \"total_amt_msat\":\"7050000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"5279\", \"total_amt_msat\":\"7055279\"}, \"attempt_time_ns\":\"1644659848080300512\", \"resolve_time_ns\":\"1644659849356625980\", \"failure\":null, \"preimage\":\"1VBiG5K++6B3WFTKjQ5CphuglY9nFjavH117HkUckpE=\"}], \"payment_index\":\"65184\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"e8dfb899796c76a6a58b13721076dd83c5fe7908def094cf3db00967de6f521a\", \"value\":\"1000\", \"creation_date\":\"1644660601\", \"fee\":\"0\", \"payment_preimage\":\"b7e6df51b1abe5a3e20f9abc5a3689bc4dc89d42e545b562825969a0fa69764e\", \"value_sat\":\"1000\", \"value_msat\":\"1000000\", \"payment_request\":\"lnbc10u1p3q0pmspp5ar0m3xted3m2dfvtzdepqakas0zlu7ggmmcffneakqyk0hn02gdqdz9f35kw6r5de5kue6wv468wmmjddfhgmmjv4ejucm0d5sxvct4vdjhggrydahxzarfdahzucqzpgxqzjhsp5xs5e7225t993m3v2zxyuqyj2r7ulj44u2le6532rz5j8ttlgzs9q9qyyssq3afw0jxu2xkxd6823munnc2slu92x8fdx20zde6nm8t5zvq2wfjksu5en6hqzpjgcwymqg0vrr34mz6ne7gj0t9f72wp5ekzmkgcuuqqnk9ff9\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"0\", \"fee_msat\":\"0\", \"creation_time_ns\":\"1644660601785973101\", \"htlcs\":[{\"attempt_id\":\"130058\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":722959, \"total_fees\":\"0\", \"total_amt\":\"1000\", \"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"1000\", \"fee\":\"0\", \"expiry\":722959, \"amt_to_forward_msat\":\"1000000\", \"fee_msat\":\"0\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"NCmfKVRZSx3FihGJwBJKH7n5VrxX86pFQxUkda/oFAo=\", \"total_amt_msat\":\"1000000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"0\", \"total_amt_msat\":\"1000000\"}, \"attempt_time_ns\":\"1644660617499528890\", \"resolve_time_ns\":\"1644660617870099819\", \"failure\":null, \"preimage\":\"t+bfUbGr5aPiD5q8WjaJvE3InULlRbVigllpoPppdk4=\"}], \"payment_index\":\"65185\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"5a1f09b0bc98624fbb72c27af13e364d726e52ac8415bdf5f94a4d9e427cb1d3\", \"value\":\"1000\", \"creation_date\":\"1644661326\", \"fee\":\"0\", \"payment_preimage\":\"afaa579b56d7aab1745eb29b43ee2d1a1b94edd1e3fec4ab38086224e3210cbc\", \"value_sat\":\"1000\", \"value_msat\":\"1000000\", \"payment_request\":\"lnbc10u1p3q0zjypp5tg0snv9unp3ylwmjcfa0z03kf4exu54vss2mma0effxeusnuk8fsdz2f35kw6r5de5kue6wv468wmmjddfhgmmjv4ejucm0d5s82urkda6x2uevypehgmmjv57nzvp3xucqzpgxqzjhsp5apeyvhyztvpv05ef4m7gwx0xlk3ufzvk8y3dql9za6a58q6d5hzq9qyyssqe4cf4p54wep24k5uwzhnun25k873h7h68822qxzq4yjje62k0arnz6hc6a2p6kt6zsu77kxskg7ymtcvp330nv5l6dsxc77ddd2uwucq0xjrtg\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"0\", \"fee_msat\":\"0\", \"creation_time_ns\":\"1644661326504815591\", \"htlcs\":[{\"attempt_id\":\"130059\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":722959, \"total_fees\":\"0\", \"total_amt\":\"1000\", \"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"1000\", \"fee\":\"0\", \"expiry\":722959, \"amt_to_forward_msat\":\"1000000\", \"fee_msat\":\"0\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"6HJGXIJbAsfTKa78hxnm/aPEiZY5ItB8ou67Q4NNpcQ=\", \"total_amt_msat\":\"1000000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"0\", \"total_amt_msat\":\"1000000\"}, \"attempt_time_ns\":\"1644661326538459961\", \"resolve_time_ns\":\"1644661326891622343\", \"failure\":null, \"preimage\":\"r6pXm1bXqrF0XrKbQ+4tGhuU7dHj/sSrOAhiJOMhDLw=\"}], \"payment_index\":\"65186\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"2ebd0b606d106b96259e9b2fb17cd2e8e9bf2f4517be094bc215bc62c71bb4d2\", \"value\":\"150\", \"creation_date\":\"1644688707\", \"fee\":\"1\", \"payment_preimage\":\"6b5607cebf017524d09d42fb349dfe849f1f68f9b72b07ec292b4ae639256eec\", \"value_sat\":\"150\", \"value_msat\":\"150000\", \"payment_request\":\"lnbc1500n1p3q0afapp5967skcrdzp4evfv7nvhmzlxjar5m7t69z7lqjj7zzk7x93cmknfqdqu2askcmr9wssx7e3q2dshgmmndp5scqzpgxqyz5vqsp5sg990j7lfjz0tajeyg8e6tw78phutrmdh5qm90q3jptz5erypejq9qyyssqjf3hs8s7sqaamgel865aqjs78p29qsk5t0fk3tu5k3te5zav0j990gjk96acyrzkasffaqlqlt5eqspuwnhyw63jlpwv49sd50f2ztcphah30f\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"1\", \"fee_msat\":\"1150\", \"creation_time_ns\":\"1644688707219024167\", \"htlcs\":[{\"attempt_id\":\"130060\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":723053, \"total_fees\":\"1\", \"total_amt\":\"151\", \"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"150\", \"fee\":\"1\", \"expiry\":723013, \"amt_to_forward_msat\":\"150000\", \"fee_msat\":\"1150\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"787317395838795776\", \"chan_capacity\":\"50000000\", \"amt_to_forward\":\"150\", \"fee\":\"0\", \"expiry\":723013, \"amt_to_forward_msat\":\"150000\", \"fee_msat\":\"0\", \"pub_key\":\"035e4ff418fc8b5554c5d9eea66396c227bd429a3251c8cbc711002ba215bfc226\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"ggpXy99MhPX2WSIPnS3eOG/Fj229AbK8EZBWKmRkDmQ=\", \"total_amt_msat\":\"150000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"1150\", \"total_amt_msat\":\"151150\"}, \"attempt_time_ns\":\"1644688707440008666\", \"resolve_time_ns\":\"1644688707978164100\", \"failure\":null, \"preimage\":\"a1YHzr8BdSTQnUL7NJ3+hJ8faPm3KwfsKStK5jklbuw=\"}], \"payment_index\":\"65187\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"9e7769bcac36953f6c161bf426a8652d664f0c13bf3b02a51ddcc55bb033c12b\", \"value\":\"300\", \"creation_date\":\"1644694112\", \"fee\":\"1\", \"payment_preimage\":\"1917f7be8e7be4ab36f3e74a0fcecb4a0da5dcb63e2672497c1cf415b1a9e235\", \"value_sat\":\"300\", \"value_msat\":\"300000\", \"payment_request\":\"lnbc3u1p3qszjvpp5nemkn09vx62n7mqkr06zd2r994ny7rqnhuas9fgamnz4hvpncy4sdqqcqzpgxqyz5vqsp5t8y8j0rwe7k6cyfhr0kuesm90u7nhgej6xk4lhfzkzx69t4gynyq9qyyssqhtjw2ywn9h8fcdxum6qfjvleya2e4gh9dy342sujf8gpfnqrwjtxsx64z23w4ym4dzvj0f4a03uypjze0ruy572zewa7dqxlqsapvkcpc6gruk\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"1\", \"fee_msat\":\"1510\", \"creation_time_ns\":\"1644694112950182910\", \"htlcs\":[{\"attempt_id\":\"130061\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":723063, \"total_fees\":\"1\", \"total_amt\":\"301\", \"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"300\", \"fee\":\"1\", \"expiry\":723023, \"amt_to_forward_msat\":\"300000\", \"fee_msat\":\"1510\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"767893423426633728\", \"chan_capacity\":\"100000000\", \"amt_to_forward\":\"300\", \"fee\":\"0\", \"expiry\":723023, \"amt_to_forward_msat\":\"300000\", \"fee_msat\":\"0\", \"pub_key\":\"037cc5f9f1da20ac0d60e83989729a204a33cc2d8e80438969fadf35c1c5f1233b\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"Wch5PG7PrawRNxvtzMNlfz07ozLRrV/dIrCNoq6oJMg=\", \"total_amt_msat\":\"300000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"1510\", \"total_amt_msat\":\"301510\"}, \"attempt_time_ns\":\"1644694113072012550\", \"resolve_time_ns\":\"1644694118906223610\", \"failure\":null, \"preimage\":\"GRf3vo575Ks28+dKD87LSg2l3LY+JnJJfBz0FbGp4jU=\"}], \"payment_index\":\"65188\", \"failure_reason\":\"FAILURE_REASON_NONE\"}, {\"payment_hash\":\"b37a542d1c68b94e843f84b53076927c7d9c45753db4a83b62058703a8f3a840\", \"value\":\"48\", \"creation_date\":\"1644696231\", \"fee\":\"1\", \"payment_preimage\":\"b0d7dc7e8534952daa6a9b0f7422b1cee7850dd94e1e2a7785a78790334a5517\", \"value_sat\":\"48\", \"value_msat\":\"48000\", \"payment_request\":\"lnbc480n1p3qsy5kpp5kda9gtgudzu5applsj6nqa5j037ec3t48k62swmzqkrs828n4pqqdqqcqzpgxqyz5vqsp57ut3f2feg58ykuxc8xk3c0wn7qppm8lhw88eldpk9sku7y0c3w5s9qyyssqldk96evua697zm6ke5f0sryw7s8x5tv8qmd53nr8a3rwuy7e42tryq0pzzj5r3dzakd0tzfjs2h392d2sdsrta9a6pq8lgpelhwn8lgqc4prfk\", \"status\":\"SUCCEEDED\", \"fee_sat\":\"1\", \"fee_msat\":\"1081\", \"creation_time_ns\":\"1644696231872274579\", \"htlcs\":[{\"attempt_id\":\"130062\", \"status\":\"SUCCEEDED\", \"route\":{\"total_time_lock\":723066, \"total_fees\":\"1\", \"total_amt\":\"49\", \"hops\":[{\"chan_id\":\"769355773858938881\", \"chan_capacity\":\"16777215\", \"amt_to_forward\":\"48\", \"fee\":\"1\", \"expiry\":723026, \"amt_to_forward_msat\":\"48000\", \"fee_msat\":\"1081\", \"pub_key\":\"03abf6f44c355dec0d5aa155bdbdd6e0c8fefe318eff402de65c6eb2e1be55dc3e\", \"tlv_payload\":true, \"mpp_record\":null, \"amp_record\":null, \"custom_records\":{}}, {\"chan_id\":\"762456338409586689\", \"chan_capacity\":\"100000000\", \"amt_to_forward\":\"48\", \"fee\":\"0\", \"expiry\":723026, \"amt_to_forward_msat\":\"48000\", \"fee_msat\":\"0\", \"pub_key\":\"037cc5f9f1da20ac0d60e83989729a204a33cc2d8e80438969fadf35c1c5f1233b\", \"tlv_payload\":true, \"mpp_record\":{\"payment_addr\":\"9xcUqTlFDktw2DmtHD3T8AIdn/dxz5+0NiwtzxH4i6k=\", \"total_amt_msat\":\"48000\"}, \"amp_record\":null, \"custom_records\":{}}], \"total_fees_msat\":\"1081\", \"total_amt_msat\":\"49081\"}, \"attempt_time_ns\":\"1644696231953404330\", \"resolve_time_ns\":\"1644696235855846626\", \"failure\":null, \"preimage\":\"sNfcfoU0lS2qapsPdCKxzueFDdlOHip3haeHkDNKVRc=\"}], \"payment_index\":\"65189\", \"failure_reason\":\"FAILURE_REASON_NONE\"}],
        //\"first_index_offset\":\"65180\", \"last_index_offset\":\"65189\"}"

        [TestMethod]
        public void TestAPIGetInvoicesAsString()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/invoices";

            var parameters = new Dictionary<string, string>()
            {
                { "pending_only", "true" },
                { "num_max_invoices", "2" }
            };

            string responseStr = LndRpcClient.LndApiGetStr(
                host: host, 
                restpath: restpath, 
                urlParameters: parameters,
                adminMacaroon: ConfigurationManager.AppSettings["LnMainnetMacaroonRead"]);
            Console.WriteLine(responseStr);
        }

        [TestMethod]
        public void TestAPISubscribeInvoice()
        {
            string host = ConfigurationManager.AppSettings["LnMainnetHost"];
            string restpath = "/v1/invoices/subscribe";
            LndApiGetStrAsync(host, restpath, adminMacaroon: MacaroonAdmin[false]);

            for(int i = 0; i < 5; i++)
            {
                Console.WriteLine(i.ToString());
                Thread.Sleep(3000);
                if (i < 2)
                {
                    restpath = "/v1/invoices";
                    var invoice = new Invoice()
                    {
                        value = "1000",
                        memo = "Testing",
                    };

                    //admin
                    string responseStr = LndRpcClient.LndApiPostStr(host, restpath, invoice, adminMacaroon: MacaroonAdmin[false]);
                    Console.WriteLine("added: " + responseStr);
                }
            }
            Console.WriteLine("Done");
        }

        public static void LndApiGetStrAsync(string host, string restpath, int port = 8080, string adminMacaroon = "")
        {
            //var m = System.IO.File.ReadAllBytes("readonly.macaroon");
            //HexEncoder h = new HexEncoder();
            string macaroon = "";
            if (adminMacaroon != "")
            {
                macaroon = adminMacaroon;
            }
            else
            {
                throw new Exception("No admin macaroon provided.");
            }

            //public cert - no sensitivity
            //string tlscert = "2d2d2d2d2d424547494e2043455254494649434154452d2d2d2d2d0a4d494943417a434341617167417749424167494a4150486f4e765a39665942304d416f4743437147534d343942414d434d4430784c54417242674e5642414d4d0a4a474e766157357759573570597a45755a57467a6448567a4c6d4e736233566b595842774c6d463664584a6c4c6d4e766254454d4d416f4741315545436777440a6247356b4d434158445445344d444d794e4445354d5459774e6c6f59447a49784d5467774d6a49344d546b784e6a4132576a41394d5330774b775944565151440a4443526a62326c756347467561574d784c6d5668633352316379356a624739315a47467763433568656e56795a53356a623230784444414b42674e5642416f4d0a413278755a44425a4d424d4742797147534d34394167454743437147534d3439417745484130494142475169586c5970527766436648736e65694352627774430a4e774738656562437646786b344c6e4461584732684472305137394c465044376d34354271756f684937653531496f385073454c51644d4a2f2f686d6756616a0a675a41776759307744675944565230504151482f4241514441674b6b4d41384741315564457745422f7751464d414d4241663877616759445652305242474d770a5959496b59323970626e4268626d6c6a4d53356c59584e3064584d7559327876645752686348417559587031636d55755932397467676c7362324e68624768760a63335348424838414141474845414141414141414141414141414141414141414141474842416f4141415348455036414141414141414141416730362f2f34580a76344d77436759494b6f5a497a6a30454177494452774177524149674242735234374b592b6b777761456551565245634237703078472f41522b34446e7478770a6d794a633476454349446561797a6962437156357a795850392f624849485074637a51336c4148704b33546c62707932636c61410a2d2d2d2d2d454e442043455254494649434154452d2d2d2d2d0a";
            // h.EncodeData(m);
            //string TLSFilename = "tls.cert";
            //X509Certificate2 certificates = new X509Certificate2();

            //var tlsc = h.DecodeData(tlscert);
            //certificates.Import(tlsc);

            ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

            var client = new RestClient("https://" + host + ":" + Convert.ToString(port));
            //client.ClientCertificates = new X509CertificateCollection() { certificates };

            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadWrite);
            //store.Add(certificates);

            //client.RemoteCertificateValidationCallback =
            //    delegate (object s, X509Certificate certificate,
            //              X509Chain chain, SslPolicyErrors sslPolicyErrors)
            //    {
            //    TODO: fix later
            //        return true;
            //    };

            var request = new RestRequest(restpath, Method.Get);
            request.AddHeader("Grpc-Metadata-macaroon", macaroon);

            //Execute Async
            //client.ExecuteAsync(request, HandleResponse);

            var response = client.ExecuteAsync(request).Result;

            //var response = client.Execute(request);
            string responseStr = response.Content;
            //return responseStr;
        }

        //private static void HandleResponse(IRestResponse response, RestRequestAsyncHandle h)
        //{
        //    Console.WriteLine(response.Content);
        //}

        private static string LndApiGet(string host, string restpath, int port = 8080, Dictionary<string, string> urlParameters = null, string macaroonAdmin = "", string macaroonRead = "")
        {
            string macaroon = "";
            if (macaroonRead != "")
            {
                macaroon = macaroonRead;
            }
            else if (macaroonAdmin != "")
            {
                macaroon = macaroonAdmin;
            }

            //X509Certificate2 certificates = new X509Certificate2();

            //var tls = System.IO.File.ReadAllBytes(TLSFilename);
            //var tlsh = h.EncodeData(tls);
            //var tlsc = h.DecodeData(tlsh);

            //certificates.Import(tlsc);

            var client = new RestClient("https://" + host + ":" + Convert.ToString(port));
            //client.ClientCertificates = new X509CertificateCollection() { certificates };

            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadWrite);
            //store.Add(certificates);

            //client.RemoteCertificateValidationCallback =
            //    delegate (object s, X509Certificate certificate,
            //              X509Chain chain, SslPolicyErrors sslPolicyErrors)
            //    {
            //        //TODO: fix later
            //        return true;
            //    };

            var request = new RestRequest(restpath, Method.Get);

            if (urlParameters != null)
            {
                foreach (var p in urlParameters)
                {
                    request.AddUrlSegment(p.Key, p.Value);
                }
            }

            request.AddHeader("Grpc-Metadata-macaroon", macaroon);

            var response = client.ExecuteAsync(request).Result;
            string responseStr = response.Content;
            return responseStr;
        }

        private static string LndApiPost(string host, string restpath, object body, int port = 8080)
        {
            string TLSFilename = "tls.cert";
            var m = System.IO.File.ReadAllBytes("readonly.macaroon");
            //HexEncoder h = new HexEncoder();
            //string macaroon = h.EncodeData(m);

            X509Certificate2 certificates = new X509Certificate2();

            var tls = System.IO.File.ReadAllBytes(TLSFilename);
            //var tlsh = h.EncodeData(tls);

            //var tlsc = h.DecodeData(tlsh);

            //certificates.Import(tlsc);

            var client = new RestClient("https://" + host + ":" + Convert.ToString(port));
            //client.ClientCertificates = new X509CertificateCollection() { certificates };

            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadWrite);
            //store.Add(certificates);

            //client.RemoteCertificateValidationCallback =
            //    delegate (object s, X509Certificate certificate,
            //              X509Chain chain, SslPolicyErrors sslPolicyErrors)
            //    {
            //        //TODO: fix later
            //        return true;
            //    };

            var request = new RestRequest(restpath, Method.Post);
            //request.AddHeader("Grpc-Metadata-macaroon", macaroon);
            request.RequestFormat = DataFormat.Json;

            request.AddBody(body);

            var response = client.ExecuteAsync(request).Result;
            string responseStr = response.Content;
            return responseStr;
        }
    }
}