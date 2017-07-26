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
                rdoDto.ArtifactTypeGuids.Add(Constants.Guids.ObjectType.Tests);
                rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Tests.Name, name));
                rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Tests.Result, result));
                rdoDto.Fields.Add(new FieldValue(Constants.Guids.Fields.Tests.ErrorMessage, errorMessage));


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
    }
}
