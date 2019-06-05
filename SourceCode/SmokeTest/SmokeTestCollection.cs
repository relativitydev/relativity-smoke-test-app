using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.DocumentViewer.Services;
using Relativity.Imaging.Services.Interfaces;
using Relativity.Processing.Services;
using Relativity.Productions.Services;
using Relativity.Productions.Services.Interfaces.DTOs;
using Relativity.Services.Objects;
using Relativity.Services.ResourcePool;
using Relativity.Services.Search;
using SmokeTest.Exceptions;
using SmokeTest.Helpers;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Relativity.Services.Interfaces.DtSearchIndexManager;
using IAgentHelper = SmokeTest.Interfaces.IAgentHelper;

namespace SmokeTest
{
	public class SmokeTestCollection
	{
		public List<SmokeTestModel> SmokeTests = new List<SmokeTestModel>();
		public IRSAPIClient RsapiClient { get; set; }
		public Relativity.Services.Interfaces.Agent.IAgentManager AgentManager { get; set; }
		public IObjectManager ObjectManager { get; set; }
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
		public IDBContext WorkspaceDbContext { get; set; }
		public int WorkspaceArtifactId { get; set; }
		public int DocumentIdentifierFieldArtifactId { get; set; }
		public IRdoHelper RdoHelper { get; set; }
		public IdtSearchManager DtSearchManager { get; set; }
		public IDtSearchIndexManager DtSearchIndexManager { get; set; }
		public string RelativityUrl { get; set; }


		public SmokeTestCollection(IRSAPIClient rsapiClient, Relativity.Services.Interfaces.Agent.IAgentManager agentManager, IObjectManager objectManager, IProductionManager productionManager,
				IProductionDataSourceManager productionDataSourceManager, IProcessingCustodianManager processingCustodianManager, IProcessingSetManager processingSetManager, IProcessingDataSourceManager processingDataSourceManager, IResourcePoolManager resourcePoolManager, IProcessingJobManager processingJobManager,
				IKeywordSearchManager keywordSearchManager, IDocumentViewerServiceManager documentViewerServiceManager, IImagingProfileManager imagingProfileManager, IImagingSetManager imagingSetManager, IImagingJobManager imagingJobManager, IDBContext workspaceDbContext, IdtSearchManager dtSearchManager, IDtSearchIndexManager dtSearchIndexManager, int workspaceArtifactId, int documentIdentifierFieldArtifactId, string relativityUrl)
		{
			RsapiClient = rsapiClient;
			AgentManager = agentManager;
			ObjectManager = objectManager;
			ProductionManager = productionManager;
			ProductionDataSourceManager = productionDataSourceManager;
			KeywordSearchManager = keywordSearchManager;
			DocumentViewerServiceManager = documentViewerServiceManager;
			ProcessingCustodianManager = processingCustodianManager;
			ProcessingDataSourceManager = processingDataSourceManager;
			ProcessingSetManager = processingSetManager;
			ImagingProfileManager = imagingProfileManager;
			ImagingSetManager = imagingSetManager;
			ImagingJobManager = imagingJobManager;
			ResourcePoolManager = resourcePoolManager;
			ProcessingJobManager = processingJobManager;
			WorkspaceDbContext = workspaceDbContext;
			DtSearchManager = dtSearchManager;
			DtSearchIndexManager = dtSearchIndexManager;
			WorkspaceArtifactId = workspaceArtifactId;
			DocumentIdentifierFieldArtifactId = documentIdentifierFieldArtifactId;
			RdoHelper = new RdoHelper();
			RelativityUrl = relativityUrl;
		}

		public void Run()
		{
			Initialize();
			CreateAllTests();
			RunAllTests();
		}

		private void Initialize()
		{
			// Order of tests is import because documents have to be imaged first before running conversion and production tests.
			SmokeTests.Add(new SmokeTestModel(10, "Field Test", FieldTest));
			SmokeTests.Add(new SmokeTestModel(20, "Group Test", GroupTest));
			SmokeTests.Add(new SmokeTestModel(30, "User Test", UserTest));
			SmokeTests.Add(new SmokeTestModel(40, "Workspace Test", WorkspaceTest));
			SmokeTests.Add(new SmokeTestModel(50, "Agent Test", AgentTest));
			SmokeTests.Add(new SmokeTestModel(70, "Image Test", ImageTest));
			SmokeTests.Add(new SmokeTestModel(80, "Conversion Test", ConversionTest));
			SmokeTests.Add(new SmokeTestModel(60, "Production Test", ProductionTest));
			SmokeTests.Add(new SmokeTestModel(90, "Processing Test", ProcessingTest));
			SmokeTests.Add(new SmokeTestModel(100, "Data Grid Test", DataGridTest));
		}

