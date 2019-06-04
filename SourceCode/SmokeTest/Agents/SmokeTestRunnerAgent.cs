using kCura.Relativity.Client;
using Relativity.API;
using Relativity.DocumentViewer.Services;
using Relativity.Imaging.Services.Interfaces;
using Relativity.Processing.Services;
using Relativity.Productions.Services;
using Relativity.Services.Objects;
using Relativity.Services.ResourcePool;
using Relativity.Services.Search;
using SmokeTest.Exceptions;
using SmokeTest.Helpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace SmokeTest.Agents
{
	[kCura.Agent.CustomAttributes.Name(Constants.SmokeTestRunnerAgentName)]
	[System.Runtime.InteropServices.Guid("D41F2DB7-8803-4E4F-B63B-CC7DB3CE99C8")]
	public class SmokeTestRunnerAgent : kCura.Agent.AgentBase
	{
		private IAPILog _logger;

		public override void Execute()
		{
			_logger = Helper.GetLoggerFactory().GetLogger();
			try
			{
				ExecutionIdentity systemExecutionIdentity = ExecutionIdentity.System;
				IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(systemExecutionIdentity);
				Relativity.Services.Interfaces.Agent.IAgentManager agentManager = Helper.GetServicesManager().CreateProxy<Relativity.Services.Interfaces.Agent.IAgentManager>(systemExecutionIdentity);
				IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(systemExecutionIdentity);
				IProductionManager productionManager = Helper.GetServicesManager().CreateProxy<IProductionManager>(systemExecutionIdentity);
				IProductionDataSourceManager productionDataSourceManager = Helper.GetServicesManager().CreateProxy<IProductionDataSourceManager>(systemExecutionIdentity);
				IKeywordSearchManager keywordSearchManager = Helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(systemExecutionIdentity);
				IDocumentViewerServiceManager documentViewerServiceManager = Helper.GetServicesManager().CreateProxy<IDocumentViewerServiceManager>(systemExecutionIdentity);
				IProcessingCustodianManager processingCustodianManager = Helper.GetServicesManager().CreateProxy<IProcessingCustodianManager>(systemExecutionIdentity);
				IProcessingSetManager processingSetManager = Helper.GetServicesManager().CreateProxy<IProcessingSetManager>(systemExecutionIdentity);
				IProcessingDataSourceManager processingDataSourceManager = Helper.GetServicesManager().CreateProxy<IProcessingDataSourceManager>(systemExecutionIdentity);
				IImagingProfileManager imagingProfileManager = Helper.GetServicesManager().CreateProxy<IImagingProfileManager>(systemExecutionIdentity);
				IImagingSetManager imagingSetManager = Helper.GetServicesManager().CreateProxy<IImagingSetManager>(systemExecutionIdentity);
				IImagingJobManager imagingJobManager = Helper.GetServicesManager().CreateProxy<IImagingJobManager>(systemExecutionIdentity);
				IResourcePoolManager resourcePoolManager = Helper.GetServicesManager().CreateProxy<IResourcePoolManager>(systemExecutionIdentity);
				IProcessingJobManager processingJobManager = Helper.GetServicesManager().CreateProxy<IProcessingJobManager>(systemExecutionIdentity);
				IDBContext eddsDbContext = Helper.GetDBContext(-1);
				List<int> workspaceArtifactIds = RetrieveAllApplicationWorkspaces(eddsDbContext, Constants.Guids.Application.SmokeTest);

				foreach (int currentWorkspaceArtifactId in workspaceArtifactIds)
				{
					if (currentWorkspaceArtifactId != -1)
					{
						try
						{
							RaiseMessage($"Running Smoke tests in Workspace [{currentWorkspaceArtifactId}]", 1);
							IDBContext workspaceDbContext = Helper.GetDBContext(currentWorkspaceArtifactId);
							int documentIdentifierFieldArtifactId = SqlHelper.GetIdentifierFieldArtifactId(workspaceDbContext, currentWorkspaceArtifactId);
							SmokeTestCollection smokeTestCollection = new SmokeTestCollection(
									rsapiClient: rsapiClient,
									agentManager: agentManager,
									objectManager: objectManager,
									productionManager: productionManager,
									productionDataSourceManager: productionDataSourceManager,
									keywordSearchManager: keywordSearchManager,
									documentViewerServiceManager: documentViewerServiceManager,
									imagingProfileManager: imagingProfileManager,
									imagingSetManager: imagingSetManager,
									imagingJobManager: imagingJobManager,
									processingCustodianManager: processingCustodianManager,
									processingSetManager: processingSetManager,
									processingDataSourceManager: processingDataSourceManager,
									resourcePoolManager: resourcePoolManager,
									processingJobManager: processingJobManager,
									workspaceDbContext: workspaceDbContext,
									workspaceArtifactId: currentWorkspaceArtifactId,
									documentIdentifierFieldArtifactId: documentIdentifierFieldArtifactId);
							smokeTestCollection.Run();
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Smoke Test Runner Agent");
						}
						finally
						{
							RaiseMessage($"Finished running Smoke tests in Workspace [{currentWorkspaceArtifactId}]", 1);
							rsapiClient?.Dispose();
							agentManager?.Dispose();
							productionManager?.Dispose();
							productionDataSourceManager?.Dispose();
							keywordSearchManager?.Dispose();
							documentViewerServiceManager?.Dispose();
							processingCustodianManager?.Dispose();
							processingSetManager?.Dispose();
							processingDataSourceManager?.Dispose();
							resourcePoolManager?.Dispose();
							processingJobManager?.Dispose();
							imagingProfileManager?.Dispose();
							imagingSetManager?.Dispose();
							imagingJobManager?.Dispose();
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when running the Smoke tests.", ex);
			}
		}

		private List<int> RetrieveAllApplicationWorkspaces(IDBContext eddsDbContext, Guid applicationGuid)
		{
			List<int> workspaceArtifactIds = new List<int>();

			try
			{
				DataTable dataTable = SqlHelper.RetrieveApplicationWorkspaces(eddsDbContext, applicationGuid);

				foreach (DataRow dataRow in dataTable.Rows)
				{
					int currentWorkspaceArtifactId = (int)dataRow["ArtifactID"];
					if (currentWorkspaceArtifactId > 0)
					{
						workspaceArtifactIds.Add(currentWorkspaceArtifactId);
					}
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error when querying for workspaces where Smoke Test application is installed.", ex);
			}

			return workspaceArtifactIds;
		}

		public override string Name => Constants.SmokeTestRunnerAgentName;
	}
}
