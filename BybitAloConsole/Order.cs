using System;

namespace BybitAloConsole
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string Symbol { get; set; }
        public Side Side { get; set; }
        public string OrderType { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public int LeavesQty { get; set; }
        public string OrderStatus { get; set; }
        public bool PostOnly { get; set; }
    }
    public enum Side
    {
        Buy,
        Sell
    }
}