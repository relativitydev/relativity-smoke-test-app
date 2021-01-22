using Relativity.Services.Objects;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IAgentHelper
	{
		ResultModel CreateAgent(Relativity.Services.Interfaces.Agent.IAgentManager agentManager, IObjectManager objectManager, string applicationName, string agentName);
		ResultModel DeleteAgent(Relativity.Services.Interfaces.Agent.IAgentManager agentManager, int agentArtifactId);
	}
}