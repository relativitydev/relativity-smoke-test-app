using kCura.Relativity.Client;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
  public interface IUserHelper
  {
    ResultModel CreateUser(IRSAPIClient rsapiClient, string fistName, string lastName, string emailAddress);
    ResultModel DeleteUser(IRSAPIClient rsapiClient, int userArtifactId);
  }
}