using Relativity.Services.Interfaces.Agent.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmokeTest.Helpers
{
	public class AgentHelper : IAgentHelper
	{
		public ResultModel CreateAgent(Relativity.Services.Interfaces.Agent.IAgentManager agentManager, IObjectManager objectManager, string applicationName, string agentName)
		{
			if (agentManager == null)
			{
				throw new ArgumentNullException(nameof(agentManager));
			}
			if (objectManager == null)
			{
				throw new ArgumentNullException(nameof(objectManager));
			}
			if (string.IsNullOrWhiteSpace(applicationName))
			{
				throw new ArgumentException($"{nameof(applicationName)} is not valid", nameof(applicationName));
			}
			if (string.IsNullOrWhiteSpace(agentName))
			{
				throw new ArgumentException($"{nameof(agentName)} is not valid", nameof(agentName));
			}

			ResultModel resultModel = new ResultModel("Agent");
			try
			{
				try
				{
					//Query all Agent Types in the Instance
					List<AgentTypeResponse> agentTypesInInstance = GetAgentTypesInInstanceAsync(agentManager).Result;

					//Filter Agent Types from Relativity application
					List<AgentTypeResponse> agentTypesInApplication = agentTypesInInstance.Where(x => x.ApplicationName.Equals(applicationName)).ToList();

					//Agent to create
					AgentTypeResponse agentTypeResponse = agentTypesInApplication.First(x => x.Name.Equals(agentName));
					int agentTypeArtifactId = agentTypeResponse.ArtifactID;
					decimal defaultInterval = agentTypeResponse.DefaultInterval ?? Constants.Agents.AGENT_INTERVAL;
					int defaultLoggingLevel = agentTypeResponse.DefaultLoggingLevel ?? (int)Constants.Agents.AGENT_LOGGING_LEVEL;

					//Check if Agent already exists
					bool doesAgentExists = CheckIfAtLeastSingleInstanceOfAgentExistsAsync(objectManager, agentName).Result;

					if (doesAgentExists)
					{
						Console.WriteLine($"Agent already exists. Skipped creation. [{nameof(agentName)}:{agentName}]");
					}
					else
					{
						//Query Agent Server for Agent Type
						List<AgentServerResponse> agentServersForAgentType = GetAgentServersForAgentTypeAsync(agentManager, agentTypeArtifactId).Result;
						int firstAgentServerArtifactId = agentServersForAgentType.First().ArtifactID;

						//Create Single Agent
						int newAgentArtifactId = CreateAgentAsync(agentManager, agentTypeArtifactId, firstAgentServerArtifactId, defaultInterval, defaultLoggingLevel).Result;
						Console.WriteLine($"Agent Created. [{nameof(agentName)}: {agentName}]");

						resultModel.Success = true;
						resultModel.ArtifactId = newAgentArtifactId;
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when creating agent. [{nameof(agentName)} = {agentName}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		public ResultModel DeleteAgent(Relativity.Services.Interfaces.Agent.IAgentManager agentManager, int agentArtifactId)
		{
			if (agentManager == null)
			{
				throw new ArgumentNullException(nameof(agentManager));
			}
			if (agentArtifactId <= 0)
			{
				throw new ArgumentException($"{nameof(agentArtifactId)} should be a positive number.");
			}

			ResultModel resultModel = new ResultModel("Agent");

			try
			{
				try
				{
					agentManager.DeleteAsync(Constants.Agents.EDDS_WORKSPACE_ARTIFACT_ID, agentArtifactId).Wait();
					resultModel.Success = true;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when deleting agent. [{nameof(agentArtifactId)} = {agentArtifactId}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		private async Task<List<AgentTypeResponse>> GetAgentTypesInInstanceAsync(Relativity.Services.Interfaces.Agent.IAgentManager agentManager)
		{
			List<AgentTypeResponse> agentTypeResponseList = await agentManager.GetAgentTypesAsync(Constants.Agents.EDDS_WORKSPACE_ARTIFACT_ID);
			Console.WriteLine($"Total Agent Types in Instance: {agentTypeResponseList.Count}");
			return agentTypeResponseList;
		}

		private async Task<bool> CheckIfAtLeastSingleInstanceOfAgentExistsAsync(IObjectManager objectManager, string agentName)
		{
			List<int> agentArtifactIds = await GetAgentArtifactIdsAsync(objectManager, agentName);

			bool doesAgentExists = agentArtifactIds.Count > 0;
			return doesAgentExists;
		}

		private async Task<List<int>> GetAgentArtifactIdsAsync(IObjectManager objectManager, string agentName)
		{
			List<int> agentArtifactIds = new List<int>();

			QueryRequest agentQueryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Name = Constants.Agents.AGENT_OBJECT_TYPE
				},
				Fields = new List<FieldRef>
				{
					new FieldRef
					{
						Name = Constants.Agents.AGENT_FIELD_NAME
					}
				},
				Condition = $"(('{Constants.Agents.AGENT_FIELD_NAME}' LIKE '{agentName}'))"
			};

			QueryResult agentQueryResult = await objectManager.QueryAsync(
				Constants.Agents.EDDS_WORKSPACE_ARTIFACT_ID,
				agentQueryRequest,
				1,
				3);

			if (agentQueryResult.Objects.Count > 0)
			{
				agentArtifactIds.AddRange(agentQueryResult.Objects.Select(x => x.ArtifactID).ToList());
			}

			return agentArtifactIds;
		}

		private async Task<List<AgentServerResponse>> GetAgentServersForAgentTypeAsync(Relativity.Services.Interfaces.Agent.IAgentManager agentManager, int agentTypeArtifactId)
		{
			List<AgentServerResponse> agentServerResponseList = await agentManager.GetAvailableAgentServersAsync(Constants.Agents.EDDS_WORKSPACE_ARTIFACT_ID, agentTypeArtifactId);
			Console.WriteLine($"Total Available Agent Servers for Agent Type: {agentServerResponseList.Count}");
			return agentServerResponseList;
		}

		private async Task<int> CreateAgentAsync(Relativity.Services.Interfaces.Agent.IAgentManager agentManager, int agentTypeArtifactId, int agentServerArtifactId, decimal defaultInterval, int defaultLoggingLevel)
		{
			AgentRequest newAgentRequest = new AgentRequest
			{
				Enabled = Constants.Agents.ENABLE_AGENT,
				Interval = defaultInterval,
				Keywords = Constants.Agents.KEYWORDS,
				Notes = Constants.Agents.NOTES,
				LoggingLevel = defaultLoggingLevel,
				AgentType = new Securable<ObjectIdentifier>(
					new ObjectIdentifier
					{
						ArtifactID = agentTypeArtifactId
					}),
				AgentServer = new Securable<ObjectIdentifier>(
					new ObjectIdentifier
					{
						ArtifactID = agentServerArtifactId
					})
			};

			int newAgentArtifactId = await agentManager.CreateAsync(Constants.Agents.EDDS_WORKSPACE_ARTIFACT_ID, newAgentRequest);
			Console.WriteLine($"{nameof(newAgentArtifactId)}: {newAgentArtifactId}");
			return newAgentArtifactId;
		}
	}
}
