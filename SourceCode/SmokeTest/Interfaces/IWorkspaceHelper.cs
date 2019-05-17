using kCura.Relativity.Client;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IWorkspaceHelper
	{
		ResultModel CreateWorkspace(IRSAPIClient rsapiClient, string workspaceName);
		ResultModel DeleteWorkspace(IRSAPIClient rsapiClient, int workspaceArtifactId);
	}
}