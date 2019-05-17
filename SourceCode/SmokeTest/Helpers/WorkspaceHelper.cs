using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmokeTest.Helpers
{
	public class WorkspaceHelper : IWorkspaceHelper
	{
		private static readonly TaskCompletionSource<ProcessInformation> TaskCompletionSource = new TaskCompletionSource<ProcessInformation>();

		public ResultModel CreateWorkspace(IRSAPIClient rsapiClient, string workspaceName)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (workspaceName == null)
			{
				throw new ArgumentNullException(nameof(workspaceName));
			}

			ResultModel resultModel = new ResultModel("Workspace");
			rsapiClient.APIOptions.WorkspaceID = -1;

			try
			{
				Workspace workspaceDto = new Workspace
				{
					Name = workspaceName
				};

				try
				{
					int templateId = 1015024;
					ProcessOperationResult processOperationResult = rsapiClient.Repositories.Workspace.CreateAsync(templateId, workspaceDto);

					if (processOperationResult.Success)
					{
						Task<ProcessInformation> task = MonitorProcessStateAsync(rsapiClient, processOperationResult.ProcessID);
						ProcessInformation processInfo = (ProcessInformation)task.Result;
						DisconnectMonitorProcessStateAsync(rsapiClient);

						if (processInfo.State == ProcessStateValue.Completed)
						{
							int? firstOrDefault = processInfo.OperationArtifactIDs.FirstOrDefault();
							if (firstOrDefault != null)
							{
								int newWorkspaceArtifactId = firstOrDefault.Value;
								resultModel.Success = true;
								resultModel.ArtifactId = newWorkspaceArtifactId;
							}
						}
						else
						{
							throw new SmokeTestException($"An error occured when creating workspace. [{nameof(workspaceName)} = {workspaceName}, State: {processInfo.State.ToString()}, Status: {processInfo.Status}, Exception Message: {processInfo.Message}]");
						}
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when creating workspace. [{nameof(workspaceName)} = {workspaceName}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		private static Task<ProcessInformation> MonitorProcessStateAsync(IRSAPIClient rsapiClient, Guid processId)
		{
			rsapiClient.ProcessComplete += HandleProcessCompleteEvent;
			rsapiClient.ProcessProgress += HandleProcessProgressEvent;
			rsapiClient.ProcessCompleteWithError += HandleProcessCompleteWithErrorEvent;
			rsapiClient.ProcessFailure += HandleProcessFailureEvent;
			rsapiClient.MonitorProcessState(rsapiClient.APIOptions, processId);
			return TaskCompletionSource.Task;
		}

		private static void DisconnectMonitorProcessStateAsync(IRSAPIClient rsapiClient)
		{
			rsapiClient.ProcessComplete -= HandleProcessCompleteEvent;
			rsapiClient.ProcessProgress -= HandleProcessProgressEvent;
			rsapiClient.ProcessCompleteWithError -= HandleProcessCompleteWithErrorEvent;
			rsapiClient.ProcessFailure -= HandleProcessFailureEvent;
		}

		private static void HandleProcessProgressEvent(object sender, ProcessProgressEventArgs eventArgs)
		{
			ProcessInformation info = eventArgs.ProcessInformation;
			Console.WriteLine("Completed {0} of {1} Operations", info.OperationsCompleted, info.TotalOperations);
		}

		private static void HandleProcessCompleteEvent(object sender, ProcessCompleteEventArgs eventArgs)
		{
			TaskCompletionSource.SetResult(eventArgs.ProcessInformation);
		}

		private static void HandleProcessCompleteWithErrorEvent(object sender, ProcessCompleteWithErrorEventArgs eventArgs)
		{
			TaskCompletionSource.SetResult(eventArgs.ProcessInformation);
		}

		private static void HandleProcessFailureEvent(object sender, ProcessFailureEventArgs eventArgs)
		{
			TaskCompletionSource.SetResult(eventArgs.ProcessInformation);
		}

		public ResultModel DeleteWorkspace(IRSAPIClient rsapiClient, int workspaceArtifactId)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
			}

			ResultModel resultModel = new ResultModel("Workspace");
			rsapiClient.APIOptions.WorkspaceID = -1;

			try
			{
				try
				{
					rsapiClient.Repositories.Workspace.DeleteSingle(workspaceArtifactId);
					resultModel.Success = true;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when deleting workspace. [{nameof(workspaceArtifactId)} = {workspaceArtifactId}]", ex);
				}

			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}
	}
}
