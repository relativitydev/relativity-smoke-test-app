using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmokeTest.Helpers
{
	public class RdoHelper : IRdoHelper
	{
		public void CreateTestRdoRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, string name, string status, string error, string errorDetails)
		{
			string errorContext = "An error occured when creating Test RDO record";
			string errorProperties = $"{errorContext}. CreateSingle. [{nameof(name)} = {name}, {nameof(status)} = {status}, {nameof(error)} = {error}, {nameof(errorDetails)} = {errorDetails}]";
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				RDO rdoDto = new RDO();
				rdoDto.ArtifactTypeGuids.Add(Constants.Guids.ObjectType.Test);
				rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Name, name));
				rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Status, status));
				rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Error, error));
				rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.ErrorDetails, errorDetails));

				try
				{
					rsapiClient.Repositories.RDO.CreateSingle(rdoDto);
				}
				catch (Exception ex)
				{

					throw new SmokeTestException($"{errorContext}. CreateSingle. [{errorProperties}]", ex);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [{errorProperties}]", ex);
			}
		}

		public void UpdateTestRdoRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, int artifactId, string name, string status, string error, string errorDetails)
		{
			string errorContext = "An error occured when updating Test RDO record";
			string errorProperties = $"{errorContext}. CreateSingle. [{nameof(name)} = {name}, {nameof(status)} = {status}, {nameof(error)} = {error}, {nameof(errorDetails)} = {errorDetails}]";
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				RDO rdoDto = new RDO(artifactId);
				rdoDto.ArtifactTypeGuids.Add(Constants.Guids.ObjectType.Test);
				if (name != null)
				{
					rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Name, name));
				}
				if (status != null)
				{
					rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Status, status));
				}
				if (error != null)
				{
					rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Error, error));
				}
				if (errorDetails != null)
				{
					rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.ErrorDetails, errorDetails));
				}

				try
				{
					rsapiClient.Repositories.RDO.UpdateSingle(rdoDto);
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. UpdateSingle. [{errorProperties}]", ex);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [{errorProperties}]", ex);
			}
		}

		public bool CheckIfTestRdoRecordExists(IRSAPIClient rsapiClient, int workspaceArtifactId, string name)
		{
			bool retVal;
			string errorContext = "An error occured when querying for Test RDO record.";
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				Query<RDO> testRdoQuery = new Query<RDO>
				{
					ArtifactTypeGuid = Constants.Guids.ObjectType.Test,
					Condition = new TextCondition(Constants.Guids.Fields.Test.Name, TextConditionEnum.EqualTo, name),
					Fields = FieldValue.NoFields
				};
				QueryResultSet<RDO> testRdoQueryResultSet;

				try
				{
					testRdoQueryResultSet = rsapiClient.Repositories.RDO.Query(testRdoQuery);
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. Query. [{nameof(name)}: {name}]", ex);
				}

				if (!testRdoQueryResultSet.Success)
				{
					throw new SmokeTestException($"{errorContext}. [{nameof(name)}: {name}, ErrorMessage: {testRdoQueryResultSet.Message}]");
				}


				retVal = testRdoQueryResultSet.Results.Count > 0;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [{nameof(name)}: {name}]", ex);
			}

			return retVal;
		}

		public RDO RetrieveTestRdo(IRSAPIClient rsapiClient, int workspaceArtifactId, string name)
		{
			RDO retVal;
			string errorContext = "An error occured when querying for Test RDO record.";
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				Query<RDO> testRdoQuery = new Query<RDO>
				{
					ArtifactTypeGuid = Constants.Guids.ObjectType.Test,
					Condition = new TextCondition(Constants.Guids.Fields.Test.Name, TextConditionEnum.EqualTo, name),
					Fields = FieldValue.AllFields
				};
				QueryResultSet<RDO> testRdoQueryResultSet;

				try
				{
					testRdoQueryResultSet = rsapiClient.Repositories.RDO.Query(testRdoQuery);
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. Query. [{nameof(name)}: {name}]", ex);
				}

				if (!testRdoQueryResultSet.Success)
				{
					throw new SmokeTestException($"{errorContext}. [{nameof(name)}: {name}, ErrorMessage: {testRdoQueryResultSet.Message}]");
				}


				retVal = testRdoQueryResultSet.Results.Count > 0
						? testRdoQueryResultSet.Results.First().Artifact
						: null;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [{nameof(name)}: {name}]", ex);
			}

			return retVal;
		}

		public List<RDO> RetrieveAllTestRdos(IRSAPIClient rsapiClient, int workspaceArtifactId)
		{
			List<RDO> retVal = new List<RDO>();
			string errorContext = "An error occured when querying for all the Test RDO records.";
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				Query<RDO> testRdoQuery = new Query<RDO>
				{
					ArtifactTypeGuid = Constants.Guids.ObjectType.Test,
					Fields = FieldValue.AllFields
				};
				QueryResultSet<RDO> testRdoQueryResultSet;

				try
				{
					testRdoQueryResultSet = rsapiClient.Repositories.RDO.Query(testRdoQuery);
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. Query.", ex);
				}

				if (!testRdoQueryResultSet.Success)
				{
					throw new SmokeTestException($"{errorContext}. [ErrorMessage: {testRdoQueryResultSet.Message}]");
				}

				if (testRdoQueryResultSet.Results.Count > 0)
				{
					retVal.AddRange(testRdoQueryResultSet.Results.Select(x => x.Artifact));
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}.", ex);
			}

			return retVal;
		}

		public int GetTestRdoRecordsCountWithStatus(IRSAPIClient rsapiClient, int workspaceArtifactId, string status = null)
		{
			int retVal;
			string errorContext = $"An error occured when querying for Test RDO records count. [{nameof(status)}: {status}]";
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				Query<RDO> testRdoQuery = new Query<RDO>
				{
					ArtifactTypeGuid = Constants.Guids.ObjectType.Test,
					Fields = FieldValue.NoFields
				};
				if (status != null)
				{
					testRdoQuery.Condition = new TextCondition(Constants.Guids.Fields.Test.Status, TextConditionEnum.EqualTo, status);
				}

				QueryResultSet<RDO> testRdoQueryResultSet;

				try
				{
					testRdoQueryResultSet = rsapiClient.Repositories.RDO.Query(testRdoQuery);
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. Query. [{nameof(status)}: {status}]", ex);
				}

				if (!testRdoQueryResultSet.Success)
				{
					throw new SmokeTestException($"{errorContext}. [{nameof(status)}: {status}, ErrorMessage: {testRdoQueryResultSet.Message}]");
				}


				retVal = testRdoQueryResultSet.Results.Count;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}. [{nameof(status)}: {status}]", ex);
			}

			return retVal;
		}
	}
}
