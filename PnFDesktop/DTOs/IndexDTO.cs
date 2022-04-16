using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class IndexDTO
    {
        public Guid Id { get; set; }

        public string ExchangeCode { get; set; }

        public string? ExchangeSubCode { get; set; }

        public string? SuperSector { get; set; }

        public string? Description {get; set;}

    }
}
