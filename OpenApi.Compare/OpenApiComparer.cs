using Microsoft.OpenApi.Any;
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
                    (c, b, a) => CompareOpenApiContent(c, b, a, ChangeType.ParameterContentMediaType, ChangeType.ParameterContentStructure, ChangeType.ParameterContentEncoding, ChangeType.ParameterContentExample)
                );

                CompareValue(changes, before, after, ChangeType.Deprecated, Compatibility.Backwards, x => x.Deprecated);
                CompareValue(changes, before, after, ChangeType.Description, Compatibility.Backwards, x => x.Description);
                CompareOpenApiExampleValue(changes, before.Example, after.Example, ChangeType.ParameterExample);

                MatchForComparison
                (
                    changes,
                    before.Examples,
                    after.Examples,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiExampleMediaType(c, b, a, ChangeType.ParameterExample)
                );

                CompareValue(changes, before, after, ChangeType.ParameterExplode, Compatibility.Breaking, x => x.Explode);
                CompareValue(changes, before, after, ChangeType.ParameterIn, Compatibility.Breaking, x => x.In);
                CompareValue(changes, before, after, ChangeType.ParameterRequired, (before.Required) ? Compatibility.Backwards : Compatibility.Breaking, x => x.Required);
                CompareOpenApiSchema(changes, before.Schema, after.Schema, ChangeType.ParameterSchema);
                CompareValue(changes, before, after, ChangeType.ParameterStyle, Compatibility.Breaking, x => x.Style);
            }
        }

        private static void CompareOpenApiRequestBody(List<Change> changes, OpenApiRequestBody before, OpenApiRequestBody after)
        {
            if ((before == null) && (after == null))
            {
                // No change.  Ignore.
            }
            else if (before == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = ChangeType.RequestBody,
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
                    ChangeType = ChangeType.RequestBody,
                    Compatibility = Compatibility.Breaking,
                    Before = before,
                    After = after,
                });
            }
            else
            {
                CompareValue(changes, before, after, ChangeType.Description, Compatibility.Backwards, x => x.Description);
                CompareValue(changes, before, after, ChangeType.RequestBodyRequired, (before.Required) ? Compatibility.Backwards : Compatibility.Breaking, x => x.Required);

                MatchForComparison
                (
                    changes,
                    before.Content,
                    after.Content,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiContent(c, b, a, ChangeType.RequestBodyContentMediaType, ChangeType.RequestBodyContentStructure, ChangeType.RequestBodyContentEncoding, ChangeType.RequestBodyContentExample)
                );
            }
        }

        private static void CompareOpenApiResponse(List<Change> changes, KeyValuePair<string, OpenApiResponse> before, KeyValuePair<string, OpenApiResponse> after)
        {
            if (before.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = ChangeType.Response,
                    Compatibility = IsSuccessfulStatusCode(after.Key) ? Compatibility.Breaking : Compatibility.PotentiallyBreaking,
                    HttpStatusCode = after.Key,
                    Before = null,
                    After = after.Value,
                });
            }
            else if (after.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = ChangeType.Response,
                    Compatibility = IsSuccessfulStatusCode(before.Key) ? Compatibility.Breaking : Compatibility.PotentiallyBreaking,
                    HttpStatusCode = before.Key,
                    Before = before.Value,
                    After = null,
                });
            }
            else
            {
                var innerChanges = new List<Change>();

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Content,
                    after.Value.Content,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiContent(c, b, a, ChangeType.ResponseContentMediaType, ChangeType.ResponseContentStructure, ChangeType.ResponseContentEncoding, ChangeType.ResponseContentExample)
                );

                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Description, Compatibility.Backwards, x => x.Description);

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Headers,
                    after.Value.Headers,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiResponseHeader(c, b, a)
                );

                foreach (var change in innerChanges)
                    change.HttpStatusCode = before.Key;

                changes.AddRange(innerChanges);
            }
        }

        internal static bool IsSuccessfulStatusCode(string statusCode)
        {
            return (int.TryParse(statusCode, out var value) && (value >= 200) && (value <= 299));
        }

        private static void CompareOpenApiResponseHeader(List<Change> changes, KeyValuePair<string, OpenApiHeader> before, KeyValuePair<string, OpenApiHeader> after)
        {
            if (before.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = ChangeType.ResponseHeader,
                    Compatibility = Compatibility.Backwards,
                    ResponseHeader = after.Key,
                    Before = null,
                    After = after.Value,
                });
            }
            else if (after.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = ChangeType.ResponseHeader,
                    Compatibility = Compatibility.Breaking,
                    ResponseHeader = before.Key,
                    Before = before.Value,
                    After = null,
                });
            }
            else
            {
                var innerChanges = new List<Change>();

                CompareValue(changes, before.Value, after.Value, ChangeType.ResponseHeaderAllowEmptyValue, (before.Value.AllowEmptyValue) ? Compatibility.Breaking : Compatibility.Backwards, x => x.AllowEmptyValue);
                CompareValue(changes, before.Value, after.Value, ChangeType.ResponseHeaderAllowReserved, (before.Value.AllowReserved) ? Compatibility.Breaking : Compatibility.Backwards, x => x.AllowReserved);

                MatchForComparison
                (
                    changes,
                    before.Value.Content,
                    after.Value.Content,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiContent(c, b, a, ChangeType.ResponseHeaderContentMediaType, ChangeType.ResponseHeaderContentStructure, ChangeType.ResponseHeaderContentEncoding, ChangeType.ResponseHeaderContentExample)
                );

                CompareValue(changes, before.Value, after.Value, ChangeType.Deprecated, Compatibility.Backwards, x => x.Deprecated);
                CompareValue(changes, before.Value, after.Value, ChangeType.Description, Compatibility.Backwards, x => x.Description);
                CompareOpenApiExampleValue(changes, before.Value.Example, after.Value.Example, ChangeType.ResponseHeaderExample);

                MatchForComparison
                (
                    changes,
                    before.Value.Examples,
                    after.Value.Examples,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiExampleMediaType(c, b, a, ChangeType.ResponseHeaderExample)
                );

                CompareValue(changes, before.Value, after.Value, ChangeType.ResponseHeaderExplode, Compatibility.Breaking, x => x.Explode);
                CompareValue(changes, before.Value, after.Value, ChangeType.ResponseHeaderRequired, (before.Value.Required) ? Compatibility.Backwards : Compatibility.Breaking, x => x.Required);
                CompareOpenApiSchema(changes, before.Value.Schema, after.Value.Schema, ChangeType.ParameterSchema);
                CompareValue(changes, before.Value, after.Value, ChangeType.ParameterStyle, Compatibility.Breaking, x => x.Style);

                foreach (var change in innerChanges)
                    change.ResponseHeader = before.Key;

                changes.AddRange(innerChanges);
            }
        }

        private static void CompareOpenApiContent(List<Change> changes, KeyValuePair<string, OpenApiMediaType> before, KeyValuePair<string, OpenApiMediaType> after, ChangeType mediaTypeChange, ChangeType structureChange, ChangeType encodingChange, ChangeType exampleChange)
        {
            if (before.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = mediaTypeChange,
                    Compatibility = Compatibility.Backwards,
                    MediaType = after.Key,
                    Before = null,
                    After = after.Value,
                });
            }
            else if (after.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = mediaTypeChange,
                    Compatibility = Compatibility.Breaking,
                    MediaType = before.Key,
                    Before = before.Value,
                    After = null,
                });
            }
            else
            {
                var innerChanges = new List<Change>();

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Encoding,
                    after.Value.Encoding,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiEncoding(c, b, a, encodingChange)
                );

                CompareOpenApiExampleValue(innerChanges, before.Value.Example, after.Value.Example, exampleChange);

                MatchForComparison
                (
                    innerChanges,
                    before.Value.Examples,
                    after.Value.Examples,
                    r => r.Key,
                    (c, b, a) => CompareOpenApiExampleMediaType(c, b, a, exampleChange)
                );

                CompareOpenApiSchema(innerChanges, before.Value.Schema, after.Value.Schema, structureChange);

                foreach (var change in innerChanges)
                    change.MediaType = before.Key;

                changes.AddRange(innerChanges);
            }
        }

        private static void CompareOpenApiEncoding(List<Change> changes, KeyValuePair<string, OpenApiEncoding> before, KeyValuePair<string, OpenApiEncoding> after, ChangeType encodingChange)
        {
            if (before.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = encodingChange,
                    Compatibility = Compatibility.Breaking,
                    MediaType = after.Key,
                    Before = null,
                    After = after.Value,
                });
            }
            else if (after.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = encodingChange,
                    Compatibility = Compatibility.Breaking,
                    MediaType = before.Key,
                    Before = before.Value,
                    After = null,
                });
            }
            else
            {
                var innerChanges = new List<Change>();

                // TODO: Review the following logic in more detail.
                // TODO: Break out these encoding changes into smaller types?
                CompareValue(innerChanges, before.Value, after.Value, encodingChange, (before.Value.AllowReserved ?? false) ? Compatibility.Breaking : Compatibility.Backwards, x => x.AllowReserved);
                CompareValue(innerChanges, before.Value, after.Value, encodingChange, Compatibility.Breaking, x => x.ContentType);
                CompareValue(innerChanges, before.Value, after.Value, encodingChange, Compatibility.Breaking, x => x.Explode);

                // TODO: Headers

                CompareValue(innerChanges, before.Value, after.Value, encodingChange, Compatibility.Breaking, x => x.Style);

                foreach (var change in innerChanges)
                    change.EncodingPropertyName = before.Key;

                changes.AddRange(innerChanges);
            }
        }

        private static void CompareOpenApiExampleMediaType(List<Change> changes, KeyValuePair<string, OpenApiExample> before, KeyValuePair<string, OpenApiExample> after, ChangeType changeType)
        {
            if (before.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = changeType,
                    Compatibility = Compatibility.Backwards,
                    MediaType = after.Key,
                    Before = null,
                    After = after.Value,
                });
            }
            else if (after.Value == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = changeType,
                    Compatibility = Compatibility.Backwards,
                    MediaType = before.Key,
                    Before = before.Value,
                    After = null,
                });
            }
            else
            {
                var innerChanges = new List<Change>();

                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Description, Compatibility.Backwards, x => x.Description);
                CompareValue(innerChanges, before.Value, after.Value, ChangeType.Summary, Compatibility.Backwards, x => x.Summary);
                CompareOpenApiExampleValue(innerChanges, before.Value.Value, after.Value.Value, changeType);

                foreach (var change in innerChanges)
                    change.MediaType = before.Key;

                changes.AddRange(innerChanges);
            }
        }

        private static void CompareOpenApiExampleValue(List<Change> changes, IOpenApiAny before, IOpenApiAny after, ChangeType changeType)
        {
            if ((before == null) && (after == null))
            {
                // No change.  Ignore.
            }
            else if (before == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Added,
                    ChangeType = changeType,
                    Compatibility = Compatibility.Backwards,
                    Before = before,
                    After = after,
                });
            }
            else if (after == null)
            {
                changes.Add(new Change()
                {
                    ActionType = ActionType.Removed,
                    ChangeType = changeType,
                    Compatibility = Compatibility.Backwards,
                    Before = before,
                    After = after,
                });
            }
            else
            {
                // TODO: Compare values.
            }
        }

        private static void CompareOpenApiSchema(List<Change> changes, OpenApiSchema before, OpenApiSchema after, ChangeType changeType)
        {
            // TODO: Flesh out and check for nulls (both nulls)
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