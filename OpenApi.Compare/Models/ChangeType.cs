using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace OpenApi.Compare
{
    // TODO: Flesh this out and add comments.

    /// <summary>
    /// <see cref="ChangeType"/> describes the type of change that occurred.
    /// </summary>
    public enum ChangeType
    {
        // TODO: Paths?
        //Path = 0,

        /// <summary>
        /// The deprecated status of the provided <see cref="IOpenApiElement"/> was changed.
        /// </summary>
        Deprecated,

        /// <summary>
        /// The description of the provided <see cref="IOpenApiElement"/> was changed.
        /// </summary>
        Description,

        /// <summary>
        /// An <see cref="OpenApiPathItem"/> was changed.
        /// </summary>
        Operation,

        /// <summary>
        /// A <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        Parameter,

        /// <summary>
        /// The <see cref="OpenApiParameter.AllowEmptyValue"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterAllowEmptyValue,

        /// <summary>
        /// The <see cref="OpenApiParameter.In"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterIn,

        /// <summary>
        /// The <see cref="OpenApiParameter.Required"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterRequired,

        /// <summary>
        /// The summary of the provided <see cref="IOpenApiElement"/> was changed.
        /// </summary>
        Summary,
    }
}