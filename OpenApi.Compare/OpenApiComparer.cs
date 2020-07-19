using Microsoft.OpenApi.Models;
using OpenApi.Compare.Models;
using System.Collections.Generic;
using System.Linq;

namespace OpenApi.Compare
{
    // TODO: Add a way to filter down which items you care to look for?
    // - All or only breaking changes
    // - Specific types (documentation, adding new parameters, etc.)?

    /// <summary>
    /// The <see cref="OpenApiComparer"/> provides a means to compare OpenApi specifications to check for differences.
    /// </summary>
    public static class OpenApiComparer
    {
        /// <summary>
        /// Compares two <see cref="OpenApiDocument"/>s to check for differences.
        /// </summary>
        /// <param name="before">The <see cref="OpenApiDocument"/> before a change.</param>
        /// <param name="after">The <see cref="OpenApiDocument"/> after a change.</param>
        /// <returns>The <see cref="ComparisonReport"/> containing the result of the comparison.</returns>
        public static ComparisonReport Compare(OpenApiDocument before, OpenApiDocument after)
        {
            var report = new ComparisonReport()
            {
                Before = before,
                After = after,
            };

            // TODO: Magic here.

            if (report.Changes.Count > 0)
            {
                report.OverallCompatibility = (report.Changes.Any(x => x.Compatibility == Compatibility.Breaking))
                    ? Compatibility.Breaking
                    : Compatibility.Backwards;
            }

            return report;
        }
    }
}