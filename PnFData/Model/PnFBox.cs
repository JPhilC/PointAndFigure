using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{

    public enum PnFBoxType
    {
        O,
        X
    }


    public class PnFBox: EntityData
    {
        public Guid PnFColumnId { get; set; }

        [ForeignKey("PnFColumnId")]
        public PnFColumn PnFColumn { get; set; }

        public PnFBoxType BoxType { get; set; }

        public double Size { get; set; }

        public double Value { get; set; }

        public DateTime Ticked { get; set; }

        public string? MonthIndicator { get; set; }

    }
}
