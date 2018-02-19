using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using System;

namespace SmokeTest.Helpers
{
    public class RdoHelper : IRdoHelper
    {
        public void CreateTestsRdoRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, string name, string result, string errorMessage)
        {
            rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

            try
            {
                RDO rdoDto = new RDO();
                rdoDto.ArtifactTypeGuids.Add(Constants.Guids.ObjectType.Test);
                rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Name, name));
                rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.Result, result));
                rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Test.ErrorMessage, errorMessage));


                try
                {
                    rsapiClient.Repositories.RDO.CreateSingle(rdoDto);
                }
                catch (Exception ex)
                {
                    throw new SmokeTestException($"An error occured when creating Tests RDO record. CreateSingle. [{nameof(name)} = {name}, {nameof(result)} = {result}, {nameof(errorMessage)} = {errorMessage}]", ex);
                }
            }
            catch (Exception ex)
            {
                throw new SmokeTestException($"An error occured when creating Tests RDO record. [{nameof(name)} = {name}, {nameof(result)} = {result}, {nameof(errorMessage)} = {errorMessage}]", ex);
            }
        }

        public bool CheckIfTestsRdoRecordExists(IRSAPIClient rsapiClient, int workspaceArtifactId, string name)
        {
            bool retVal;
            string errorContext = "An error occured when querying for Tests RDO record.";
            rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

            try
            {
                Query<RDO> testsRdoQuery = new Query<RDO>
                {
                    ArtifactTypeGuid = Constants.Guids.ObjectType.Test,
                    Condition = new TextCondition(Constants.Guids.Fields.Test.Name, TextConditionEnum.EqualTo, name),
                    Fields = FieldValue.NoFields
                };
                QueryResultSet<RDO> testsRdoQueryResultSet;

                try
                {
                    testsRdoQueryResultSet = rsapiClient.Repositories.RDO.Query(testsRdoQuery);
                }
                catch (Exception ex)
                {
                    throw new SmokeTestException($"{errorContext}. Query. [{nameof(name)}: {name}]", ex);
                }

                if (!testsRdoQueryResultSet.Success)
                {
                    throw new SmokeTestException($"{errorContext}. [{nameof(name)}: {name}, ErrorMessage: {testsRdoQueryResultSet.Message}]");
                }


                retVal = testsRdoQueryResultSet.Results.Count > 0;
            }
            catch (Exception ex)
            {
                throw new SmokeTestException($"{errorContext}. [{nameof(name)}: {name}]", ex);
            }

            return retVal;
        }
    }
}
