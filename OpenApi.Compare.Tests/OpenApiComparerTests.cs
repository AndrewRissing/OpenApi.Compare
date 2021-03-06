using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NUnit.Framework;
using OpenApi.Compare.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("abc", false)]
        [TestCase("199", false)]
        [TestCase("200", true)]
        [TestCase("201", true)]
        [TestCase("250", true)]
        [TestCase("299", true)]
        [TestCase("300", false)]
        public void IsSuccessfulStatusCode(string input, bool expected)
        {
            Assert.AreEqual(expected, OpenApiComparer.IsSuccessfulStatusCode(input));
        }

        [TestCaseSource(nameof(SimpleScenariosData))]
        public void SimpleScenarios(OpenApiDocument before, OpenApiDocument after, ComparisonReport expected)
        {
            var actual = OpenApiComparer.Compare(before, after);

            Assert.IsNotNull(actual);
            Assert.AreNotSame(expected, actual);
            Assert.AreSame(before, actual.Before);
            Assert.AreSame(after, actual.After);
            Assert.AreEqual(expected.OverallCompatibility, actual.OverallCompatibility);
            Assert.NotNull(actual.Changes);
            Assert.AreEqual(expected.Changes.Count, actual.Changes.Count);

            for (var i = 0; i < actual.Changes.Count; ++i)
            {
                var expectedChange = expected.Changes[i];
                var actualChange = actual.Changes[i];

                Assert.AreNotSame(expectedChange, actualChange);
                Assert.NotNull(actualChange);
                Assert.AreEqual(expectedChange.ActionType, actualChange.ActionType);

                if (expectedChange.Before == null)
                {
                    Assert.Null(actualChange.Before);
                }
                else
                {
                    Assert.NotNull(actualChange.Before);
                    Assert.IsInstanceOf(expectedChange.Before.GetType(), actualChange.Before);
                }

                if (expectedChange.After == null)
                {
                    Assert.Null(actualChange.After);
                }
                else
                {
                    Assert.NotNull(actualChange.After);
                    Assert.IsInstanceOf(expectedChange.After.GetType(), actualChange.After);
                }

                Assert.AreEqual(expectedChange.ChangeType, actualChange.ChangeType);
                Assert.AreEqual(expectedChange.Compatibility, actualChange.Compatibility);
                Assert.AreEqual(expectedChange.OperationType, actualChange.OperationType);
                Assert.AreEqual(expectedChange.Path, actualChange.Path);
                Assert.AreEqual(expectedChange.MediaType, actualChange.MediaType);
                Assert.AreEqual(expectedChange.HttpStatusCode, actualChange.HttpStatusCode);
                Assert.AreEqual(expectedChange.ResponseHeader, actualChange.ResponseHeader);
                Assert.AreEqual(expectedChange.EncodingPropertyName, actualChange.EncodingPropertyName);
            }
        }

        public static IEnumerable<TestCaseData> SimpleScenariosData()
        {
            var scenarios = new Tuple<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>[]
            {
                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "No change",
                    (x) =>
                    {
                        // Do nothing.
                    },
                    new ComparisonReport() { OverallCompatibility = Compatibility.NoChange },
                    new ComparisonReport() { OverallCompatibility = Compatibility.NoChange }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation",
                    (x) =>
                    {
                        x.Paths["/new"] = new OpenApiPathItem()
                        {
                            Operations =
                            {
                                {
                                    OperationType.Get, new OpenApiOperation()
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

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Deprecated",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Deprecated = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Deprecated,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiOperation(),
                                After = new OpenApiOperation(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Deprecated,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiOperation(),
                                After = new OpenApiOperation(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Description",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Description = "changed";
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiOperation(),
                                After = new OpenApiOperation(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiOperation(),
                                After = new OpenApiOperation(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Optional Parameter",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters.Add(new OpenApiParameter()
                        {
                            Name = "optional",
                            Required = false,
                        });
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Added,
                                ChangeType = ChangeType.OperationParameter,
                                Compatibility = Compatibility.Backwards,
                                Before = null,
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Removed,
                                ChangeType = ChangeType.OperationParameter,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = null,
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Required Parameter",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters.Add(new OpenApiParameter()
                        {
                            Name = "required",
                            Required = true,
                        });
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Added,
                                ChangeType = ChangeType.OperationParameter,
                                Compatibility = Compatibility.Breaking,
                                Before = null,
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Removed,
                                ChangeType = ChangeType.OperationParameter,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = null,
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - AllowEmptyValue",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].AllowEmptyValue = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowEmptyValue,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowEmptyValue,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - AllowReserved",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].AllowReserved = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowReserved,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowReserved,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - Deprecated",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].Deprecated = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Deprecated,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Deprecated,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - Description",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].Description = "Something new";
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - Explode",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].Explode = false;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterExplode,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterExplode,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - In",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].In = ParameterLocation.Header;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterIn,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterIn,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - Required",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].Required = false;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterRequired,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterRequired,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Parameter - Style",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Parameters[0].Style = ParameterStyle.SpaceDelimited;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterStyle,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterStyle,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Operation Summary",
                    (x) =>
                    {
                        x.Paths["/pet/findByStatus"].Operations[OperationType.Get].Summary = "changed";
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Summary,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiOperation(),
                                After = new OpenApiOperation(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet/findByStatus",
                                OperationType = OperationType.Get,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Summary,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiOperation(),
                                After = new OpenApiOperation(),
                            }
                        }
                    }
                ),

               Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Description",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Description = "changed";
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiPathItem(),
                                After = new OpenApiPathItem(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiPathItem(),
                                After = new OpenApiPathItem(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Optional Parameter",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters.Add(new OpenApiParameter()
                        {
                            Name = "optional",
                            Required = false,
                        });
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Added,
                                ChangeType = ChangeType.PathParameter,
                                Compatibility = Compatibility.Backwards,
                                Before = null,
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Removed,
                                ChangeType = ChangeType.PathParameter,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = null,
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Required Parameter",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters.Add(new OpenApiParameter()
                        {
                            Name = "required",
                            Required = true,
                        });
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Added,
                                ChangeType = ChangeType.PathParameter,
                                Compatibility = Compatibility.Breaking,
                                Before = null,
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Removed,
                                ChangeType = ChangeType.PathParameter,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = null,
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - AllowEmptyValue",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].AllowEmptyValue = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowEmptyValue,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowEmptyValue,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - AllowReserved",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].AllowReserved = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowReserved,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterAllowReserved,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - Deprecated",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].Deprecated = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Deprecated,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Deprecated,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - Description",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].Description = "Something new";
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Description,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - Explode",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].Explode = true;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterExplode,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterExplode,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - In",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].In = ParameterLocation.Header;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterIn,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterIn,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - Required",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].Required = false;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterRequired,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterRequired,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

                Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Parameter - Style",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Parameters[0].Style = ParameterStyle.SpaceDelimited;
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Breaking,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterStyle,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
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
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.ParameterStyle,
                                Compatibility = Compatibility.Breaking,
                                Before = new OpenApiParameter(),
                                After = new OpenApiParameter(),
                            }
                        }
                    }
                ),

               Tuple.Create<string, Action<OpenApiDocument>, ComparisonReport, ComparisonReport>
                (
                    "Path Summary",
                    (x) =>
                    {
                        x.Paths["/pet2/{petId}"].Summary = "changed";
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Summary,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiPathItem(),
                                After = new OpenApiPathItem(),
                            }
                        }
                    },
                    new ComparisonReport()
                    {
                        OverallCompatibility = Compatibility.Backwards,
                        Changes =
                        {
                            new Change()
                            {
                                Path = "/pet2/{petId}",
                                OperationType = null,
                                ActionType = ActionType.Modified,
                                ChangeType = ChangeType.Summary,
                                Compatibility = Compatibility.Backwards,
                                Before = new OpenApiPathItem(),
                                After = new OpenApiPathItem(),
                            }
                        }
                    }
                ),

               // TODO: Add tests for ParameterContentEncoding.
               // TODO: Add tests for ParameterContentEncoding - AllowReserved
               // TODO: Add tests for ParameterContentEncoding - ContentType
               // TODO: Add tests for ParameterContentEncoding - Explode
               // TODO: Add tests for ParameterContentEncoding - Style
               // TODO: Add tests for ParameterContentExample - MediaType
               // TODO: Add tests for ParameterContentExample - Description
               // TODO: Add tests for ParameterContentExample - Summary
               // TODO: Add tests for ParameterContentExample - Value
               // TODO: Add tests for ParameterContentMediaType.
               // TODO: Add tests for ParameterContentStructure.
               // TODO: Add tests for ParameterExample for Example - MediaType
               // TODO: Add tests for ParameterExample for Example - Description
               // TODO: Add tests for ParameterExample for Example - Summary
               // TODO: Add tests for ParameterExample for Example - Value
               // TODO: Add tests for ParameterExample for Examples - MediaType
               // TODO: Add tests for ParameterExample for Examples - Description
               // TODO: Add tests for ParameterExample for Examples - Summary
               // TODO: Add tests for ParameterExample for Examples - Value
               // TODO: Add tests for RequestBody.
               // TODO: Add tests for RequestBodyContentEncoding.
               // TODO: Add tests for RequestBodyContentEncoding - AllowReserved
               // TODO: Add tests for RequestBodyContentEncoding - ContentType
               // TODO: Add tests for RequestBodyContentEncoding - Explode
               // TODO: Add tests for RequestBodyContentEncoding - Style
               // TODO: Add tests for RequestBodyContentExample - MediaType
               // TODO: Add tests for RequestBodyContentExample - Description
               // TODO: Add tests for RequestBodyContentExample - Summary
               // TODO: Add tests for RequestBodyContentExample - Value
               // TODO: Add tests for RequestBodyContentMediaType.
               // TODO: Add tests for RequestBodyContentStructure.
               // TODO: Add tests for RequestBody.Description.
               // TODO: Add tests for RequestBody.Required.
               // TODO: Add tests for Response (2XX).
               // TODO: Add tests for Response (3XX, 4XX, 5XX).
               // TODO: Add tests for ResponseContentEncoding.
               // TODO: Add tests for ResponseContentEncoding - AllowReserved
               // TODO: Add tests for ResponseContentEncoding - ContentType
               // TODO: Add tests for ResponseContentEncoding - Explode
               // TODO: Add tests for ResponseContentEncoding - Style
               // TODO: Add tests for ResponseContentExample - MediaType
               // TODO: Add tests for ResponseContentExample - Description
               // TODO: Add tests for ResponseContentExample - Summary
               // TODO: Add tests for ResponseContentExample - Value
               // TODO: Add tests for ResponseContentMediaType.
               // TODO: Add tests for ResponseContentStructure.
               // TODO: Add tests for Response.Description.
               // TODO: Add tests for ResponseHeader.
               // TODO: Add tests for ResponseHeaderAllowEmptyValue.
               // TODO: Add tests for ResponseHeaderAllowReserved.
               // TODO: Add tests for ResponseHeaderContentEncoding.
               // TODO: Add tests for ResponseHeaderContentEncoding - AllowReserved
               // TODO: Add tests for ResponseHeaderContentEncoding - ContentType
               // TODO: Add tests for ResponseHeaderContentEncoding - Explode
               // TODO: Add tests for ResponseHeaderContentEncoding - Style
               // TODO: Add tests for ResponseHeaderContentExample - MediaType
               // TODO: Add tests for ResponseHeaderContentExample - Description
               // TODO: Add tests for ResponseHeaderContentExample - Summary
               // TODO: Add tests for ResponseHeaderContentExample - Value
               // TODO: Add tests for ResponseHeaderContentMediaType.
               // TODO: Add tests for ResponseHeaderContentStructure.
               // TODO: Add tests for ResponseHeaderExample - MediaType
               // TODO: Add tests for ResponseHeaderExample - Description
               // TODO: Add tests for ResponseHeaderExample - Summary
               // TODO: Add tests for ResponseHeaderExample - Value
               // TODO: Add tests for ResponseHeaderExplode.
               // TODO: Add tests for ResponseHeaderRequired.
               // TODO: Add tests for ResponseHeaderSchema.
               // TODO: Add tests for ResponseHeaderStyle.
            };

            foreach (var scenario in scenarios)
            {
                var before = GetSample("PetStore");

                // Mutate the after.
                var after = GetSample("PetStore");
                scenario.Item2(after);

                var report = scenario.Item3;
                report.Before = before;
                report.After = after;

                yield return new TestCaseData(before, after, scenario.Item3).SetName($"{{m}} ({scenario.Item1})");

                // Rerun the test reversing before/after, using the reversed report.
                var reversedReport = scenario.Item4;
                reversedReport.Before = after;
                reversedReport.After = before;

                yield return new TestCaseData(after, before, reversedReport).SetName($"{{m}} ({scenario.Item1} - Reversed)");
            }
        }

        private static OpenApiDocument GetSample(string name)
        {
            return new OpenApiStringReader().Read(dctSamples[name], out var _);
        }
    }
}