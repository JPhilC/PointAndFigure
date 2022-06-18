using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PnFData.Model
{
    public class PortfolioShare:EntityData
    {
        [Required]
        public Guid PortfolioId { get;set;}

        [ForeignKey("ProfolioId")]
        public Portfolio Portfolio { get; set; }

        [Required]
        public Guid ShareId { get;set;}

        [ForeignKey("ShareId")]
        public Share Share { get;set;}

        [DefaultValue(0d)]
        public double Holding { get; set;}
    }
}
