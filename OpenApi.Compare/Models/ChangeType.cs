using Microsoft.OpenApi.Models;

namespace OpenApi.Compare
{
    /// <summary>
    /// <see cref="ChangeType"/> describes the type of change that occurred.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// An <see cref="OpenApiPathItem"/> was changed.
        /// </summary>
        Path = 0,

        // TODO: Flesh this out and add comments.

        ///// <summary>
        ///// TODO: Figure out what to do about this.
        ///// </summary>
        //Documentation, // TODO: Might be too generic (Summary, Description, etc.?)

        //Operation,

        //Response,

        //RequestBody,

        //Parameter,

        //Tag
    }
}