using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class ShareSummaryDTO
    {
        public Guid Id { get; set; }
        public string? Tidm { get; set; }

        public string? Name { get; set; }

        public double MarketCapMillions { get; set; }

        public double Close {get; set;}

        public double RsValue {get;set;}

        public double PeerRsValue {get;set;}

        public double Ema10 { get; set; }

        public double Ema30 { get; set; }

        public bool ClosedAboveEma10 { get; set; }

        public bool ClosedAboveEma30 { get; set; }

        public bool Rising { get; set; }

        public bool DoubleTop { get; set; }

        public bool TripleTop { get; set; }

        public bool RsRising { get; set; }

        public bool RsBuy { get; set; }

        public bool PeerRsRising { get; set; }

        public bool PeerRsBuy { get; set; }

        public bool Falling { get; set; }

        public bool DoubleBottom { get; set; }

        public bool TripleBottom { get; set; }


        public bool RsFalling { get; set; }

        public bool RsSell { get; set; }

        public bool PeerRsFalling { get; set; }

        public bool PeerRsSell { get; set; }

        public bool AboveBullSupport { get; set; }

        public int NewEvents {get;set;}

        public int Score {get;set;}


    }
}
