using kCura.Relativity.Client;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IWorkspaceHelper
	{
		ResultModel QueryTemplateAndCreateWorkspace(IRSAPIClient rsapiClient, string templateName, string workspaceName);
		ResultModel DeleteWorkspace(IRSAPIClient rsapiClient, int workspaceArtifactId);
	}
}