﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri(@"https://<yourportal>.csod.com/services/api/CsodUser/1");
            var request = WebRequest.Create(uri);

            var sessionToken = "<your Session Token goes here eg: 1e7s0kob976h>";
            var sessionTokenSecret = "<your Session Token Secret goes here eg: 4gSg+W2cftuFOyWbweBPvgTjCmgNPSH/6HCBmeASp/jtIIX8S9mp5bZfOqOCHpnP1IoV7Eet8yA484RzEhfnuA==>";

            request.Headers.Add("x-csod-date", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000"));
            request.Headers.Add("x-csod-session-token", sessionToken);
            request.Method = "GET";

            var stringToSign = ConstructStringToSign(request.Method, request.Headers, uri.AbsolutePath);
            var sig = SignString512(stringToSign, sessionTokenSecret);
            request.Headers.Add("x-csod-signature", sig);

            request.Timeout = 999999;

            using (var response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseFromServer = reader.ReadToEnd();
                    Console.WriteLine(responseFromServer);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Enter To Exit");
            Console.ReadLine();
        }
        public static string ConstructStringToSign(string httpMethod, NameValueCollection headers, string pathAndQuery)
        {
            StringBuilder stringToSign = new StringBuilder();
            var httpVerb = httpMethod.Trim() + "\n";
            var csodHeaders = headers.Cast<string>().Where(w => w.StartsWith("x-csod-"))
                                                    .Where(w => w != "x-csod-signature")
                                                    .Distinct()
                                                    .OrderBy(s => s)
                                                    .Aggregate(string.Empty, (a, l) => a + l.ToLower().Trim() + ":" + headers[l].Trim() + "\n");
            stringToSign.Append(httpVerb);
            stringToSign.Append(csodHeaders);
            stringToSign.Append(pathAndQuery);
            return stringToSign.ToString();
        }

        public static string SignString512(string stringToSign, string secretKey)
        {
            byte[] secretkeyBytes = Convert.FromBase64String(secretKey);
            byte[] inputBytes = Encoding.UTF8.GetBytes(stringToSign);
            using (var hmac = new HMACSHA512(secretkeyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                return System.Convert.ToBase64String(hashValue);
            }
        }
    }
}
