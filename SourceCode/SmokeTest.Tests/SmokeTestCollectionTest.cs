using kCura.Data.RowDataGateway;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using Relativity.DocumentViewer.Services;
using Relativity.Processing.Services;
using Relativity.Imaging.Services.Interfaces;
using Relativity.Productions.Services;
using Relativity.Services.Agent;
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
        public IAgentManager AgentManager { get; set; }
        public IProductionManager ProductionManager { get; set; }
        public IProductionDataSourceManager ProductionDataSourceManager { get; set; }
        public IKeywordSearchManager KeywordSearchManager { get; set; }
        public IDocumentViewerServiceManager DocumentViewerServiceManager { get; set; }
        public IImagingProfileManager ImagingProfileManager { get; set; }
        public IImagingSetManager ImagingSetManager { get; set; }
        public IImagingJobManager ImagingJobManager { get; set; }
        public IProcessingCustodianManager ProcessingCustodianManager { get; set; }
        public IProcessingSetManager ProcessingSetManager { get; set; }
        public IProcessingDataSourceManager ProcessingDataSourceManager { get; set; }
        public IResourcePoolManager ResourcePoolManager { get; set; }
        public IProcessingJobManager ProcessingJobManager { get; set; }

        [SetUp]
        public void SetUp()
        {
            ServiceFactory = new ServiceFactory(Constants.ServiceFactorySettings);
            RsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>();
            AgentManager = ServiceFactory.CreateProxy<IAgentManager>();
            ProductionManager = ServiceFactory.CreateProxy<IProductionManager>();
            ProductionDataSourceManager = ServiceFactory.CreateProxy<IProductionDataSourceManager>();
            KeywordSearchManager = ServiceFactory.CreateProxy<IKeywordSearchManager>();
            ProcessingCustodianManager = ServiceFactory.CreateProxy<IProcessingCustodianManager>();
            DocumentViewerServiceManager = ServiceFactory.CreateProxy<IDocumentViewerServiceManager>();
            ProcessingSetManager = ServiceFactory.CreateProxy<IProcessingSetManager>();
            ProcessingDataSourceManager = ServiceFactory.CreateProxy<IProcessingDataSourceManager>();
            ResourcePoolManager = ServiceFactory.CreateProxy<IResourcePoolManager>();
            ProcessingJobManager = ServiceFactory.CreateProxy<IProcessingJobManager>();
            IDBContext workspaceDbContext = new DBContext(new Context("serverName", "databaseName", "sqlServerUsername", "sqlServerPassword"));

            int documentIdentifierFieldArtifactId = SqlHelper.GetIdentifierFieldArtifactId(Constants.WorkspaceArtifactId);
            Sut = new SmokeTestCollection(
                rsapiClient: RsapiClient,
                agentManager: AgentManager,
                productionManager: ProductionManager,
                productionDataSourceManager: ProductionDataSourceManager,
                keywordSearchManager: KeywordSearchManager,
                documentViewerServiceManager: DocumentViewerServiceManager,
                imagingProfileManager: ImagingProfileManager,
                imagingSetManager: ImagingSetManager,
                imagingJobManager: ImagingJobManager,
                processingCustodianManager: ProcessingCustodianManager,
                processingSetManager: ProcessingSetManager,
                processingDataSourceManager: ProcessingDataSourceManager,
                resourcePoolManager: ResourcePoolManager,
                processingJobManager: ProcessingJobManager,
                workspaceDbContext: workspaceDbContext,
                workspaceArtifactId: Constants.WorkspaceArtifactId,
                documentIdentifierFieldArtifactId: documentIdentifierFieldArtifactId);
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
        public void ProductionCreationAndDeletionTest()
        {
            ResultModel productionResultModel = Sut.ProductionTest();
            Assert.That(productionResultModel.Success, Is.EqualTo(true));
        }
    }
}
