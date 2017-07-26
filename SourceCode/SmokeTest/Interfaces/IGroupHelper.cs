using kCura.Relativity.Client;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
  public interface IGroupHelper
  {
    ResultModel CreateGroup(IRSAPIClient rsapiClient, string groupName);
    ResultModel DeleteGroup(IRSAPIClient rsapiClient, int groupArtifactId);
  }
}