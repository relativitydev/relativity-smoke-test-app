using kCura.Relativity.Client;
using Relativity.API;
using Relativity.DocumentViewer.Services;
using Relativity.Imaging.Services.Interfaces;
using Relativity.Productions.Services;
using Relativity.Services.Agent;
using Relativity.Services.Search;
using SmokeTest.Exceptions;
using SmokeTest.Helpers;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using IAgentHelper = SmokeTest.Interfaces.IAgentHelper;

namespace SmokeTest
{
    public class SmokeTestCollection
    {
        public IRSAPIClient RsapiClient { get; set; }
        public IAgentManager AgentManager { get; set; }
        public IProductionManager ProductionManager { get; set; }
        public IProductionDataSourceManager ProductionDataSourceManager { get; set; }
        public IKeywordSearchManager KeywordSearchManager { get; set; }
        public IDocumentViewerServiceManager DocumentViewerServiceManager { get; set; }
        public IImagingProfileManager ImagingProfileManager { get; set; }
        public IImagingSetManager ImagingSetManager { get; set; }
        public IImagingJobManager ImagingJobManager { get; set; }
        public IDBContext WorkspaceDbContext { get; set; }
        public int WorkspaceArtifactId { get; set; }
        public int DocumentIdentifierFieldArtifactId { get; set; }

        public SmokeTestCollection(IRSAPIClient rsapiClient, IAgentManager agentManager, IProductionManager productionManager,
        IProductionDataSourceManager productionDataSourceManager,
        IKeywordSearchManager keywordSearchManager, IDocumentViewerServiceManager documentViewerServiceManager, IImagingProfileManager imagingProfileManager, IImagingSetManager imagingSetManager, IImagingJobManager imagingJobManager, IDBContext workspaceDbContext, int workspaceArtifactId, int documentIdentifierFieldArtifactId)
        {
            RsapiClient = rsapiClient;
            AgentManager = agentManager;
            ProductionManager = productionManager;
            ProductionDataSourceManager = productionDataSourceManager;
            KeywordSearchManager = keywordSearchManager;
            DocumentViewerServiceManager = documentViewerServiceManager; ;
            ImagingProfileManager = imagingProfileManager;
            ImagingSetManager = imagingSetManager;
            ImagingJobManager = imagingJobManager;
            WorkspaceDbContext = workspaceDbContext;
            WorkspaceArtifactId = workspaceArtifactId;
            DocumentIdentifierFieldArtifactId = documentIdentifierFieldArtifactId;
        }

        public void RunAllTests()
        {
            // Order of tests is import because documents have to be imaged first before running conversion and production tests.
            IRdoHelper rdoHelper = new RdoHelper();
            CheckAndRunTest("FieldTest", rdoHelper, FieldTest);
            CheckAndRunTest("GroupTest", rdoHelper, GroupTest);
            CheckAndRunTest("UserTest", rdoHelper, UserTest);
            CheckAndRunTest("AgentTest", rdoHelper, AgentTest);
            CheckAndRunTest("WorkspaceTest", rdoHelper, WorkspaceTest);
            CheckAndRunTest("ImageTest", rdoHelper, ImageTest);
            CheckAndRunTest("ConversionTest", rdoHelper, ConversionTest);
            CheckAndRunTest("ProductionTest", rdoHelper, ProductionTest);
        }

        private void CheckAndRunTest(string testName, IRdoHelper rdoHelper, Func<ResultModel> testMethodName)
        {
            string errorContext = "An error occured when checking for existing test and running a new test if it not exists.";
            try
            {
                bool doesTestExists = rdoHelper.CheckIfTestsRdoRecordExists(RsapiClient, WorkspaceArtifactId, testName);
                if (!doesTestExists)
                {
                    // Only Run a Test if it is not already run.
                    RunTest(testName, rdoHelper, testMethodName);
                }
            }
            catch (Exception ex)
            {
                throw new SmokeTestException($"{errorContext}. [TestName: {testName}]", ex);
            }
        }

        private void RunTest(string testName, IRdoHelper rdoHelper, Func<ResultModel> testMethodName)
        {
            try
            {
                ResultModel resultModel = testMethodName();
                if (!resultModel.Success)
                {
                    throw new SmokeTestException($"An error occured in {testName}. ErrorMessage: {resultModel.ErrorMessage}");
                }
                rdoHelper.CreateTestsRdoRecord(RsapiClient, WorkspaceArtifactId, testName, Constants.TestResultsStatus.Success, string.Empty);
            }
            catch (Exception ex)
            {
                rdoHelper.CreateTestsRdoRecord(RsapiClient, WorkspaceArtifactId, testName, Constants.TestResultsStatus.Fail, ex.ToString());
            }
        }

        public ResultModel FieldTest()
        {
            IFieldHelper fieldHelper = new FieldHelper();
            string fieldName = $"{Constants.Prefix}-{Guid.NewGuid()}";
            ResultModel fieldResultModel = fieldHelper.CreateSingleChoiceDocumentField(RsapiClient, WorkspaceArtifactId, fieldName);
            if (fieldResultModel.Success)
            {
                fieldHelper.DeleteSingleChoiceDocumentField(RsapiClient, WorkspaceArtifactId, fieldResultModel.ArtifactId);
            }
            return fieldResultModel;
        }

        public ResultModel GroupTest()
        {
            IGroupHelper groupHelper = new GroupHelper();
            string groupName = $"{Constants.Prefix}-{Guid.NewGuid()}";
            ResultModel groupResultModel = groupHelper.CreateGroup(RsapiClient, groupName);
            if (groupResultModel.Success)
            {
                groupHelper.DeleteGroup(RsapiClient, groupResultModel.ArtifactId);
            }
            return groupResultModel;
        }

