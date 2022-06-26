using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class PortfolioShareDTO
    {
        public Guid Id { get; set; }

        public string Tidm { get; set; }

        public string Name { get; set; }

        public double? Holding { get; set;}

    }
}
