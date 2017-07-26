using kCura.Relativity.Client;

namespace SmokeTest.Interfaces
{
    public interface IRdoHelper
    {
        void CreateTestsRdoRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, string name, string result, string errorMessage);
    }
}