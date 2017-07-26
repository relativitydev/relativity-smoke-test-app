using Relativity.Services.Agent;
using Relativity.Services.ResourceServer;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmokeTest.Helpers
{
  public class AgentHelper : IAgentHelper
  {
    public ResultModel CreateAgent(IAgentManager agentManager, string agentName, int agentTypeId, int agentServer, bool enableAgent, int agentInterval, Agent.LoggingLevelEnum agentLoggingLevel)
    {
      if (agentManager == null)
      {
        throw new ArgumentNullException(nameof(agentManager));
      }
      if (agentName == null)
      {
        throw new ArgumentNullException(nameof(agentName));
      }

      if (string.IsNullOrWhiteSpace(agentName))
      {
        throw new ArgumentException($"{nameof(agentName)} cannot be an empty string.");
      }

      if (agentTypeId <= 0)
      {
        throw new ArgumentException($"{nameof(agentTypeId)} should be a positive number.");
      }

      if (agentServer <= 0)
      {
        throw new ArgumentException($"{nameof(agentServer)} should be a positive number.");
      }

      if (agentInterval <= 0)
      {
        throw new ArgumentException($"{nameof(agentInterval)} should be a positive number.");
      }

      ResultModel resultModel = new ResultModel("Agent");
      try
      {
        try
        {
          Agent newAgent = new Agent
          {
            AgentType = new AgentTypeRef(agentTypeId),
            Enabled = enableAgent,
            Interval = agentInterval,
            Name = agentName,
            LoggingLevel = agentLoggingLevel,
            Server = new ResourceServerRef
            {
              ArtifactID = agentServer
            }
          };

          int newAgentArtifactId = agentManager.CreateSingleAsync(newAgent).Result;
          resultModel.Success = true;
          resultModel.ArtifactId = newAgentArtifactId;
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

    public ResultModel DeleteAgent(IAgentManager agentManager, int agentArtifactId)
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
          //Verify Agent still exists
          bool doesAgentExists = GetAgentByArtifactId(agentManager, agentArtifactId) != null;
          if (doesAgentExists)
          {
            agentManager.DeleteSingleAsync(agentArtifactId).Wait();
            resultModel.Success = true;
          }
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

    private Agent GetAgentByArtifactId(IAgentManager agentManager, int agentArtifactId)
    {
      if (agentManager == null)
      {
        throw new ArgumentNullException(nameof(agentManager));
      }
      if (agentArtifactId <= 0)
      {
        throw new ArgumentException($"{nameof(agentArtifactId)} should be a positive number.");
      }

      try
      {
        Agent agent = agentManager.ReadSingleAsync(agentArtifactId).Result;
        return agent;
      }
      catch (Exception ex)
      {
        throw new SmokeTestException($"An error occured when querying for agent. [{nameof(agentArtifactId)} = {agentArtifactId}].", ex);
      }
    }

    public List<Agent> GetAgentByName(IAgentManager agentManager, string agentName)
    {
      if (agentManager == null)
      {
        throw new ArgumentNullException(nameof(agentManager));
      }
      if (agentName == null)
      {
        throw new ArgumentNullException(nameof(agentName));
      }
      if (string.IsNullOrWhiteSpace(agentName))
      {
        throw new ArgumentException($"{nameof(agentName)} cannot be an empty string.");
      }

      try
      {
        Relativity.Services.Query agentQuery = new Relativity.Services.Query
        {
          Condition = $"'Name' LIKE '{agentName}'"
        };

        AgentQueryResultSet agentQueryResultSet = agentManager.QueryAsync(agentQuery).Result;

        if (agentQueryResultSet?.Results == null)
        {
          throw new SmokeTestException($"An error occured when querying for agent. QueryAsync. [{nameof(agentName)} = {agentName}].");
        }

        List<Agent> agents = new List<Agent>();
        agents.AddRange(agentQueryResultSet.Results.Select(x => x.Artifact));
        return agents;

      }
      catch (Exception ex)
      {
        throw new SmokeTestException($"An error occured when querying for agent. [{nameof(agentName)} = {agentName}].", ex);
      }
    }

    public int GetFirstAgentServerArtifactId(IAgentManager agentManager)
    {
      if (agentManager == null)
      {
        throw new ArgumentNullException(nameof(agentManager));
      }

      try
      {
        List<ResourceServer> agentServers = agentManager.GetAgentServersAsync().Result;
        int agentServerId = agentServers.First().ArtifactID;
        return agentServerId;
      }
      catch (Exception ex)
      {
        throw new SmokeTestException($"An error occured when querying for first agent server id.", ex);
      }
    }

    public int GetAgentTypeArtifactId(IAgentManager agentManager, string agentName)
    {
      if (agentManager == null)
      {
        throw new ArgumentNullException(nameof(agentManager));
      }
      if (agentName == null)
      {
        throw new ArgumentNullException(nameof(agentName));
      }

      try
      {
        List<AgentTypeRef> agentServers = agentManager.GetAgentTypesAsync().Result;
        int agentTypeId = agentServers.First(x => x.Name.Equals(agentName)).ArtifactID;
        return agentTypeId;
      }
      catch (Exception ex)
      {
        throw new SmokeTestException($"An error occured when querying for '{agentName}' agent.", ex);
      }
    }
  }
}
