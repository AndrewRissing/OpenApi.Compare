using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using OpenApi.Compare.Models;
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
        private static Dictionary<string, string> dctSamples;

        static OpenApiComparerTests()
        {
            dctSamples = new Dictionary<string, string>();

            var assembly = typeof(OpenApiComparerTests).Assembly;

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                var shortName = resourceName.Replace("OpenApi.Compare.Tests.Samples.", string.Empty).Replace(".json", string.Empty);
                var content = GetStringFromResource(assembly, resourceName);

                dctSamples[shortName] = content;
            }
        }

        private static string GetStringFromResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
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

                if (expectedChange.Before == null)
                {
                    Assert.Null(actualChange.Before);
                }
                else
                {
                    Assert.NotNull(actualChange.Before);
                    Assert.Same(expectedChange.Before.GetType(), actualChange.Before.GetType()); // TODO: Check more than type?
                }

                if (expectedChange.After == null)
                {
                    Assert.Null(actualChange.After);
                }
                else
                {
                    Assert.NotNull(actualChange.After);
                    Assert.Same(expectedChange.After.GetType(), actualChange.After.GetType()); // TODO: Check more than type?
                }

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
                // No change.
                Tuple.Create<Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    (x) =>
                    {
                        // Do nothing.
                    },
                    new ComparisonReport() { OverallCompatibility = Compatibility.NoChange },
                    new ComparisonReport() { OverallCompatibility = Compatibility.NoChange }
                ),

                // Add/Removing Paths
                Tuple.Create<Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    (x) =>
                    {
                        x.Paths["/new"] = new OpenApiPathItem()
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
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/new",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Added,
                                ChangeType = ChangeType.Operation,
                                Compatibility = Compatibility.Backwards,
                                Before = null,
                                After = new OpenApiOperation(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/new",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Removed,
                                ChangeType = ChangeType.Operation,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiOperation(),
                                After = null,
                            }
                        }
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
            return new OpenApiStringReader().Read(dctSamples[name], out var _);
        }
    }
}