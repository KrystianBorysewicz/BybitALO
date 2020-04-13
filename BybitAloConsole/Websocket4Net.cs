using JsonSerializer = Utf8Json.JsonSerializer;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace BybitAloConsole
{
    public class Websocket4Net : IWebsocket
    {
        readonly WebSocket ws;
        private BybitApi activeApi;
        double buyPrice = 0;
        double sellPrice = 99999;
        string activeOrder = "";
        string activeOrderSide = "Buy";
        public EventHandler Connected { get; set; }
        public EventHandler<MessageEventArgs> MessageReceived { get; set; }

        public Websocket4Net(string url, string apiKey, string apiSecret)
        {
            this.ws = new WebSocket(url)
            {
                EnableAutoSendPing = true,
                Security = { EnabledSslProtocols = SslProtocols.Tls12 },
                NoDelay = true
            };
            this.activeApi = new BybitApi(apiKey, apiSecret);
            this.ws.Opened += this.WsOnOpened;
            this.ws.MessageReceived += this.WsOnMessageReceived;
            this.ws.Closed += this.WsOnClosed;
        }

        public async Task Connect()
        {
            await this.ws.OpenAsync().ConfigureAwait(false);
        }

        public async Task Close()
        {
            await this.ws.CloseAsync().ConfigureAwait(false);
        }

        public void Send(string message)
        {
            this.ws.Send(message);
        }

        public void Dispose()
        {
            this.ws.Opened -= this.WsOnOpened;
            this.ws.MessageReceived -= this.WsOnMessageReceived;
            this.ws.Closed -= this.WsOnClosed;
            ((IDisposable)this.ws)?.Dispose();
        }

        void WsOnOpened(object sender, EventArgs e)
        {
            this.Connected?.Invoke(sender, e);
        }

        void WsOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var result = JObject.Parse(e.Message);
                var t = result["data"];
                var last = result["data"][result["data"].Count() - 1];
                var price = (double)last["price"];
                var side = (string)last["side"];
                var status = (string)last["order_status"];
                if ((string)result["topic"] == "trade.BTCUSD")
                {
                    if (activeOrder != "")
                    {
                        if (side == "Buy")
                        {
                            if (price > buyPrice)
                            {
                                activeApi.AmendOrderTwo(activeOrder, activeOrderSide == "Buy" ? price - 0.5 : price);
                                buyPrice = price;
                                sellPrice = price - 0.5;
                            }
                        }
                        else if (side == "Sell")
                        {
                            if (price < sellPrice)
                            {
                                activeApi.AmendOrderTwo(activeOrder, activeOrderSide == "Buy" ? price : price + 0.5);
                                buyPrice = price + 0.5;
                                sellPrice = price;
                            }
                        }
                    }
                    if (activeOrder != "" && activeOrderSide == "Buy" && price > buyPrice && side == "Buy")
                    {
                        activeApi.AmendOrderTwo(activeOrder, price);
                        //Console.WriteLine("AmendOrder");
                    }
                    else if (activeOrder != "" && activeOrderSide == "Sell" && price < sellPrice && side == "Sell")
                    {
                        activeApi.AmendOrderTwo(activeOrder, price);
                        //Console.WriteLine("AmendOrder");
                    }
                }
                if ((string)result["topic"] == "order")
                {

                    if ((status == "Filled" || status == "Cancelled") && ((string)last["order_id"] == activeOrder))
                    {
                        activeOrder = "";
                        buyPrice = 0;
                        sellPrice = 99999;
                    }
                    else if (status == "New" && (int)last["qty"] % 10 == 9)
                    {
                        activeOrder = (string)last["order_id"];
                        activeOrderSide = side;

                    }
                }
                //Console.WriteLine(e.Message);
            }
            catch { }

        }

        //async void WsOnMessageReceived(object sender, MessageReceivedEventArgs e)
        //{
        //    try
        //    {
        //        var result = JObject.Parse(e.Message);
        //        var t = result["data"];
        //        var last = result["data"][result["data"].Count() - 1];
        //        var price = (double)last["price"];
        //        var side = (string)last["side"];
        //        var status = (string)last["order_status"];
        //        if ((string)result["topic"] == "trade.BTCUSD")
        //        {
        //            if (activeOrder != "")
        //            {
        //                if (side == "Buy")
        //                {
        //                    if (price > buyPrice)
        //                    {
        //                        await activeApi.PlaceNewOrder(activeOrderSide, "BTCUSD", "Limit", activeOrderSide == "Buy" ? price - 0.5 : price, 10, true);
        //                        //activeApi.AmendOrderTwo(activeOrder, activeOrderSide == "Buy" ? price - 0.5 : price);
        //                        buyPrice = price;
        //                        sellPrice = price - 0.5;
        //                    }
        //                }
        //                else if (side == "Sell")
        //                {
        //                    if (price < sellPrice)
        //                    {
        //                        await activeApi.PlaceNewOrder(activeOrderSide, "BTCUSD", "Limit", activeOrderSide == "Buy" ? price : price + 0.5, 10, true);
        //                        //activeApi.AmendOrderTwo(activeOrder, activeOrderSide == "Buy" ? price : price + 0.5);
        //                        buyPrice = price + 0.5;
        //                        sellPrice = price;
        //                    }
        //                }
        //            }
        //            if (activeOrder != "" && activeOrderSide == "Buy" && price > buyPrice && side == "Buy")
        //            {
        //                await activeApi.PlaceNewOrder(activeOrderSide, "BTCUSD", "Limit", price, 10, true);
        //                //activeApi.AmendOrderTwo(activeOrder, price);
        //                //Console.WriteLine("AmendOrder");
        //            }
        //            else if (activeOrder != "" && activeOrderSide == "Sell" && price < sellPrice && side == "Sell")
        //            {
        //                await activeApi.PlaceNewOrder(activeOrderSide, "BTCUSD", "Limit", price, 10, true);
        //                //activeApi.AmendOrderTwo(activeOrder, price);
        //                //Console.WriteLine("AmendOrder");
        //            }
        //        }
        //        if ((string)result["topic"] == "order")
        //        {

        //            if (status == "Filled")
        //            {
        //                activeOrder = "";
        //                buyPrice = 0;
        //                sellPrice = 99999;
        //                await activeApi.CancelAll();
        //            }
        //            else if (status == "New" && (int)last["qty"] % 10 == 9)
        //            {
        //                activeOrder = (string)last["order_id"];
        //                activeOrderSide = side;

        //            }
        //        }
        //        //Console.WriteLine(e.Message);
        //    }
        //    catch { }

        //}
        async void WsOnClosed(object sender, EventArgs e)
        {
            await this.Connect();
        }
    }


    public interface IWebsocket : IDisposable
    {
        EventHandler Connected { get; set; }
        EventHandler<MessageEventArgs> MessageReceived { get; set; }
        Task Connect();
        Task Close();
        void Send(string message);
    }

    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string data)
        {
            this.Data = data;
        }

        public string Data { get; }
    }
}