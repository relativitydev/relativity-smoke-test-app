using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using System.Collections.Generic;

namespace SmokeTest.Interfaces
{
	public interface IRdoHelper
	{
		void CreateTestRdoRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, string name, string status, string error, string errorDetails);
		void UpdateTestRdoRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, int artifactId, string name, string status, string error, string errorDetails);
		bool CheckIfTestRdoRecordExists(IRSAPIClient rsapiClient, int workspaceArtifactId, string name);
		RDO RetrieveTestRdo(IRSAPIClient rsapiClient, int workspaceArtifactId, string name);
		List<RDO> RetrieveAllTestRdos(IRSAPIClient rsapiClient, int workspaceArtifactId);
		int GetTestRdoRecordsCountWithStatus(IRSAPIClient rsapiClient, int workspaceArtifactId, string status = null);
	}
}