        public ResultModel UserTest()
        {
            IUserHelper userHelper = new UserHelper();
            string firstName = $"{Constants.Prefix}-FN-{Guid.NewGuid()}";
            string lastName = $"{Constants.Prefix}-LN-{Guid.NewGuid()}";
            string emailAddress = $"{Constants.Prefix}-EA-{Guid.NewGuid()}@test.com";
            ResultModel userResultModel = userHelper.CreateUser(RsapiClient, firstName, lastName, emailAddress);
            if (userResultModel.Success)
            {
                userHelper.DeleteUser(RsapiClient, userResultModel.ArtifactId);
            }
            return userResultModel;
        }

        public ResultModel AgentTest()
        {
            IAgentHelper agentHelper = new AgentHelper();
            string agentName = $"{Constants.Prefix}-{Guid.NewGuid()}";
            int agentTypeId = agentHelper.GetAgentTypeArtifactId(AgentManager, Constants.TestAgentToCreateName);
            int agentServer = agentHelper.GetFirstAgentServerArtifactId(AgentManager);
            ResultModel agentResultModel = agentHelper.CreateAgent(
                agentManager: AgentManager,
                agentName: agentName,
                agentTypeId: agentTypeId,
                agentServer: agentServer,
                enableAgent: true,
                agentInterval: 5,
                agentLoggingLevel: Agent.LoggingLevelEnum.All);
            if (agentResultModel.Success)
            {
                agentHelper.DeleteAgent(AgentManager, agentResultModel.ArtifactId);
            }
            return agentResultModel;
        }

        public ResultModel WorkspaceTest()
        {
            IWorkspaceHelper workspaceHelper = new WorkspaceHelper();
            string workspaceName = $"{Constants.Prefix}-{Guid.NewGuid()}";
            ResultModel workspaceResultModel = workspaceHelper.CreateWorkspace(RsapiClient, workspaceName);
            if (workspaceResultModel.Success)
            {
                workspaceHelper.DeleteWorkspace(RsapiClient, workspaceResultModel.ArtifactId);
            }
            return workspaceResultModel;
        }

        public ResultModel ImageTest()
        {
            IImageHelper imageHelper = new ImageHelper();
            ResultModel imageResultModel = imageHelper.ImageDocuments(RsapiClient, ImagingProfileManager, ImagingSetManager, ImagingJobManager, WorkspaceArtifactId);
            return imageResultModel;
        }

        public ResultModel ConversionTest()
        {
            IViewerHelper viewerHelper = new ViewerHelper();
            ResultModel imageResultModel = viewerHelper.ConvertDocumentsForViewer(RsapiClient, DocumentViewerServiceManager, WorkspaceDbContext, WorkspaceArtifactId);
            return imageResultModel;
        }

        public ResultModel ProductionTest()
        {
            ResultModel productionResultModel = new ResultModel("Production");
            try
            {
                IProductionHelper productionHelper = new ProductionHelper();
                int savedSearchArtifactId = SavedSearchHelper.CreateSavedSearchWithControlNumbers(
                    keywordSearchManager: KeywordSearchManager,
                    rsapiClient: RsapiClient,
                    workspaceArtifactId: WorkspaceArtifactId,
                    documentIdentifierFieldArtifactId: DocumentIdentifierFieldArtifactId,
                    searchName: $"ST-{Guid.NewGuid()}",
                    controlNumbers: new List<string>());

                ProductionModel productionModel = new ProductionModel(
                    workspaceArtifactId: WorkspaceArtifactId,
                    productionName: $"Page Level - Simple - {Guid.NewGuid()}",
                    attachmentRelationalFieldArtifactId: Constants.GroupIdentifierFieldArtifactId,
                    batesPrefix: "ABC",
                    batesSuffix: "XYZ",
                    batesStartNumber: 1,
                    numberOfDigitsForDocumentNumbering: 7,
                    brandingFontSize: 10,
                    scaleBrandingFont: false,
                    emailRecipients: string.Empty,
                    markupSetModel: new MarkupSetModel(
                        name: $"Generic MarkupSet{DateTime.UtcNow}{DateTime.UtcNow.Millisecond}",
                        markupSetOrder: 10,
                        redactionText: "who cares"),
                    productionType: ProductionType.ImagesOnly,
                    productionDataSourceName: "Test Datasource 123",
                    savedSearchArtifactId: savedSearchArtifactId,
                    clientModel: new ClientModel(
                        rsapiClient: RsapiClient,
                        productionManager: ProductionManager,
                        productionDataSourceManager: ProductionDataSourceManager),
                    stagingAndProductionWaitTimeOutInSeconds: 300);
                int productionSetArtifactId = productionHelper.CreateAndRunProductionSet(productionModel);
                Production production = ProductionManager.ReadSingleAsync(WorkspaceArtifactId, productionSetArtifactId).Result;
                ProductionStatus productionStatus = production.ProductionMetadata.Status;
                int productionArtifactId = production.ArtifactID;

                if (productionArtifactId > 0)
                {
                    productionHelper.DeleteProductionSet(ProductionManager, WorkspaceArtifactId, productionArtifactId);
                }

                if (productionStatus == ProductionStatus.Produced)
                {
                    productionResultModel.Success = true;
                    productionResultModel.ArtifactId = productionSetArtifactId;
                    productionResultModel.ErrorMessage = string.Empty;
                }
                else
                {
                    throw new SmokeTestException("An error occured when creating a new production set and running it.");
                }
            }
            catch (Exception ex)
            {
                productionResultModel.Success = false;
                productionResultModel.ErrorMessage = ex.ToString();
            }
            return productionResultModel;
        }
    }
}
