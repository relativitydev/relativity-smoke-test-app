namespace SmokeTest.Agents
{
  [kCura.Agent.CustomAttributes.Name(Constants.SmokeTestAgentName)]
  [System.Runtime.InteropServices.Guid("05C9F5ED-0CFD-4B4D-B9FC-82FFEF374FC6")]
  public class SmokeTestAgent : kCura.Agent.AgentBase
  {
    public override void Execute()
    {
      RaiseMessage("Test message from Smoke Test Agent!", 1);
    }

    public override string Name => Constants.SmokeTestAgentName;
  }
}
