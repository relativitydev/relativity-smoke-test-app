using Relativity.API;
using SmokeTest.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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

        public static DataTable RetrieveApplicationWorkspaces(IDBContext eddsDbContext, Guid applicationGuid)
        {
            const string sql = @"DECLARE @appArtifactID INT
						SET @appArtifactID = (SELECT ArtifactID FROM ArtifactGuid WHERE ArtifactGuid = @appGuid)

						SELECT  C.ArtifactID, C.Name
						FROM CaseApplication (NOLOCK) CA
						 INNER JOIN eddsdbo.[ExtendedCase] C ON CA.CaseID = C.ArtifactID
						 INNER JOIN eddsdbo.ResourceServer RS ON C.ServerID = RS.ArtifactID
						 INNER JOIN eddsdbo.Artifact A (NOLOCK) ON C.ArtifactID = A.ArtifactID
						 INNER JOIN eddsdbo.[ApplicationInstall] as AI on CA.CurrentApplicationInstallID = AI.ApplicationInstallID
						WHERE CA.ApplicationID = @appArtifactId
							AND AI.[Status] = 6 --Installed
						ORDER BY A.CreatedOn
						";

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@appGuid", SqlDbType.UniqueIdentifier) {Value = applicationGuid}
            };

            return eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
        }
    }
}
