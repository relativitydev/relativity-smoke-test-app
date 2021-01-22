using System;

namespace SmokeTest.Agents
{
	[kCura.Agent.CustomAttributes.Name(Constants.Agents.TEST_AGENT_TO_CREATE_NAME)]
	[System.Runtime.InteropServices.Guid("05C9F5ED-0CFD-4B4D-B9FC-82FFEF374FC6")]
	public class DummyAgent : kCura.Agent.AgentBase
	{
		public override void Execute()
		{
			RaiseMessage($"Test message from {Constants.Agents.TEST_AGENT_TO_CREATE_NAME}! [TimeStamp: {DateTime.Now}]", 1);
		}

		public override string Name => Constants.Agents.TEST_AGENT_TO_CREATE_NAME;
	}
}
