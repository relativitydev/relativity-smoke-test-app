using SmokeTest.Exceptions;
using System;
using System.Data.SqlClient;

namespace SmokeTest.Tests
{
	public class SqlHelper
	{
		public static int GetIdentifierFieldArtifactId(int workspaceArtifactId)
		{
			int documentIdentifierFieldArtifactId;
			string connectionstring = $"Data Source={TestConstants.ServerName}; Initial Catalog=EDDS{workspaceArtifactId}; User Id={TestConstants.SqlLogin}; Password={TestConstants.SqlPassword};";
			SqlConnection sqlConnection = new SqlConnection
			{
				ConnectionString = connectionstring
			};
			SqlCommand sqlCommand = new SqlCommand
			{
				CommandText = @"
										SELECT
												[FieldArtifactId]
										FROM
												[EDDSDBO].[QueryField]
										WHERE
												[ArtifactTypeId] = 10 
												AND [FieldCategoryId] = 2",
				Connection = sqlConnection
			};

			try
			{
				sqlConnection.Open();
				documentIdentifierFieldArtifactId = (int)sqlCommand.ExecuteScalar();

			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"An error occured when querying for Document IdentifierField. [{nameof(workspaceArtifactId)} = {workspaceArtifactId}]", ex);
			}
			finally
			{
				sqlConnection.Close();
			}

			return documentIdentifierFieldArtifactId;
		}
	}
}
