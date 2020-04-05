using System;
using System.Collections.Generic;
using System.Text;

namespace BybitAloConsole
{
    class WsConnectionCommands
    {
        public static string GetAuthenticationCommand(string apiKey, string apiSecret)
        {
            long expires = AuthenticationService.CurrentTimeMillis() + 10000;

            string param = "GET/realtime"; // This shouldn't be here.
            string signature = AuthenticationService.CreateSignature(apiSecret, $"{param}{expires}");

            return
                $"{{\"op\":\"auth\",\"args\":[\"{apiKey}\",\"{expires}\",\"{signature}\"]}}";
        }

        public static string GetTradesCommand()
        {
            return "{\"op\":\"subscribe\",\"args\":[\"trade\"]}";
        }

        public static string GetOrdersCommand()
        {
            return "{\"op\":\"subscribe\",\"args\":[\"order\"]}";
        }
    }
}
