using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using SmokeTest.Exceptions;
using SmokeTest.Helpers;
using SmokeTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SmokeTest.Agents
{
	[kCura.Agent.CustomAttributes.Name(Constants.Agents.SMOKE_TEST_ANALYSIS_AGENT_NAME)]
	[System.Runtime.InteropServices.Guid("37A4759D-C537-42A5-B77D-D516D19EA5DB")]
	public class SmokeTestAnalysisAgent : kCura.Agent.AgentBase
	{
		private IAPILog _logger;

		public override void Execute()
		{
			_logger = Helper.GetLoggerFactory().GetLogger();

			try
			{
				RaiseMessage("Running Smoke tests analysis.", 1);
				ExecutionIdentity systemExecutionIdentity = ExecutionIdentity.System;
				IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(systemExecutionIdentity);
				IDBContext eddsDbContext = Helper.GetDBContext(-1);
				List<int> workspaceArtifactIds = RetrieveAllApplicationWorkspaces(eddsDbContext, Constants.Guids.Application.SmokeTest);
				int workspaceCount = workspaceArtifactIds.Count;
				RaiseMessage($"Found '{workspaceCount}' applicable workspaces.", 1);

				if (workspaceCount > 0)
				{
					foreach (int currentWorkspaceArtifactId in workspaceArtifactIds)
					{
						if (currentWorkspaceArtifactId != -1)
						{
							try
							{
								RaiseMessage($"Running Smoke tests analysis in Workspace [{currentWorkspaceArtifactId}]", 1);
								IRdoHelper rdoHelper = new RdoHelper();

								int allTestsWithNewStatusCount = rdoHelper.GetTestRdoRecordsCountWithStatus(rsapiClient, currentWorkspaceArtifactId, Constants.Status.TestRdo.New);
								int allTestsWithRunningTestStatusCount = rdoHelper.GetTestRdoRecordsCountWithStatus(rsapiClient, currentWorkspaceArtifactId, Constants.Status.TestRdo.RunningTest);
								if (allTestsWithNewStatusCount > 0 || allTestsWithRunningTestStatusCount > 0)
								{
									// Some Smoke Tests still running
									RaiseMessage($"Skipped analysis. Smoke tests still have to run or already running in the Workspace [{currentWorkspaceArtifactId}]", 1);
								}
								else
								{
									// All Smoke Tests finished running
									List<RDO> allTestRdoRecords = rdoHelper.RetrieveAllTestRdos(rsapiClient, currentWorkspaceArtifactId);
									int testsTotalCount = allTestRdoRecords.Count;
									if (testsTotalCount <= 0)
									{
										RaiseMessage($"Skipped analysis. No Smoke tests in Workspace [{currentWorkspaceArtifactId}]", 1);
									}
									else
									{
										int testsSuccessCount = allTestRdoRecords.Count(x => x.Fields.Get(Constants.Guids.Fields.Test.Status).ToString() == Constants.Status.TestRdo.Success);
										int testsFailCount = allTestRdoRecords.Count(x => x.Fields.Get(Constants.Guids.Fields.Test.Status).ToString() == Constants.Status.TestRdo.Fail);

										RaiseMessage($"Finished running Smoke tests! [{nameof(currentWorkspaceArtifactId)}: {currentWorkspaceArtifactId}, {nameof(testsSuccessCount)}: {testsSuccessCount}, {nameof(testsFailCount)}: {testsFailCount}]", 1);
									}
								}
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Smoke Test Runner Agent");
							}
							finally
							{
								RaiseMessage($"Finished running Smoke tests analysis in Workspace [{currentWorkspaceArtifactId}]", 1);
								rsapiClient?.Dispose();
							}

						}
					}
				}
				else
				{
					RaiseMessage("Smoke Test application is not installed in any workspace.", 1);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when running the Smoke tests.", ex);
			}
			finally
			{
				RaiseMessage("Finished Smoke tests analysis.", 1);
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

		public override string Name => Constants.Agents.SMOKE_TEST_ANALYSIS_AGENT_NAME;
	}
}
