using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BybitAloConsole
{
    public class BybitApi
    {
//#if DEBUG
//        const string BaseUri = "https://api-testnet.bybit.com";
//#else
//#endif
        const string BaseUri = "https://api.bybit.com";

        string apiKey;
        string apiSecret;

        public BybitApi(string _apiKey, string _apiSecret)
        {
            this.apiKey = _apiKey;
            this.apiSecret = _apiSecret;
        }

        public async Task GetServerTime()
        {
            var param = "/v2/public/time";
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}{param}");
            using var client = new HttpClient();
            await request.Authenticate(this.apiKey, this.apiSecret);

            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            var test = await httpResponseMessage.Content.ReadAsStringAsync();
            JObject responseJson = JObject.Parse(test);

            Console.WriteLine("Server time is: " + responseJson.ToString());
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            var param = "/open-api/order/list";
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}{param}");
            using var client = new HttpClient();
            await request.Authenticate(this.apiKey, this.apiSecret);

            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            var test = await httpResponseMessage.Content.ReadAsStringAsync();
            JObject responseJson = JObject.Parse(test);

            Console.WriteLine(responseJson.ToString());
            return responseJson["result"]["data"].Select(x => new Order
            {
                OrderId = Guid.Parse(x["order_id"].ToString()),
                Price = double.Parse(x["price"].ToString()),
                Qty = int.Parse(x["qty"].ToString()),
                OrderStatus = x["order_status"].ToString(),
                Side = x["side"].ToString() == "Buy" ? Side.Buy : Side.Sell,
                Symbol = x["symbol"].ToString(),
                OrderType = x["order_type"].ToString()
            });
        }

        public async Task PlaceOrder(Order order)
        {
            var param = "/v2/private/order/create";
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}{param}");
            using var client = new HttpClient();

            dynamic jsonContent = new ExpandoObject();
            jsonContent.side = order.Side == Side.Sell ? "Sell" : "Buy";
            jsonContent.symbol = order.Symbol;
            jsonContent.order_type = order.OrderType;
            jsonContent.price = order.Price;
            jsonContent.qty = order.Qty;

            if (order.PostOnly)
                jsonContent.time_in_force = "PostOnly";
            else
                jsonContent.time_in_force = "GoodTillCancel";

            string json = JsonConvert.SerializeObject(jsonContent);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            await request.Authenticate(this.apiKey, this.apiSecret);

            Console.WriteLine("Request: " + request.ToString());
            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            var responseJson = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
            if (responseJson["ret_code"].ToString().Equals("0"))
            {
                order.OrderId = Guid.Parse(responseJson["result"]["order_id"].ToString());
                order.OrderStatus = "Created";
            }
            Console.WriteLine("Response: " + responseJson.ToString());
        }

        public async Task PlaceStopOrder(Order order, double currentPrice, double stopPx)
        {
            var param = "/open-api/stop-order/create";
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}{param}");
            using var client = new HttpClient();

            var jsonContent = new
            {
                side = order.Side == Side.Sell ? "Sell" : "Buy",
                symbol = order.Symbol,
                order_type = order.OrderType,
                base_price = currentPrice,
                stop_px = stopPx,
                qty = order.Qty,
                time_in_force = "PostOnly"
            };
            string json = JsonConvert.SerializeObject(jsonContent);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            await request.Authenticate(this.apiKey, this.apiSecret);

            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            var responseJson = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
            if (responseJson["ret_code"].ToString().Equals("0"))
            {
                order.OrderId = Guid.Parse(responseJson["result"]["order_id"].ToString());
                order.OrderStatus = "Created";
            }
            Console.WriteLine(responseJson.ToString());
        }

        public async Task<IDictionary<Side, double>> GetCurrentPrice(string symbolName)
        {
            var param = "/v2/public/tickers";
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUri}{param}?symbol={symbolName}");
            using var client = new HttpClient();

            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            JObject responseJson = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());

            Console.WriteLine(responseJson.ToString());
            return new Dictionary<Side, double>
            {
                { Side.Sell, double.Parse(responseJson["result"][0]["ask_price"].ToString()) },
                { Side.Buy, double.Parse(responseJson["result"][0]["bid_price"].ToString()) },
            };
        }

        public async Task AmendOrder(Order order)
        {
            var param = "/open-api/order/replace";
            using var request =
                new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}{param}");
            using var client = new HttpClient();

            var jsonContent = new
            {
                order_id = order.OrderId,
                symbol = order.Symbol,
                //                p_r_qty = 1,
                p_r_price = order.Price.ToString("F1")
            };
            string json = JsonConvert.SerializeObject(jsonContent);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            await request.Authenticate(this.apiKey, this.apiSecret);

            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            var responseJson = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
            Console.WriteLine(responseJson.ToString());
        }

        public async Task AmendOrderTwo(string orderId, double price)
        {
            var param = "/open-api/order/replace";
            using var request =
                new HttpRequestMessage(HttpMethod.Post, $"{BaseUri}{param}");
            using var client = new HttpClient();

            var jsonContent = new
            {
                order_id = orderId,
                symbol = "BTCUSD",
                //                p_r_qty = 1,
                p_r_price = price.ToString("F1")
            };
            string json = JsonConvert.SerializeObject(jsonContent);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            await request.Authenticate(this.apiKey, this.apiSecret);

            HttpResponseMessage httpResponseMessage = await client.SendAsync(request);
            var responseJson = JObject.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
            Console.WriteLine(responseJson.ToString());
        }
    }

    public static class ObjectExtensions
    {
        public static IDictionary<string, object> AddProperty(this object obj, string name, object value)
        {
            var dictionary = obj.ToDictionary();
            dictionary.Add(name, value);
            return dictionary;
        }

        // helper
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in properties)
            {
                result.Add(property.Name, property.GetValue(obj));
            }
            return result;
        }
    }
}