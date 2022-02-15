using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PnFData.Model
{
    [Index("Day")]
    public class IndexDate : EntityData
    {

        public DateTime Day { get; set; }

        public List<IndexShare> Shares { get; } = new List<IndexShare>();

        public Guid IndexId { get; set; }

        [ForeignKey("IndexId")]
        public Index Index { get; set; }
    }
}
