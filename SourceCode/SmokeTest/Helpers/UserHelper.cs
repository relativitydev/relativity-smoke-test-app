using kCura.Relativity.Client;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;

namespace SmokeTest.Helpers
{
  public class UserHelper : IUserHelper
  {
    public ResultModel CreateUser(IRSAPIClient rsapiClient, string firstName, string lastName, string emailAddress)
    {
      if (rsapiClient == null)
      {
        throw new ArgumentNullException(nameof(rsapiClient));
      }
      if (firstName == null)
      {
        throw new ArgumentNullException(nameof(firstName));
      }
      if (lastName == null)
      {
        throw new ArgumentNullException(nameof(lastName));
      }
      if (emailAddress == null)
      {
        throw new ArgumentNullException(nameof(emailAddress));
      }

      ResultModel resultModel = new ResultModel("User");
      rsapiClient.APIOptions.WorkspaceID = -1;

      try
      {
        const int defaultSelectedFileType = 1;
        const int userType = 3;
        const int documentSkip = 1000003;
        const int skipDefaultPreference = 1000004;
        const int password = 1000005;
        const int sendNewPasswordTo = 1000006;
        int returnPasswordCodeId = ChoiceHelper.FindChoiceArtifactId(rsapiClient, sendNewPasswordTo, "Return");
        int passwordCodeId = ChoiceHelper.FindChoiceArtifactId(rsapiClient, password, "Auto-generate password");
        int documentSkipCodeId = ChoiceHelper.FindChoiceArtifactId(rsapiClient, documentSkip, "Enabled");
        int documentSkipPreferenceCodeId = ChoiceHelper.FindChoiceArtifactId(rsapiClient, skipDefaultPreference, "Normal");
        int defaultFileTypeCodeId = ChoiceHelper.FindChoiceArtifactId(rsapiClient, defaultSelectedFileType, "Native");
        int userTypeCodeId = ChoiceHelper.FindChoiceArtifactId(rsapiClient, userType, "Internal");
        int everyoneGroupArtifactId = GroupHelper.FindGroupArtifactId(rsapiClient, "Everyone");
        int clientArtifactId = ClientHelper.FindClientArtifactId(rsapiClient, "Relativity Template");

        kCura.Relativity.Client.DTOs.User userDto =
          new kCura.Relativity.Client.DTOs.User
          {
            AdvancedSearchPublicByDefault = true,
            AuthenticationData = "",
            BetaUser = false,
            ChangePassword = true,
            ChangePasswordNextLogin = false,
            ChangeSettings = true,
            Client = new kCura.Relativity.Client.DTOs.Client(clientArtifactId),
            DataFocus = 1,
            DefaultSelectedFileType = new kCura.Relativity.Client.DTOs.Choice(defaultFileTypeCodeId),
            DocumentSkip = new kCura.Relativity.Client.DTOs.Choice(documentSkipCodeId),
            FirstName = firstName,
            LastName = lastName,
            EmailAddress = emailAddress,
            EnforceViewerCompatibility = true,
            Groups = new List<kCura.Relativity.Client.DTOs.Group>
            {
              new kCura.Relativity.Client.DTOs.Group(everyoneGroupArtifactId)
            },
            ItemListPageLength = 25,
            KeyboardShortcuts = true,
            MaximumPasswordAge = 0,
            NativeViewerCacheAhead = true,
            PasswordAction = new kCura.Relativity.Client.DTOs.Choice(passwordCodeId),
            RelativityAccess = true,
            SendPasswordTo = new kCura.Relativity.Client.DTOs.Choice(returnPasswordCodeId),
            SkipDefaultPreference = new kCura.Relativity.Client.DTOs.Choice(documentSkipPreferenceCodeId),
            TrustedIPs = "",
            Type = new kCura.Relativity.Client.DTOs.Choice(userTypeCodeId)
          };

        try
        {
          int newUserArtifactId = rsapiClient.Repositories.User.CreateSingle(userDto);
          resultModel.Success = true;
          resultModel.ArtifactId = newUserArtifactId;
        }
        catch (Exception ex)
        {
          throw new SmokeTestException("An error occured when creating user.", ex);
        }
      }
      catch (Exception ex)
      {
        resultModel.Success = false;
        resultModel.ErrorMessage = ex.ToString();
      }

      return resultModel;
    }

    public ResultModel DeleteUser(IRSAPIClient rsapiClient, int userArtifactId)
    {
      if (rsapiClient == null)
      {
        throw new ArgumentNullException(nameof(rsapiClient));
      }
      if (userArtifactId < 1)
      {
        throw new ArgumentException($"{nameof(userArtifactId)} should be a positive number.");
      }

      ResultModel resultModel = new ResultModel("User");
      rsapiClient.APIOptions.WorkspaceID = -1;

      try
      {
        try
        {
          rsapiClient.Repositories.User.DeleteSingle(userArtifactId);
          resultModel.Success = true;
        }
        catch (Exception ex)
        {
          throw new SmokeTestException($"An error occured when deleting user. [{nameof(userArtifactId)} = {userArtifactId}]", ex);
        }
      }
      catch (Exception ex)
      {
        resultModel.Success = false;
        resultModel.ErrorMessage = ex.ToString();
      }

      return resultModel;
    }
  }
}
