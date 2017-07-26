using System;

namespace SmokeTest
{
    public class Constants
    {
        public const string SmokeTestAgentName = "Smoke Test Agent";
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
                public static readonly Guid Tests = new Guid("60CC6D15-5D97-4498-8D15-FCDCAC7601A1");
            }

            public class Fields
            {
                public class Tests
                {
                    public static readonly Guid ArtifactId = new Guid("DA9A845B-0160-43CF-9782-BEE16B9A679F");
                    public static readonly Guid Name = new Guid("136A347B-A7F7-465F-8923-62EA406D80A7");
                    public static readonly Guid Result = new Guid("88FAE1B1-5E2E-426B-A5EE-BEC5AA05C07C");
                    public static readonly Guid ErrorMessage = new Guid("683DF9FA-0D64-4EE3-8861-72F68A2BC4FC");
                }
            }
        }
    }
}
