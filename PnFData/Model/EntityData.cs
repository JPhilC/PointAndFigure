using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PnFData.Model
{
    public abstract class EntityData: ITableData
    {
        protected EntityData()
        {
        }

        [Key]
        public Guid Id { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset? CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? UpdatedAt { get; set; }


    }
}
