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
            var actual = OpenApiComparer.Compare(before, after);

            Assert.NotNull(actual);
            Assert.NotSame(expected, actual);
            Assert.Same(before, actual.Before);
            Assert.Same(after, actual.After);
            Assert.Equal(expected.OverallCompatibility, actual.OverallCompatibility);
            Assert.NotNull(actual.Changes);
            Assert.Equal(expected.Changes.Count, actual.Changes.Count);

            for (var i = 0; i < actual.Changes.Count; ++i)
            {
                var expectedChange = expected.Changes[i];
                var actualChange = actual.Changes[i];

                Assert.NotSame(expectedChange, actualChange);
                Assert.NotNull(actualChange);
                Assert.Equal(expectedChange.ActionType, actualChange.ActionType);
                Assert.Same(expectedChange.After, actualChange.After);
                Assert.Same(expectedChange.Before, actualChange.Before);
                Assert.Equal(expectedChange.ChangeType, actualChange.ChangeType);
                Assert.Equal(expectedChange.Compatibility, actualChange.Compatibility);
                Assert.Equal(expectedChange.OperationType, actualChange.OperationType);
                Assert.Equal(expectedChange.Path, actualChange.Path);
            }
        }

        public static IEnumerable<object[]> ScenarioData()
        {
            var scenarios = new Tuple<Action<OpenApiDocument>, ComparisonReport, ComparisonReport>[]
            {
                Tuple.Create<Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    (x) =>
                    {
                        x.Paths["/blahblah"] = new OpenApiPathItem()
                        {
                            Operations =
                            {
                                {
                                    OperationType.Get, new OpenApiOperation()
                                    {
                                        
                                    }
                                }
                            }
                        };
                    },
                    new ComparisonReport()
                    {
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

                var report = scenario.Item2;
                report.Before = before;
                report.After = after;

                yield return new object[] { before, after, scenario.Item2 };

                // Rerun the test reversing before/after, using the reversed report.
                var reversedReport = scenario.Item3;
                reversedReport.Before = after;
                reversedReport.After = before;

                yield return new object[] { after, before, reversedReport };
            }
        }

        private static OpenApiDocument GetSample(string name)
        {
            return dctSamples[name];
        }
    }
}