using JsonSerializer = Utf8Json.JsonSerializer;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using WebSocket4Net;

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
                var result = JsonSerializer.Deserialize<dynamic>(e.Message);
                var last = result["data"][result["data"].Count - 1];
                if (result["topic"] == "trade.BTCUSD")
                {
                    if(activeOrder != "")
                    {
                        if(last["side"] == "Buy")
                        {
                            if(last["price"] > buyPrice)
                            {
                                activeApi.AmendOrderTwo(activeOrder, activeOrderSide == "Buy" ? last["price"] - 0.5 : last["price"]);
                                buyPrice = last["price"];
                                sellPrice = last["price"] - 0.5;
                            }
                        }
                        else if (last["side"] == "Sell")
                        {
                            if (last["price"] < sellPrice)
                            {
                                activeApi.AmendOrderTwo(activeOrder, activeOrderSide == "Buy" ? last["price"] : last["price"] + 0.5);
                                buyPrice = last["price"] + 0.5;
                                sellPrice = last["price"];
                            }
                        }
                    }
                    if (activeOrder != "" && activeOrderSide == "Buy" && last["price"] > buyPrice && last["side"] == "Buy")
                    {
                        activeApi.AmendOrderTwo(activeOrder, last["price"]);
                        Console.WriteLine("AmendOrder");
                    }
                    else if (activeOrder != "" && activeOrderSide == "Sell" && last["price"] < sellPrice && last["side"] == "Sell")
                    {
                        activeApi.AmendOrderTwo(activeOrder, last["price"]);
                        Console.WriteLine("AmendOrder");
                    }
                    if (last["side"] == "Buy")
                        buyPrice = last["price"];
                    else
                        sellPrice = last.price;
                }
                if (result["topic"] == "order")
                {
                    
                    if ((last["order_status"] == "Filled" || last["order_status"] == "Cancelled") && (last["order_id"] == activeOrder))
                    {
                        activeOrder = "";
                        buyPrice = 0;
                        sellPrice = 99999;
                    }
                    else if (last["order_status"] == "New" && last["qty"] % 10 == 9)
                    {
                        activeOrder = last["order_id"];
                        activeOrderSide = last["side"];

                    }
                }
                Console.WriteLine(e.Message);
            } catch { }
            
        }

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