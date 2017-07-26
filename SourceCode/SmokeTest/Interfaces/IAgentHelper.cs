using Relativity.Services.Agent;
using SmokeTest.Models;
using System.Collections.Generic;

namespace SmokeTest.Interfaces
{
  public interface IAgentHelper
  {
    ResultModel CreateAgent(IAgentManager agentManager, string agentName, int agentTypeId, int agentServer, bool enableAgent, int agentInterval, Agent.LoggingLevelEnum agentLoggingLevel);
    ResultModel DeleteAgent(IAgentManager agentManager, int agentArtifactId);
    List<Agent> GetAgentByName(IAgentManager agentManager, string agentName);
    int GetFirstAgentServerArtifactId(IAgentManager agentManager);
    int GetAgentTypeArtifactId(IAgentManager agentManager, string agentName);
  }
}