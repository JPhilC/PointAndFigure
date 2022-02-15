using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public class IndexShare:EntityData
    {
        public Guid IndexId { get; set; }
        
        public Guid ShareId { get; set; }

        public double Factor { get; set; } = 1d;

        [ForeignKey("IndexId")]
        public Index Index { get; set; }

        [ForeignKey("ShareId")]
        public Share Share { get; set; }


    }
}
