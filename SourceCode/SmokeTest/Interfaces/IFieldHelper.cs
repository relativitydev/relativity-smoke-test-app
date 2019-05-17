using kCura.Relativity.Client;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IFieldHelper
	{
		ResultModel CreateSingleChoiceDocumentField(IRSAPIClient rsapiClient, int workspaceArtifactId, string fieldName);
		ResultModel DeleteSingleChoiceDocumentField(IRSAPIClient rsapiClient, int workspaceArtifactId, int fieldArtifactId);
	}
}