		private void CreateAllTests()
		{
			// Order of tests is import because documents have to be imaged first before running conversion and production tests.
			foreach (SmokeTestModel smokeTestModel in SmokeTests.OrderBy(x => x.Order))
			{
				CreateTestIfItNotAlreadyExists(smokeTestModel);
			}
		}

		private void CreateTestIfItNotAlreadyExists(SmokeTestModel smokeTestModel)
		{
			string errorContext = "An error occured when checking for existing test and creating a new test if it not exists.";
			try
			{
				bool doesTestExists = RdoHelper.CheckIfTestRdoRecordExists(RsapiClient, WorkspaceArtifactId, smokeTestModel.Name);
				if (!doesTestExists)
				{
					// Only Create a Test if it is not already exists.
					RdoHelper.CreateTestRdoRecord(RsapiClient, WorkspaceArtifactId, smokeTestModel.Name, Constants.Status.TestRdo.New, string.Empty, string.Empty);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [TestName: {smokeTestModel.Name}]", ex);
			}
		}

		private void RunAllTests()
		{
			// Order of tests is import because documents have to be imaged first before running conversion and production tests.
			foreach (SmokeTestModel smokeTestModel in SmokeTests.OrderBy(x => x.Order))
			{
				RunTestOnlyIfStatusIsNew(smokeTestModel);
			}
		}

		private void RunTestOnlyIfStatusIsNew(SmokeTestModel smokeTestModel)
		{
			string errorContext = "An error occured when checking for existing test and running a new test if it not exists.";
			try
			{
				RDO testRdo = RdoHelper.RetrieveTestRdo(RsapiClient, WorkspaceArtifactId, smokeTestModel.Name);
				string testStatus = testRdo.Fields.Get(Constants.Guids.Fields.Test.Status).ToString();
				// Only Run a Test if its Status is New
				if (testStatus.Equals(Constants.Status.TestRdo.New))
				{
					RunTest(smokeTestModel, testRdo);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [TestName: {smokeTestModel.Name}]", ex);
			}
		}

		private void RunTest(SmokeTestModel smokeTestModel, RDO testRdo)
		{
			const int maxRetry = 3;
			try
			{
				int retryCount = 0;
				string stackTrace = string.Empty;

				RdoHelper.UpdateTestRdoRecord(RsapiClient, WorkspaceArtifactId, testRdo.ArtifactID, null, Constants.Status.TestRdo.RunningTest, null, null);
				ResultModel resultModel = smokeTestModel.Method();
				while (resultModel.Success == false && retryCount < maxRetry)
				{
					try
					{
						Thread.Sleep(5000);
						resultModel = smokeTestModel.Method();
					}
					catch (Exception ex)
					{
						stackTrace += ex.ToString();
					}

					retryCount++;
				}
				if (!resultModel.Success)
				{
					throw new SmokeTestException($"An error occured in {smokeTestModel.Name}. ErrorMessage: {resultModel.ErrorMessage}. Exception: {stackTrace}");
				}
				RdoHelper.UpdateTestRdoRecord(RsapiClient, WorkspaceArtifactId, testRdo.ArtifactID, null, Constants.Status.TestRdo.Success, null, null);
			}
			catch (Exception ex)
			{
				string error = ExceptionMessageFormatter.GetInnerMostExceptionMessage(ex);
				string errorDetails = ex.ToString();
				RdoHelper.UpdateTestRdoRecord(RsapiClient, WorkspaceArtifactId, testRdo.ArtifactID, null, Constants.Status.TestRdo.Fail, error, errorDetails);
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

			ResultModel agentResultModel = agentHelper.CreateAgent(
					agentManager: AgentManager,
					objectManager: ObjectManager,
					applicationName: Constants.Agents.SMOKE_TEST_APPLICATION_NAME,
					agentName: Constants.Agents.TEST_AGENT_TO_CREATE_NAME);
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
			ResultModel workspaceResultModel = workspaceHelper.QueryTemplateAndCreateWorkspace(RsapiClient, Constants.WORKSPACE_TEMPLATE_NAME, workspaceName);
			if (workspaceResultModel.Success)
			{
				workspaceHelper.DeleteWorkspace(RsapiClient, workspaceResultModel.ArtifactId);
			}
			return workspaceResultModel;
		}

		public ResultModel ImageTest()
		{
			ResultModel imageResultModel;
			try
			{
				IImageHelper imageHelper = new ImageHelper(RsapiClient, ImagingProfileManager, ImagingSetManager, ImagingJobManager);
				if (!SavedSearchHelper.DocumentsExistInWorkspace(RsapiClient, WorkspaceArtifactId))
				{
					imageResultModel = new ResultModel("Image")
					{
						Success = false,
						ErrorMessage = "There are no documents in the workspace."
					};
				}
				else
				{
					int savedSearchArtifactId = SavedSearchHelper.CreateSavedSearchWithControlNumbers(
						keywordSearchManager: KeywordSearchManager,
						rsapiClient: RsapiClient,
						workspaceArtifactId: WorkspaceArtifactId,
						documentIdentifierFieldArtifactId: DocumentIdentifierFieldArtifactId,
						searchName: Constants.AllDocumentsSavedSearchName,
						controlNumbers: new List<string>());

					imageResultModel = imageHelper.ImageDocuments(WorkspaceArtifactId);

					if (imageResultModel.Success)
					{
						bool savedSearchDeleteResult = SavedSearchHelper.DeleteKeywordSearch(KeywordSearchManager, savedSearchArtifactId, WorkspaceArtifactId).Result;
					}
				}
			}
			catch (Exception ex)
			{
				imageResultModel = new ResultModel("Image")
				{
					Success = false,
					ErrorMessage = $@"Error running Text: {ex}"
				};
			}

			return imageResultModel;
		}

		public ResultModel ConversionTest()
		{
			ResultModel imageResultModel;
			try
			{
				IViewerHelper viewerHelper = new ViewerHelper(RsapiClient, DocumentViewerServiceManager, WorkspaceDbContext);
				RsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				if (!SavedSearchHelper.DocumentsExistInWorkspace(RsapiClient, WorkspaceArtifactId))
				{
					imageResultModel = new ResultModel("Viewer")
					{
						Success = false,
						ErrorMessage = "There are no documents in the workspace."
					};
				}
				else
				{
					int savedSearchArtifactId = SavedSearchHelper.CreateSavedSearchWithControlNumbers(
						keywordSearchManager: KeywordSearchManager,
						rsapiClient: RsapiClient,
						workspaceArtifactId: WorkspaceArtifactId,
						documentIdentifierFieldArtifactId: DocumentIdentifierFieldArtifactId,
						searchName: Constants.AllDocumentsSavedSearchName,
						controlNumbers: new List<string>());

					imageResultModel = viewerHelper.ConvertDocumentsForViewer(WorkspaceArtifactId);

					// Delete saved search
				}
			}
			catch (Exception ex)
			{
				imageResultModel = new ResultModel("Viewer")
				{
					Success = false,
					ErrorMessage = $@"Error running Text: {ex}"
				};
			}

			return imageResultModel;
		}

		public ResultModel ProductionTest()
		{
			ResultModel productionResultModel = new ResultModel("Production");
			try
			{
				IProductionHelper productionHelper = new ProductionHelper();
				RsapiClient.APIOptions.WorkspaceID = WorkspaceArtifactId;
				int savedSearchArtifactId = SavedSearchHelper.CreateSavedSearchWithControlNumbers(
						keywordSearchManager: KeywordSearchManager,
						rsapiClient: RsapiClient,
						workspaceArtifactId: WorkspaceArtifactId,
						documentIdentifierFieldArtifactId: DocumentIdentifierFieldArtifactId,
						searchName: $"ST-{Guid.NewGuid()}",
						controlNumbers: new List<string>());

				if (!SavedSearchHelper.DocumentsExistInWorkspace(RsapiClient, WorkspaceArtifactId))
				{
					productionResultModel.Success = false;
					productionResultModel.ErrorMessage = "There are no documents in the workspace.";
				}
				else
				{
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
					ProductionStatusDetailsResult productionStatusDetailsResult = ProductionManager.GetProductionStatusDetails(WorkspaceArtifactId, productionSetArtifactId).Result;
					string productionStatus = (string) productionStatusDetailsResult.StatusDetails.FirstOrDefault().Value;

					if (productionSetArtifactId > 0)
					{
						productionHelper.DeleteProductionSet(ProductionManager, WorkspaceArtifactId, productionSetArtifactId);
					}

					if (productionStatus == "Produced")
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
			}
			catch (Exception ex)
			{
				productionResultModel.Success = false;
				productionResultModel.ErrorMessage = ex.ToString();
			}
			return productionResultModel;
		}

		public ResultModel ProcessingTest()
		{
			ProcessingHelper processingHelper = new ProcessingHelper(RsapiClient, ProcessingCustodianManager, ProcessingSetManager, ProcessingDataSourceManager, ResourcePoolManager, ProcessingJobManager);
			return processingHelper.CreateAndRunProcessingSet(WorkspaceArtifactId);
		}

		public ResultModel DataGridTest()
		{
			DataGridHelper dataGridHelper = new DataGridHelper(RsapiClient, KeywordSearchManager, DtSearchManager, DtSearchIndexManager, RelativityUrl);
			return dataGridHelper.VerifyDataGridFunctionality(WorkspaceArtifactId);
		}
	}
}
