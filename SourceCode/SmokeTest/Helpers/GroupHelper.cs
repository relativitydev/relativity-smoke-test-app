using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Linq;

namespace SmokeTest.Helpers
{
	public class GroupHelper : IGroupHelper
	{
		public ResultModel CreateGroup(IRSAPIClient rsapiClient, string groupName)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (groupName == null)
			{
				throw new ArgumentNullException(nameof(groupName));
			}

			ResultModel resultModel = new ResultModel("Group");
			rsapiClient.APIOptions.WorkspaceID = -1;

			try
			{
				Group groupDto = new Group
				{
					Name = groupName
				};
				try
				{
					int newGroupArtifactId = rsapiClient.Repositories.Group.CreateSingle(groupDto);
					resultModel.Success = true;
					resultModel.ArtifactId = newGroupArtifactId;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when creating groupName. [{nameof(groupName)} = {groupName}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		public ResultModel DeleteGroup(IRSAPIClient rsapiClient, int groupArtifactId)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (groupArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(groupArtifactId)} should be a positive number.");
			}

			ResultModel resultModel = new ResultModel("Group");
			rsapiClient.APIOptions.WorkspaceID = -1;

			try
			{
				try
				{
					rsapiClient.Repositories.Group.DeleteSingle(groupArtifactId);
					resultModel.Success = true;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when deleting groupName. [{nameof(groupArtifactId)} = {groupArtifactId}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		public static int FindGroupArtifactId(IRSAPIClient rsapiClient, string groupName)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (groupName == null)
			{
				throw new ArgumentNullException(nameof(groupName));
			}

			try
			{
				int groupArtifactId = 0;
				TextCondition groupCondition = new TextCondition(GroupFieldNames.Name, TextConditionEnum.EqualTo, groupName);
				Query<Group> queryGroup = new Query<Group> { Condition = groupCondition };
				queryGroup.Fields.Add(new FieldValue(ArtifactQueryFieldNames.ArtifactID));

				try
				{
					QueryResultSet<Group> groupQueryResultSet = rsapiClient.Repositories.Group.Query(queryGroup);
					if (groupQueryResultSet.Success && groupQueryResultSet.Results.Count == 1)
					{
						Result<Group> firstOrDefault = groupQueryResultSet.Results.FirstOrDefault();
						if (firstOrDefault != null)
						{
							groupArtifactId = firstOrDefault.Artifact.ArtifactID;
						}
					}
					else
					{
						throw new SmokeTestException($"An error occured when querying for GroupArtifactId. [{nameof(groupQueryResultSet.Success)} = {groupQueryResultSet.Success}, {nameof(groupQueryResultSet.Results.Count)} = {groupQueryResultSet.Results.Count}]");
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when querying for GroupArtifactId. Query. [{nameof(groupName)} = {groupName}]", ex);
				}
				return groupArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"An error occured when querying for GroupArtifactId. [{nameof(groupName)} = {groupName}]", ex);
			}
		}
	}
}
