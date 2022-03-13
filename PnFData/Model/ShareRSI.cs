using PnFData.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public enum RelativeToEnum
    {
        Market,     // RSI relative to Market
        Sector      // RSI relative to Sector
    }
    public class ShareRSI : EntityData, IDayValue
    {

        public RelativeToEnum RelativeTo { get; set; }

        public DateTime Day { get; set; }

        public double Value { get; set; }

        [Required]
        public Guid ShareId { get; set; }

        [ForeignKey(nameof(ShareId))]
        public Share Share { get; set; }
    }
}
