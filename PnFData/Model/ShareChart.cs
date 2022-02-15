using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public class ShareChart:EntityData
    {
        public Guid ShareId { get; set; }

        [ForeignKey("ShareId")]
        public Share Share { get; set; }

        public Guid PnFChartId { get; set; }

        [ForeignKey("PnFChartId")]
        public PnFChart PnFChart { get; set; }

    }
}
