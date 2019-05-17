using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmokeTest.Helpers
{
	public class ChoiceHelper
	{
		public static int FindChoiceArtifactId(IRSAPIClient rsapiClient, int choiceTypeId, string value)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (choiceTypeId < 1)
			{
				throw new ArgumentException($"{nameof(choiceTypeId)} should be a positive number.");
			}
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			try
			{
				int choiceArtifactId = 0;
				WholeNumberCondition choiceTypeCondition = new WholeNumberCondition(ChoiceFieldNames.ChoiceTypeID, NumericConditionEnum.EqualTo, (int)choiceTypeId);
				TextCondition choiceNameCondition = new TextCondition(ChoiceFieldNames.Name, TextConditionEnum.EqualTo, value);
				CompositeCondition choiceCompositeCondition = new CompositeCondition(choiceTypeCondition, CompositeConditionEnum.And, choiceNameCondition);
				List<FieldValue> fieldValues = new List<FieldValue>
				{
					new FieldValue(ArtifactQueryFieldNames.ArtifactID)
				};
				List<Sort> sorts = new List<Sort>();
				Query<kCura.Relativity.Client.DTOs.Choice> choiceQuery = new Query<kCura.Relativity.Client.DTOs.Choice>(fieldValues, choiceCompositeCondition, sorts);

				try
				{
					QueryResultSet<kCura.Relativity.Client.DTOs.Choice> choiceQueryResult = rsapiClient.Repositories.Choice.Query(choiceQuery);
					if (choiceQueryResult.Success && choiceQueryResult.Results.Count == 1)
					{
						Result<kCura.Relativity.Client.DTOs.Choice> firstOrDefault = choiceQueryResult.Results.FirstOrDefault();
						if (firstOrDefault != null)
						{
							choiceArtifactId = firstOrDefault.Artifact.ArtifactID;
						}
					}
					else
					{
						throw new SmokeTestException($"An error occured when querying for ChoiceArtifactId. [{nameof(choiceQueryResult.Success)} = {choiceQueryResult.Success}, {nameof(choiceQueryResult.Results.Count)} = {choiceQueryResult.Results.Count}]");
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when querying for ChoiceArtifactId. Query. [{nameof(choiceTypeId)} = {choiceTypeId}, {nameof(value)} = {value}]", ex);
				}
				return choiceArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"An error occured when querying for ChoiceArtifactId. [{nameof(choiceTypeId)} = {choiceTypeId}, {nameof(value)} = {value}]", ex);
			}
		}
	}
}
