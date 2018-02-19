using System;

namespace SmokeTest
{
    public class Constants
    {
        public const string SmokeTestRunnerAgentName = "Smoke Test Runner Agent";
        public const string TestAgentToCreateName = "Dummy Agent";
        public static readonly string Prefix = "ST";
        public static readonly int GroupIdentifierFieldArtifactId = 1003671;

        public class TestResultsStatus
        {
            public static readonly string Success = "Success";
            public static readonly string Fail = "Fail";
        }

        public class Guids
        {
            public static readonly Guid Application = new Guid("0125C8D4-8354-4D8F-B031-01E73C866C7C");

            public class ObjectType
            {
                public static readonly Guid Test = new Guid("71ED667F-EE38-4BC7-AB76-6645D8A5587F");
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
            }
        }
    }
}
