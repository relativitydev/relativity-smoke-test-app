using Relativity.Services.Agent;
using System;

namespace SmokeTest
{
	public class Constants
	{
		public const string WORKSPACE_TEMPLATE_NAME = "Relativity Starter Template";
		public static readonly string Prefix = "ST";
		public static readonly int GroupIdentifierFieldArtifactId = 1003671;
		public const string AllDocumentsSavedSearchName = "All Documents";
		public const string DevVmComputerName = "RELATIVITYDEVVM";

		public const int NumberOfTextFilesToCreate = 10;
		public const String ProcessingLoadFilePath = "C:\\Program Files\\kCura Corporation\\Relativity\\Licenses\\jQuery";

		public class InstanceSetting
		{
			public const string Name = "SmokeTestShouldRunProcessingTest";
			public const string Section = "Relativity.SmokeTest";
		}

		public class Agents
		{
			public const string SMOKE_TEST_APPLICATION_NAME = "Smoke Test";
			public const string SMOKE_TEST_RUNNER_AGENT_NAME = "Smoke Test Runner Agent";
			public const string SMOKE_TEST_ANALYSIS_AGENT_NAME = "Smoke Test Analysis Agent";
			public const string TEST_AGENT_TO_CREATE_NAME = "Dummy Agent";
			public const string AGENT_OBJECT_TYPE = "Agent";
			public const string AGENT_FIELD_NAME = "Name";
			public const string KEYWORDS = SMOKE_TEST_APPLICATION_NAME;
			public const string NOTES = SMOKE_TEST_APPLICATION_NAME;
			public const int EDDS_WORKSPACE_ARTIFACT_ID = -1;
			public const bool ENABLE_AGENT = true;
			public const int AGENT_INTERVAL = 20;
			public const Agent.LoggingLevelEnum AGENT_LOGGING_LEVEL = Agent.LoggingLevelEnum.All;
		}

		public class Status
		{
			public class TestRdo
			{
				public static readonly string New = "New";
				public static readonly string RunningTest = "Running Test";
				public static readonly string Success = "Success";
				public static readonly string Fail = "Fail";
			}
		}

		public class Guids
		{
			public class Application
			{
				public static readonly Guid SmokeTest = new Guid("0125C8D4-8354-4D8F-B031-01E73C866C7C");
				public static readonly Guid Processing = new Guid("ED0E23F9-DA60-4298-AF9A-AE6A9B6A9319");
				public static readonly Guid DataGridCore = new Guid("6A8C2341-6888-44DA-B1A4-5BDCE0D1A383");
			}

			public class ObjectType
			{
				public static readonly Guid Test = new Guid("71ED667F-EE38-4BC7-AB76-6645D8A5587F");
				public static readonly Guid ProcessingProfile = new Guid("4BE0A8E2-C236-4DAC-B8DF-E7944B84CEE5");
				public static readonly Guid RelaticityTimeZone = new Guid("BC411C4D-285A-466B-8824-E084FD981F8B");
				public static readonly Guid ProcessingSet = new Guid("45B1F80D-C4E7-4A8D-A72A-ED9E21F89900");
				public static readonly Guid Custodian = new Guid("D216472D-A1AA-4965-8B36-367D43D4E64C");
			}

			public class Fields
			{
				public class Test
				{
					public static readonly Guid ArtifactId = new Guid("FB785856-785C-4E42-9772-6F69A4ED3119");
					public static readonly Guid Name = new Guid("9D9F70EF-0AB1-4912-849E-418976EA92CF");
					public static readonly Guid Status = new Guid("A057E0A2-73CE-44B1-AF8D-F8B896358446");
					public static readonly Guid Error = new Guid("D289FEAA-76F4-4E82-86D6-E2E522F42A71");
					public static readonly Guid ErrorDetails = new Guid("F615298C-EB1E-4892-B8AA-103CEE85888B");
					public static readonly Guid SystemLastModifiedOn = new Guid("E82F7D31-E05F-4D3F-8D7D-9A39097AC01E");
				}

				public class ProcessingProfile
				{
					public static Guid DeNist = new Guid("04AB3BCA-1A74-4541-92CA-FD6F6D741EEA");
				}

				public class ProcessingSet
				{
					public static Guid DiscoverStatus = new Guid("513DD373-661B-4EA5-9AC4-43BEA2F793EE");
					public static Guid PublishStatus = new Guid("E3343C3E-0FFA-4846-B4D3-CB1E5A37140C");
				}
			}

			public class Choices
			{
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
