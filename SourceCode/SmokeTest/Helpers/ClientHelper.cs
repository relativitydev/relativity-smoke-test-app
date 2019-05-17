using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using System;
using System.Linq;

namespace SmokeTest.Helpers
{
	public class ClientHelper
	{
		public static int FindClientArtifactId(IRSAPIClient rsapiClient, string clientName)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (clientName == null)
			{
				throw new ArgumentNullException(nameof(clientName));
			}

			try
			{
				int clientArtifactId = 0;
				TextCondition clientCondition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, clientName);
				Query<Client> queryClient = new Query<Client>
				{
					Condition = clientCondition,
					Fields = FieldValue.AllFields
				};

				try
				{
					QueryResultSet<Client> resultSetClient = rsapiClient.Repositories.Client.Query(queryClient);
					if (resultSetClient.Success && resultSetClient.Results.Count == 1)
					{
						Result<Client> firstOrDefault = resultSetClient.Results.FirstOrDefault();
						if (firstOrDefault != null)
						{
							clientArtifactId = firstOrDefault.Artifact.ArtifactID;
						}
					}
					else
					{
						throw new SmokeTestException($"An error occured when querying for ClientArtifactId. [{nameof(resultSetClient.Success)} = {resultSetClient.Success}, {nameof(resultSetClient.Results.Count)} = {resultSetClient.Results.Count}]");
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when querying for ClientArtifactId. Query. [{nameof(clientName)} = {clientName}]", ex);
				}
				return clientArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"An error occured when querying for ClientArtifactId. [{nameof(clientName)} = {clientName}]", ex);
			}
		}
	}
}
