using System;

namespace SmokeTest.Agents
{
	[kCura.Agent.CustomAttributes.Name(Constants.TestAgentToCreateName)]
	[System.Runtime.InteropServices.Guid("05C9F5ED-0CFD-4B4D-B9FC-82FFEF374FC6")]
	public class DummyAgent : kCura.Agent.AgentBase
	{
		public override void Execute()
		{
			RaiseMessage($"Test message from {Constants.TestAgentToCreateName}! [TimeStamp: {DateTime.Now}]", 1);
		}

		public override string Name => Constants.TestAgentToCreateName;
	}
}
