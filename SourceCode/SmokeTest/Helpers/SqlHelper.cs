using Relativity.API;
using SmokeTest.Exceptions;
using System;

namespace SmokeTest.Helpers
{
    public class SqlHelper
    {
        public static int GetIdentifierFieldArtifactId(IDBContext workspaceDbContext, int workspaceArtifactId)
        {
            int documentIdentifierFieldArtifactId;
            try
            {
                string sql = @"
                SELECT
                    [FieldArtifactId]
                FROM
                    [EDDSDBO].[QueryField]
                WHERE
                    [ArtifactTypeId] = 10 
                    AND [FieldCategoryId] = 2";
                documentIdentifierFieldArtifactId = workspaceDbContext.ExecuteSqlStatementAsScalar<int>(sql);
            }
            catch (Exception ex)
            {
                throw new SmokeTestException($"An error occured when querying for DocumentIdentifierFieldArtifactId. [{nameof(workspaceArtifactId)} = {workspaceArtifactId}]", ex);
            }

            return documentIdentifierFieldArtifactId;
        }
    }
}
