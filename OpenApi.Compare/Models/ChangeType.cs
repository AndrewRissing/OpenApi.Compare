using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace OpenApi.Compare
{
    // TODO: Combine similar change types and rely upon Before/After and related properties for context?

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
        /// The <see cref="OpenApiMediaType.Encoding"/> of a <see cref="OpenApiParameter"/>'s <see cref="OpenApiParameter.Content"/> was changed.
        /// </summary>
        ParameterContentEncoding,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Example"/> or a <see cref="OpenApiMediaType.Examples"/> of a <see cref="OpenApiParameter"/>'s <see cref="OpenApiParameter.Content"/> was changed.
        /// </summary>
        ParameterContentExample,

        /// <summary>
        /// The media type of a <see cref="OpenApiParameter"/>'s <see cref="OpenApiParameter.Content"/> was changed.
        /// </summary>
        ParameterContentMediaType,

        /// <summary>
        /// The structure of a <see cref="OpenApiParameter"/>'s <see cref="OpenApiParameter.Content"/> was changed.
        /// </summary>
        ParameterContentStructure,

        /// <summary>
        /// The <see cref="OpenApiParameter.Example"/> or a <see cref="OpenApiParameter.Examples"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterExample,

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
        /// The <see cref="OpenApiParameter.Schema"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterSchema,

        /// <summary>
        /// The <see cref="OpenApiParameter.Style"/> of a <see cref="OpenApiParameter"/> was changed.
        /// </summary>
        ParameterStyle,

        /// <summary>
        /// A <see cref="OpenApiParameter"/> on a <see cref="OpenApiPathItem"/> was changed.
        /// </summary>
        PathParameter,

        /// <summary>
        /// A <see cref="OpenApiRequestBody"/> on an <see cref="OpenApiOperation"/> was changed.
        /// </summary>
        RequestBody,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Encoding"/> of a <see cref="OpenApiRequestBody"/>'s <see cref="OpenApiRequestBody.Content"/> was changed.
        /// </summary>
        RequestBodyContentEncoding,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Example"/> or a <see cref="OpenApiMediaType.Examples"/> of a <see cref="OpenApiRequestBody"/>'s <see cref="OpenApiRequestBody.Content"/> was changed.
        /// </summary>
        RequestBodyContentExample,

        /// <summary>
        /// The media type of a <see cref="OpenApiRequestBody"/>'s <see cref="OpenApiRequestBody.Content"/> was changed.
        /// </summary>
        RequestBodyContentMediaType,

        /// <summary>
        /// The structure of a <see cref="OpenApiRequestBody"/>'s <see cref="OpenApiRequestBody.Content"/> was changed.
        /// </summary>
        RequestBodyContentStructure,

        /// <summary>
        /// A <see cref="OpenApiRequestBody.Required"/> on a <see cref="OpenApiRequestBody"/> was changed.
        /// </summary>
        RequestBodyRequired,

        /// <summary>
        /// A <see cref="OpenApiResponse"/> on an <see cref="OpenApiOperation"/> was changed.
        /// </summary>
        Response,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Encoding"/> of a <see cref="OpenApiResponse"/>'s <see cref="OpenApiResponse.Content"/> was changed.
        /// </summary>
        ResponseContentEncoding,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Example"/> or a <see cref="OpenApiMediaType.Examples"/> of a <see cref="OpenApiResponse"/>'s <see cref="OpenApiResponse.Content"/> was changed.
        /// </summary>
        ResponseContentExample,

        /// <summary>
        /// The media type of a <see cref="OpenApiResponse"/>'s <see cref="OpenApiResponse.Content"/> was changed.
        /// </summary>
        ResponseContentMediaType,

        /// <summary>
        /// The structure of a <see cref="OpenApiResponse"/>'s <see cref="OpenApiResponse.Content"/> was changed.
        /// </summary>
        ResponseContentStructure,

        /// <summary>
        /// A <see cref="OpenApiHeader"/> on an <see cref="OpenApiResponse"/> was changed.
        /// </summary>
        ResponseHeader,

        /// <summary>
        /// The <see cref="OpenApiHeader.AllowEmptyValue"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderAllowEmptyValue,

        /// <summary>
        /// The <see cref="OpenApiHeader.AllowReserved"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderAllowReserved,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Encoding"/> of a <see cref="OpenApiHeader"/>'s <see cref="OpenApiHeader.Content"/> was changed.
        /// </summary>
        ResponseHeaderContentEncoding,

        /// <summary>
        /// The <see cref="OpenApiMediaType.Example"/> or a <see cref="OpenApiMediaType.Examples"/> of a <see cref="OpenApiHeader"/>'s <see cref="OpenApiHeader.Content"/> was changed.
        /// </summary>
        ResponseHeaderContentExample,

        /// <summary>
        /// The media type of a <see cref="OpenApiHeader"/>'s <see cref="OpenApiHeader.Content"/> was changed.
        /// </summary>
        ResponseHeaderContentMediaType,

        /// <summary>
        /// The structure of a <see cref="OpenApiHeader"/>'s <see cref="OpenApiHeader.Content"/> was changed.
        /// </summary>
        ResponseHeaderContentStructure,

        /// <summary>
        /// The <see cref="OpenApiHeader.Example"/> or a <see cref="OpenApiHeader.Examples"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderExample,

        /// <summary>
        /// The <see cref="OpenApiHeader.Explode"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderExplode,

        /// <summary>
        /// The <see cref="OpenApiHeader.Required"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderRequired,

        /// <summary>
        /// The <see cref="OpenApiHeader.Schema"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderSchema,

        /// <summary>
        /// The <see cref="OpenApiHeader.Style"/> of a <see cref="OpenApiHeader"/> was changed.
        /// </summary>
        ResponseHeaderStyle,

        /// <summary>
        /// The summary of the provided <see cref="IOpenApiElement"/> was changed.
        /// </summary>
        Summary,
    }
}