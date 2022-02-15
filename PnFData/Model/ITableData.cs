using System.Diagnostics.CodeAnalysis;

namespace PnFData.Model
{
    public interface ITableData
    {
        /// <summary>
        /// Gets or sets the unique ID for this entity.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique version identifier which is updated every time the entity is updated.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is part of the data model.")]
        byte[] Version { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity was created.
        /// </summary>
        DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity was last modified.
        /// </summary>
        DateTimeOffset? UpdatedAt { get; set; }
    }
}
