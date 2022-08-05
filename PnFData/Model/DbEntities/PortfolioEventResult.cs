namespace PnFData.Model
{
    public class PortfolioEventResult
    {
        public string Tidm { get; set; }

        public string ShareName { get; set; }

        public string Portfolio { get; set; }

        public int NewEvents { get; set; }

        public bool Rising { get; set; }
        public bool Falling { get; set; }
        public bool Buy { get; set; }
        public bool BuyBuy { get; set; }
        public bool Sell { get; set; }
        public bool SellSell { get; set; }
        public bool SupportBreach { get; set; }
        public bool RsBuy { get; set; }
        public bool RsSell { get; set; }
        public bool PeerRsBuy { get; set; }
        public bool PeerRsSell { get; set; }
        public bool AboveEma10 { get; set; }
        public bool BelowEma10 { get; set; }
        public bool AboveEma30 { get; set; }
        public bool BelowEma30 { get; set; }
        public bool High52W { get; set; }
        public bool Low52W { get; set; }
        public bool MomentumUp { get; set; }
        public bool MomentumDown { get; set; }
    }
}
