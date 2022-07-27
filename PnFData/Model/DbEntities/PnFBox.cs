using System.ComponentModel.DataAnnotations;
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
        [Required]
        public Guid PnFColumnId { get; set; }

        [ForeignKey("PnFColumnId")]
        public PnFColumn Column { get; set; }

        public PnFBoxType BoxType { get; set; }

        /// <summary>
        /// The vertical position of this box in the column
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The box size.
        /// </summary>
        public double Size { get; set; }

        public double Value { get; set; }

        public DateTime Ticked { get; set; }

        public string? MonthIndicator { get; set; }

    }
}
