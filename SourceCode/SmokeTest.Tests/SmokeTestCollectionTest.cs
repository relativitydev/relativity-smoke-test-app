using DbContextHelper;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using Relativity.Imaging.Services.Interfaces;
using Relativity.Processing.Services;
using Relativity.Productions.Services;
using Relativity.Services.InstanceSetting;
using Relativity.Services.Interfaces.DtSearchIndexManager;
using Relativity.Services.Objects;
using Relativity.Services.ResourcePool;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;
using SmokeTest.Models;

namespace SmokeTest.Tests
{
	[TestFixture]
	public class SmokeTestCollectionTest
	{
		public SmokeTestCollection Sut { get; set; }
		public ServiceFactory ServiceFactory { get; set; }
		public IRSAPIClient RsapiClient { get; set; }
		public Relativity.Services.Interfaces.Agent.IAgentManager AgentManager { get; set; }
		public IObjectManager ObjectManager { get; set; }
		public IProductionManager ProductionManager { get; set; }
		public IProductionDataSourceManager ProductionDataSourceManager { get; set; }
		public IKeywordSearchManager KeywordSearchManager { get; set; }
		public IImagingProfileManager ImagingProfileManager { get; set; }
		public IImagingSetManager ImagingSetManager { get; set; }
		public IImagingJobManager ImagingJobManager { get; set; }
		public IProcessingCustodianManager ProcessingCustodianManager { get; set; }
		public IProcessingSetManager ProcessingSetManager { get; set; }
		public IProcessingDataSourceManager ProcessingDataSourceManager { get; set; }
		public IResourcePoolManager ResourcePoolManager { get; set; }
		public IProcessingJobManager ProcessingJobManager { get; set; }
		public IdtSearchManager DtSearchManager { get; set; }
		public IDtSearchIndexManager DtSearchIndexManager { get; set; }
		public IInstanceSettingManager InstanceSettingManager { get; set; }

		[SetUp]
		public void SetUp()
		{
			ServiceFactory = new ServiceFactory(Constants.ServiceFactorySettings);
			RsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>();
			AgentManager = ServiceFactory.CreateProxy<Relativity.Services.Interfaces.Agent.IAgentManager>();
			ObjectManager = ServiceFactory.CreateProxy<IObjectManager>();
			ProductionManager = ServiceFactory.CreateProxy<IProductionManager>();
			ProductionDataSourceManager = ServiceFactory.CreateProxy<IProductionDataSourceManager>();
			KeywordSearchManager = ServiceFactory.CreateProxy<IKeywordSearchManager>();
			ProcessingCustodianManager = ServiceFactory.CreateProxy<IProcessingCustodianManager>();
			ProcessingSetManager = ServiceFactory.CreateProxy<IProcessingSetManager>();
			ProcessingDataSourceManager = ServiceFactory.CreateProxy<IProcessingDataSourceManager>();
			ResourcePoolManager = ServiceFactory.CreateProxy<IResourcePoolManager>();
			ProcessingJobManager = ServiceFactory.CreateProxy<IProcessingJobManager>();
			ImagingProfileManager = ServiceFactory.CreateProxy<IImagingProfileManager>();
			ImagingSetManager = ServiceFactory.CreateProxy<IImagingSetManager>();
			ImagingJobManager = ServiceFactory.CreateProxy<IImagingJobManager>();
			DtSearchManager = ServiceFactory.CreateProxy<IdtSearchManager>();
			DtSearchIndexManager = ServiceFactory.CreateProxy<IDtSearchIndexManager>();
			InstanceSettingManager = ServiceFactory.CreateProxy<IInstanceSettingManager>();
			IDBContext workspaceDbContext = new DbContext(Constants.ServerName, $"EDDS{Constants.WorkspaceArtifactId}", Constants.SqlLogin, Constants.SqlPassword);
			int documentIdentifierFieldArtifactId = SqlHelper.GetIdentifierFieldArtifactId(Constants.WorkspaceArtifactId);
			Sut = new SmokeTestCollection(
					rsapiClient: RsapiClient,
					agentManager: AgentManager,
					objectManager: ObjectManager,
					productionManager: ProductionManager,
					productionDataSourceManager: ProductionDataSourceManager,
					keywordSearchManager: KeywordSearchManager,
					imagingProfileManager: ImagingProfileManager,
					imagingSetManager: ImagingSetManager,
					imagingJobManager: ImagingJobManager,
					processingCustodianManager: ProcessingCustodianManager,
					processingSetManager: ProcessingSetManager,
					processingDataSourceManager: ProcessingDataSourceManager,
					resourcePoolManager: ResourcePoolManager,
					processingJobManager: ProcessingJobManager,
					dtSearchManager: DtSearchManager,
					dtSearchIndexManager: DtSearchIndexManager,
					workspaceDbContext: workspaceDbContext,
					workspaceArtifactId: Constants.WorkspaceArtifactId,
					documentIdentifierFieldArtifactId: documentIdentifierFieldArtifactId,
					relativityUrl: Constants.RelativityUrl,
					instanceSettingManager: InstanceSettingManager);
		}

		[TearDown]
		public void TearDown()
		{
			ServiceFactory = null;
			RsapiClient = null;
			AgentManager = null;
			ProductionManager = null;
			ProductionDataSourceManager = null;
			KeywordSearchManager = null;
			Sut = null;
		}

		[Test]
		public void FieldCreationAndDeletionTest()
		{
			ResultModel fieldResultModel = Sut.FieldTest();
			Assert.That(fieldResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void GroupCreationAndDeletionTest()
		{
			ResultModel groupResultModel = Sut.GroupTest();
			Assert.That(groupResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void UserCreationAndDeletionTest()
		{
			ResultModel userResultModel = Sut.UserTest();
			Assert.That(userResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void WorkspaceCreationAndDeletionTest()
		{
			ResultModel workspaceResultModel = Sut.WorkspaceTest();
			Assert.That(workspaceResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void AgentCreationAndDeletionTest()
		{
			ResultModel agentResultModel = Sut.AgentTest();
			Assert.That(agentResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void ImagingTest()
		{
			ResultModel imagingResultModel = Sut.ImageTest();
			Assert.That(imagingResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void ProductionCreationAndDeletionTest()
		{
			ResultModel productionResultModel = Sut.ProductionTest();
			Assert.That(productionResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void ProcessingTest()
		{
			ResultModel processingResultModel = Sut.ProcessingTest();
			Assert.That(processingResultModel.Success, Is.EqualTo(true));
		}

		[Test]
		public void DataGridTest()
		{
			ResultModel dataGridResultModel = Sut.DataGridTest();
			Assert.That(dataGridResultModel.Success, Is.EqualTo(true));
		}
	}
}
