using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    public enum PnFColumnType
    {
        O,
        X,
        XO  // Special case of 1 box reversal charts
    }

    public class PnFColumn : EntityData
    {
        [Required]
        public Guid PnFChartId { get; set; }

        [ForeignKey("PnFChartId")]
        public PnFChart PnFChart { get; set; }

        public PnFColumnType ColumnType { get; set; }

        public int CurrentBoxIndex { get; set; } = 0;

        public double Volume { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public bool ContainsNewYear { get; set; }

        public List<PnFBox> Boxes { get; } = new List<PnFBox>();

        public void AddBox(PnFBoxType boxType, double boxSize, int index, double value, DateTime day, string? monthIndicator = null)
        {
            if (Boxes.Count == 0)
            {
                StartAt = day;
            }

            // Subsequent boxes so may need to add more than one to get to the specified index.
            if (ColumnType == PnFColumnType.O)
            {
                // Falling
                while (CurrentBoxIndex > index)
                {
                    CurrentBoxIndex--;
                    AddBoxInternal(boxType, boxSize, CurrentBoxIndex, value, day, monthIndicator);
                }
            }
            else
            {
                // Rising
                while (CurrentBoxIndex < index)
                {
                    CurrentBoxIndex++;
                    AddBoxInternal(boxType, boxSize, CurrentBoxIndex, value, day, monthIndicator);
                }
            }

            EndAt = day;

        }

        private void AddBoxInternal(PnFBoxType boxType, double boxSize, int index, double value, DateTime day, string? monthIndicator = null)
        {
            PnFBox currentBox = new PnFBox()
            {
                BoxType = boxType,
                Size = boxSize,
                Index = index,
                Value = value,
                Ticked = day
            };
            Boxes.Add(currentBox);

            if (monthIndicator != null)
            {
                currentBox.MonthIndicator = monthIndicator;
            }

        }

    }
}
