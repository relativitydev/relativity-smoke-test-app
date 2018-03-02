using System;

namespace SmokeTest
{
    public class Constants
    {
        public const string SmokeTestRunnerAgentName = "Smoke Test Runner Agent";
        public const string TestAgentToCreateName = "Dummy Agent";
        public static readonly string Prefix = "ST";
        public static readonly int GroupIdentifierFieldArtifactId = 1003671;
        public const string AllDocumentsSavedSearchName = "All Documents";

        public const int NumberOfTextFilesToCreate = 10;
        public const String ProcessingLoadFilePath = "C:\\Program Files\\kCura Corporation\\Relativity\\Licenses\\jQuery";

        public class TestResultsStatus
        {
            public static readonly string Success = "Success";
            public static readonly string Fail = "Fail";
        }

        public class Guids
        {
            public class Application
            {
                public static readonly Guid SmokeTest = new Guid("0125C8D4-8354-4D8F-B031-01E73C866C7C");
                public static readonly Guid Processing = new Guid("ED0E23F9-DA60-4298-AF9A-AE6A9B6A9319");
            }

            public class ObjectType
            {
                public static readonly Guid Test = new Guid("71ED667F-EE38-4BC7-AB76-6645D8A5587F");
<<<<<<< HEAD
                public static readonly Guid ImagingProfile = new Guid("C6FAC105-3493-4551-B956-4757066E622F");
                public static readonly Guid ImagingSet = new Guid("BA574E88-7408-4434-A688-2324ECFC769E");
                public static readonly Guid ImagingSetScheduler = new Guid("45C9FEB9-43D8-43FE-A216-85B1F062B0A7");
                public static readonly Guid ProcessingProfile = new Guid("4BE0A8E2-C236-4DAC-B8DF-E7944B84CEE5");
                public static readonly Guid RelaticityTimeZone = new Guid("BC411C4D-285A-466B-8824-E084FD981F8B");
                public static readonly Guid ProcessingSet = new Guid("45B1F80D-C4E7-4A8D-A72A-ED9E21F89900");
                public static readonly Guid Custodian = new Guid("D216472D-A1AA-4965-8B36-367D43D4E64C");
=======
>>>>>>> bdec342a7ff8da0af805822b2d1e9cd7ad8a6db0
            }

            public class Fields
            {
                public class Test
                {
                    public static readonly Guid ArtifactId = new Guid("FB785856-785C-4E42-9772-6F69A4ED3119");
                    public static readonly Guid Name = new Guid("9D9F70EF-0AB1-4912-849E-418976EA92CF");
                    public static readonly Guid Result = new Guid("A057E0A2-73CE-44B1-AF8D-F8B896358446");
                    public static readonly Guid ErrorMessage = new Guid("D289FEAA-76F4-4E82-86D6-E2E522F42A71");
                }
<<<<<<< HEAD

                public class ImagingSet
                {
                    public static readonly Guid Name = new Guid("0876B4E6-8BF7-48A4-851B-A6A2A208D6CC");
                    public static readonly Guid DataSource = new Guid("A539FF48-8418-44FC-B9DD-26152C01F112");
                    public static readonly Guid ImagingProfile = new Guid("4011A9DB-625F-4553-9E9E-3CFC42488B5F");
                    public static readonly Guid EmailNotificationRecipients = new Guid("39F1D115-3AF1-4818-8660-07F774D99EB8");
                    public static readonly Guid Status = new Guid("030747E3-E154-4DF1-BD10-CF6C9734D10A");
                }

                public class ImagingSetScheduler
                {
                    public static readonly Guid Name = new Guid("BED4644C-3144-44B1-A3BB-C7808182B417");
                    public static readonly Guid ImagingSet = new Guid("5DE74965-B185-46EF-984A-D1B18F1781C0");
                    public static readonly Guid Frequency = new Guid("8E0278D2-93A8-40DE-A82E-C4D3E96A2C2E");
                    public static readonly Guid Time = new Guid("DF57565B-3685-439D-91C1-091678F507F9");
                    public static readonly Guid LockImagesForQc = new Guid("11FDD822-CE72-4540-BEF9-78D9E7B8A673");
                    public static readonly Guid NextRun = new Guid("284C4AD2-A4CA-4415-9247-09618C68D0AD");
                }

                public class ProcessingProfile
                {
                    public static Guid DeNist = new Guid("04AB3BCA-1A74-4541-92CA-FD6F6D741EEA");
                }

                public class ProcessingSet
                {
                    public static Guid Discover_Status = new Guid("513DD373-661B-4EA5-9AC4-43BEA2F793EE");
                    public static Guid Publish_Status = new Guid("E3343C3E-0FFA-4846-B4D3-CB1E5A37140C");
                }
=======
>>>>>>> bdec342a7ff8da0af805822b2d1e9cd7ad8a6db0
            }

            public class Choices
            {
<<<<<<< HEAD
                public class ImagingSetSchedulerFrequency
                {
                    public static readonly Guid Sunday = new Guid("532A3B0D-F0EE-4F5C-9516-E7F698E62B27");
                    public static readonly Guid Monday = new Guid("392E052E-EC84-45FD-A15B-C0AF073273B6");
                    public static readonly Guid Tuesday = new Guid("04863E57-33CD-46E0-B0E0-AB2308AF6388");
                    public static readonly Guid Wednesday = new Guid("6A544774-D0CD-4F18-8AD0-33FE4291667A");
                    public static readonly Guid Thursday = new Guid("AE22F86F-D703-469A-9D93-8BA4B8636A42");
                    public static readonly Guid Friday = new Guid("7F6F9F4E-A76B-4202-A7A1-643168C799CA");
                    public static readonly Guid Saturday = new Guid("C7E2341D-9AF6-4925-AA00-695678979EA1");
                }

                public class Processing
                {
                    public class DiscoverStatus
                    {
                        public static readonly Guid Completed = new Guid("BE180E9D-8ABF-41F8-8507-37BD1D22F72E");
                        public static readonly Guid CompletedWithErrors = new Guid("F29B76D6-927F-4172-BC7A-92EB7B7328C9");
                    }
                    public class PublishStatus
                    {
                        public static readonly Guid Completed = new Guid("2682816F-2683-4CC0-BE6E-583FBDFF304B");
                        public static readonly Guid CompletedWithErrors = new Guid("410C43FD-8A90-4E64-A5AF-5E4CDCFB99A7");
                    }
                }
=======

>>>>>>> bdec342a7ff8da0af805822b2d1e9cd7ad8a6db0
            }
        }

        public class ImagingSetJobStatus
        {
            public const string Completed = "Completed";
        }

        public enum ProcessingJobType
        {
            Discover,
            Publish
        }
    }
}
