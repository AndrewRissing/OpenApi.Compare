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
        /// <remarks>
        /// If <see langword="null"/>, the <see cref="Change"/> is from an <see cref="OpenApiPathItem"/>.
        /// </remarks>
        public OperationType? OperationType { get; set; }

        /// <summary>
        /// The media type of the changed area.
        /// </summary>
        /// <remarks>
        /// The value may be null for changes that are not related to a media type.
        /// </remarks>
        public string MediaType { get; set; }

        /// <summary>
        /// The HTTP Status Code related to the given change.
        /// </summary>
        /// <remarks>
        /// The value may be null for changes that are not related to an HTTP Status Code.
        /// </remarks>
        public string HttpStatusCode { get; set; }

        /// <summary>
        /// The name of the Response header related to the given change.
        /// </summary>
        /// <remarks>
        /// The value may be null for changes that are not related to a response header.
        /// </remarks>
        public string ResponseHeader { get; set; }

        /// <summary>
        /// The name of the property whose encoding has changed.
        /// </summary>
        /// <remarks>
        /// The value may be null for changes that are not related to an encoding change.
        /// </remarks>
        public string EncodingPropertyName { get; set; }

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