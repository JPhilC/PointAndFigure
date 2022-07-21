using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PnFData.Model
{
    public abstract class EntityData: ObservableObject, ITableData
    {
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
