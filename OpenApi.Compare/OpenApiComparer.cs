using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using OpenApi.Compare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenApi.Compare
{
    // TODO: Add Security and other uncheck fields?

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
            var innerChanges = new List<Change>();

            MatchForComparison
            (
                innerChanges,
                before.Value?.Operations,
                after.Value?.Operations,
                kvp => kvp.Key,
                CompareOpenApiOperations
            );

            if ((before.Value != null) && (after.Value != null))
            {
                // These only execute if the path exists in both.
                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Description, Compatibility.Backwards, x => x.Description);

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Parameters,
                    after.Value.Parameters,
                    p => p.Name,
                    (c, b, a) => CompareOpenApiParameters(c, b, a, ChangeType.PathParameter)
                );

                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Summary, Compatibility.Backwards, x => x.Summary);
            }

            var path = before.Key ?? after.Key;

            // Set the path for any changes produced from subsequent calls.
            foreach (var change in innerChanges)
                change.Path = path;

            changes.AddRange(innerChanges);
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
                var innerChanges = new List<Change>();

                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Deprecated, Compatibility.Backwards, x => x.Deprecated);
                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Description, Compatibility.Backwards, x => x.Description);

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Parameters,
                    after.Value.Parameters,
                    p => p.Name,
                    (c, b, a) => CompareOpenApiParameters(c, b, a, ChangeType.OperationParameter)
                );

                CompareOpenApiRequestBody(innerChanges, before.Value.RequestBody, after.Value.RequestBody);

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Responses,
                    after.Value.Responses,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiResponse(c, b, a)
                );

                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Summary, Compatibility.Backwards, x => x.Summary);

                foreach (var change in innerChanges)
                    change.OperationType = before.Key;

                changes.AddRange(innerChanges);
            }
        }

        private static void CompareOpenApiParameters(List<Change> changes, OpenApiParameter before, OpenApiParameter after, ChangeType parameterChangeType)
        {
            if (before == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = parameterChangeType,
                    Compatibility = (after.Required) ? Compatibility.Breaking : Compatibility.Backwards,
                    Before = before,
                    After = after,
                });
            }
            else if (after == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = parameterChangeType,
                    Compatibility = Compatibility.Breaking,
                    Before = before,
                    After = after,
                });
            }
            else
            {
                CompareValue(changes, before, after, ChangeType.ParameterAllowEmptyValue, (before.AllowEmptyValue) ? Compatibility.Breaking : Compatibility.Backwards, x => x.AllowEmptyValue);
                CompareValue(changes, before, after, ChangeType.ParameterAllowReserved, (before.AllowReserved) ? Compatibility.Breaking : Compatibility.Backwards, x => x.AllowReserved);

                MatchForComparison
                (
                    changes,
                    before.Content,
                    after.Content,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiMediaType(c, b, a)
                );

                CompareValue(changes, before, after, ChangeType.Deprecated, Compatibility.Backwards, x => x.Deprecated);
                CompareValue(changes, before, after, ChangeType.Description, Compatibility.Backwards, x => x.Description);
                CompareValue(changes, before, after, ChangeType.ParameterExplode, Compatibility.Breaking, x => x.Explode);
                CompareValue(changes, before, after, ChangeType.ParameterIn, Compatibility.Breaking, x => x.In);
                CompareValue(changes, before, after, ChangeType.ParameterRequired, (before.Required) ? Compatibility.Backwards : Compatibility.Breaking, x => x.Required);
                CompareValue(changes, before, after, ChangeType.ParameterStyle, Compatibility.Breaking, x => x.Style);
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

        private static void CompareOpenApiRequestBody(List<Change> changes, OpenApiRequestBody before, OpenApiRequestBody after)
        {
            // TODO: Flesh out and check for nulls.
        }

        private static void CompareOpenApiResponse(List<Change> changes, KeyValuePair<string, OpenApiResponse> before, KeyValuePair<string, OpenApiResponse> after)
        {
            // TODO: Flesh out and check for nulls.
            // TODO: Sort out how to indicate which response code this was for?
        }

        private static void CompareOpenApiMediaType(List<Change> changes, KeyValuePair<string, OpenApiMediaType> before, KeyValuePair<string, OpenApiMediaType> after)
        {
            // TODO: Flesh out and check for nulls.
        }

        private static void CompareValue<T, TValue>(List<Change> changes, T before, T after, ChangeType changeType, Compatibility compatibility, Func<T, TValue> getValue)
            where T : IOpenApiElement
            where TValue : IComparable
        {
            var beforeValue = getValue(before);
            var afterValue = getValue(after);

            // Either one is null and the other isn't or they have differing values.
            if (((beforeValue != null) ^ (afterValue != null))
                || ((beforeValue != null) && (afterValue != null) && (beforeValue.CompareTo(afterValue) != 0)))
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Modified,
                    ChangeType = changeType,
                    Compatibility = compatibility,
                    Before = before,
                    After = after,
                });
            }
        }

        private static void CompareValue<T, TValue>(List<Change> changes, T before, T after, ChangeType changeType, Compatibility compatibility, Func<T, TValue?> getValue)
            where T : IOpenApiElement
            where TValue : struct, IComparable
        {
            var beforeValue = getValue(before);
            var afterValue = getValue(after);

            // Either one is null and the other isn't or they have differing values.
            if (((beforeValue.HasValue) ^ (afterValue.HasValue))
                || (beforeValue.HasValue && afterValue.HasValue && (beforeValue.Value.CompareTo(afterValue.Value) != 0)))
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Modified,
                    ChangeType = changeType,
                    Compatibility = compatibility,
                    Before = before,
                    After = after,
                });
            }
        }

        private static string RemoveParameterNamesFromPath(string path)
        {
            return Regex.Replace(path, @"\{(.*?)\}", "{}");
        }
    }
}