using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public static class PublicIP
    {
        static IPAddress CurrentAddress;

        public static byte[] Get()
        {
            return CurrentAddress.GetAddressBytes();
        }
        public static string GetString()
        {
            return CurrentAddress.ToString();
        }

        public static void Update()
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            var response = client
                .GetStringAsync("https://checkip.amazonaws.com")
                .GetAwaiter()
                .GetResult()
                .Trim();

            CurrentAddress = IPAddress.Parse(response);
        }
    }
}
