using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public class Portfolio:EntityData
    {
        public string Name { get; set;}

        public List<PortfolioShare> Shares { get; set;} = new List<PortfolioShare>();
    }
}
