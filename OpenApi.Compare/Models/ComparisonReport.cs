using Microsoft.OpenApi.Models;
using OpenApi.Compare.Models;
using System.Collections.Generic;

namespace OpenApi.Compare
{
    /// <summary>
    /// The <see cref="ComparisonReport"/> contains details about differences between two <see cref="OpenApiDocument"/>s.
    /// </summary>
    public class ComparisonReport
    {
        /// <summary>
        /// The overall <see cref="Compatibility"/> determined by the comparison.
        /// </summary>
        public Compatibility OverallCompatibility { get; set; }

        /// <summary>
        /// The <see cref="OpenApiDocument"/> before the change.
        /// </summary>
        public OpenApiDocument Before { get; set; }

        /// <summary>
        /// The <see cref="OpenApiDocument"/> after the change.
        /// </summary>
        public OpenApiDocument After { get; set; }

        /// <summary>
        /// The list of <see cref="Change"/>s detected by the comparison.
        /// </summary>
        public List<Change> Changes { get; set; }
    }
}