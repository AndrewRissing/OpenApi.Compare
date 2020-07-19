using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using OpenApi.Compare.Models;

namespace OpenApi.Compare
{
    /// <summary>
    /// The <see cref="Change"/> class captures a specific change that occurs.
    /// </summary>
    public class Change
    {
        /// <summary>
        /// The <see cref="Compatibility"/> of the <see cref="Change"/>.
        /// </summary>
        public Compatibility Compatibility { get; set; }

        /// <summary>
        /// The <see cref="ChangeType"/> determined by the comparison.
        /// </summary>
        public ChangeType ChangeType { get; set; }

        /// <summary>
        /// The <see cref="ActionType"/> determined by the comparison.
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        /// The path to which the change occurred.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The <see cref="OperationType"/> to which the change occurred.
        /// </summary>
        public OperationType OperationType { get; set; }

        /// <summary>
        /// The <see cref="IOpenApiElement"/> corresponding to the before <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <remarks>
        /// The value may be null for a new element.
        /// </remarks>
        public IOpenApiElement Before { get; set; }

        /// <summary>
        /// The <see cref="IOpenApiElement"/> corresponding to the after <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <remarks>
        /// The value may be null for a deleted element.
        /// </remarks>
        public IOpenApiElement After { get; set; }
    }
}