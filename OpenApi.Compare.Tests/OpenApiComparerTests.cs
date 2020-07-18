using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace OpenApi.Compare.Tests
{
    public class OpenApiComparerTests
    {
        [Theory]
        [MemberData(nameof(ScenarioData))]
        public void Scenarios_Forward(OpenApiDocument before, OpenApiDocument after, ComparisonReport expected)
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
                var before = CreateStandardOpenApiDocument();

                // Mutate the after.
                var after = CreateStandardOpenApiDocument();
                scenario.Item1(after);

                yield return new object[] { before, after, scenario.Item2 };

                // Update the report to swap its findings.
                var reversedComparisonReport = ReverseComparisonReport(scenario.Item2);

                // Rerun the test reversing before/after.
                yield return new object[] { after, before, reversedComparisonReport };
            }
        }

        public static OpenApiDocument CreateStandardOpenApiDocument()
        {
            // TODO: Fix this.
            return null;
        }

        public static ComparisonReport ReverseComparisonReport(ComparisonReport comparisonReport)
        {
            // TODO: Fix this.
            return null;
        }
    }
}