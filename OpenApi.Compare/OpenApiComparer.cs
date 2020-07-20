using Microsoft.OpenApi.Models;
using OpenApi.Compare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

            MatchForComparison
            (
                report.Changes,
                before.Paths,
                after.Paths,
                kvp => RemoveParameterNamesFromPath(kvp.Key),
                CompareOpenApiPathItems
            );

            if (report.Changes.Count > 0)
            {
                report.OverallCompatibility = (report.Changes.Any(x => x.Compatibility == Compatibility.Breaking))
                    ? Compatibility.Breaking
                    : Compatibility.Backwards;
            }

            return report;
        }

        private static void CompareOpenApiPathItems(List<Change> changes, KeyValuePair<string, OpenApiPathItem> before, KeyValuePair<string, OpenApiPathItem> after)
        {
            // TODO: Compare OpenApiPathItem properties?

            var pathChanges = new List<Change>();
            var path = before.Key ?? after.Key;

            MatchForComparison
            (
                pathChanges,
                before.Value?.Operations,
                after.Value?.Operations,
                kvp => kvp.Key,
                CompareOpenApiOperations
            );

            foreach (var change in pathChanges)
                change.Path = path;

            changes.AddRange(pathChanges);
        }

        private static void CompareOpenApiOperations(List<Change> changes, KeyValuePair<OperationType, OpenApiOperation> before, KeyValuePair<OperationType, OpenApiOperation> after)
        {
            if (before.Value == null)
            {
                changes.Add(new Change()
                {
                    OperationType = after.Key,
                    ActionType = ActionType.Added,
                    ChangeType = ChangeType.Operation,
                    Compatibility = Compatibility.Backwards,
                    Before = before.Value,
                    After = after.Value,
                });
            }
            else if (after.Value == null)
            {
                changes.Add(new Change()
                {
                    OperationType = before.Key,
                    ActionType = ActionType.Removed,
                    ChangeType = ChangeType.Operation,
                    Compatibility = Compatibility.Breaking,
                    Before = before.Value,
                    After = after.Value,
                });
            }
            else
            {
                // TODO: Compare OpenApiOperation properties.
                // operationChanges
            }
        }

        private static void MatchForComparison<T, TKey>
        (
            List<Change> changes,
            IEnumerable<T> before,
            IEnumerable<T> after,
            Func<T, TKey> keySelector,
            Action<List<Change>, T, T> comparer
        )
        {
            var dctBefore = (before ?? Enumerable.Empty<T>()).ToDictionary(keySelector);
            var dctAfter = (after ?? Enumerable.Empty<T>()).ToDictionary(keySelector);
            var allKeys = new HashSet<TKey>(dctBefore.Keys.Concat(dctAfter.Keys));

            foreach (var key in allKeys)
            {
                dctBefore.TryGetValue(key, out var beforeMatch);
                dctAfter.TryGetValue(key, out var afterMatch);

                comparer(changes, beforeMatch, afterMatch);
            }
        }

        private static string RemoveParameterNamesFromPath(string path)
        {
            return Regex.Replace(path, @"\{(.*?)\}", "{}");
        }
    }
}