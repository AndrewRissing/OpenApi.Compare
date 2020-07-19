using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace OpenApi.Compare.Tests
{
    public class OpenApiComparerTests
    {
        // TODO: Do we need multiple?
        private static Dictionary<string, OpenApiDocument> dctSamples;

        static OpenApiComparerTests()
        {
            dctSamples = new Dictionary<string, OpenApiDocument>();

            var assembly = typeof(OpenApiComparerTests).Assembly;

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                var shortName = resourceName.Replace("OpenApi.Compare.Tests.Samples.", string.Empty).Replace(".json", string.Empty);
                var openApiDocument = GetOpenApiDocumentFromResource(assembly, resourceName);

                dctSamples[shortName] = openApiDocument;
            }
        }

        private static OpenApiDocument GetOpenApiDocumentFromResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return new OpenApiStreamReader().Read(stream, out var _);
            }
        }

        [Theory]
        [MemberData(nameof(ScenarioData))]
        private void Scenarios(OpenApiDocument before, OpenApiDocument after, ComparisonReport expected)
        {
            // TODO: Flesh this out.
        }

        public static IEnumerable<object[]> ScenarioData()
        {
            var scenarios = new Tuple<Action<OpenApiDocument>, ComparisonReport>[]
            {
                Tuple.Create<Action<OpenApiDocument>, ComparisonReport>
                (
                    (x) =>
                    {
                        x.Paths["/blahblah"] = new OpenApiPathItem()
                        {

                        };
                    },
                    new ComparisonReport()
                    {

                    }
                ),
            };

            foreach (var scenario in scenarios)
            {
                var before = GetSample("PetStore");

                // Mutate the after.
                var after = GetSample("PetStore");
                scenario.Item1(after);

                yield return new object[] { before, after, scenario.Item2 };

                // Update the report to swap its findings.
                var reversedComparisonReport = ReverseComparisonReport(scenario.Item2);

                // Rerun the test reversing before/after.
                yield return new object[] { after, before, reversedComparisonReport };
            }
        }

        private static OpenApiDocument GetSample(string name)
        {
            return dctSamples[name];
        }

        public static ComparisonReport ReverseComparisonReport(ComparisonReport comparisonReport)
        {
            // TODO: Fix this.
            return null;
        }
    }
}