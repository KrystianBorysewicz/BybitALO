using System;
using System.Collections.Generic;
using System.Text;

namespace BybitAloConsole
{
    public class NewMainViewModel : IDisposable
    {
        readonly IWebsocket websocket;
        string apiSecret;
        string apiKey;

        public NewMainViewModel(IWebsocket websocket)
        {
            this.websocket = websocket;

            this.websocket.Connected += this.Connected;
            this.websocket.MessageReceived += this.MessageReceived;
        }

        void Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Websocket connection open");
            this.websocket.Send(WsConnectionCommands.GetTradesCommand());
            this.websocket.Send(WsConnectionCommands.GetAuthenticationCommand(this.apiKey, this.apiSecret));
            this.websocket.Send(WsConnectionCommands.GetOrdersCommand());
        }

        void MessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Console.WriteLine($"Received: {messageEventArgs.Data}");
            //this.messageHandler.Handle(messageEventArgs);
        }

        public void Connect(string apiKey, string apiSecret)
        {
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
            this.websocket.Close();
            this.websocket.Connect();
        }

        public void Dispose()
        {
            this.websocket.Connected -= this.Connected;
            this.websocket.MessageReceived -= this.MessageReceived;
            this.websocket.Dispose();
        }
    }
}
