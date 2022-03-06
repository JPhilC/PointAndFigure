using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public class IndexRSI : EntityData
    {

        public DateTime Day { get; set; }

        public double Value { get; set; }

        [Required]
        public Guid IndexId { get; set; }

        [ForeignKey(nameof(IndexId))]
        public Index Index { get; set; }
    }
}
