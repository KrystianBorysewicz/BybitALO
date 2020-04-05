using System;
using System.Threading.Tasks;

namespace BybitAloConsole
{
    class Program
    {
        static string ApiKey = "";
        static string ApiSecret = "";
        static BybitApi activeApi = new BybitApi(ApiKey, ApiSecret);
        static IWebsocket ws = new Websocket4Net("wss://stream.bybit.com/realtime", ApiKey, ApiSecret);
        static void Main(string[] args)
        {
            var x = new NewMainViewModel(ws);
            x.Connect(ApiKey, ApiSecret);
            while (true) { };
        }
        //static async Task operate()
        //{
        //    var order = new Order
        //    {
        //        Price = 4800,
        //        Qty = 1,
        //        Symbol = "BTCUSD",
        //        Side = Side.Buy,
        //        OrderType = "Limit",
        //        PostOnly = false
        //    };
        //    await activeApi.PlaceOrder(order);
        //    Console.WriteLine("Posted");
        //}
    }
}
