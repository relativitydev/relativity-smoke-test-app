using kCura.Relativity.Client;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
    public interface IImageHelper
    {
        ResultModel ImageDocuments(IRSAPIClient rsapiClient, int workspaceArtifactId);
    }
}