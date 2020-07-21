using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace OpenApi.Compare
{
    /// <summary>
    /// <see cref="ChangeType"/> describes the type of change that occurred.
    /// </summary>
    public enum ChangeType
    {
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
        /// A <see cref="OpenApiParameter"/> on a <see cref="OpenApiOperation"/> was changed.
        /// </summary>
        OperationParameter,

        /// <summary>
        /// The <see cref="OpenApiParameter.AllowEmptyValue"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterAllowEmptyValue,

        /// <summary>
        /// The <see cref="OpenApiParameter.AllowReserved"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterAllowReserved,

        /// <summary>
        /// The <see cref="OpenApiParameter.Explode"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterExplode,

        /// <summary>
        /// The <see cref="OpenApiParameter.In"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterIn,

        /// <summary>
        /// The <see cref="OpenApiParameter.Required"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterRequired,

        /// <summary>
        /// The <see cref="OpenApiParameter.Style"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterStyle,

        /// <summary>
        /// A <see cref="OpenApiParameter"/> on a <see cref="OpenApiPathItem"/> was changed.
        /// </summary>
        PathParameter,

        /// <summary>
        /// The summary of the provided <see cref="IOpenApiElement"/> was changed.
        /// </summary>
        Summary,
    }
}