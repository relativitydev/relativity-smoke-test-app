using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Audit.Services.Interface.Query;
using Relativity.Services.Objects.DataContracts;
using SmokeTest.Exceptions;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Sort = Relativity.Services.Objects.DataContracts.Sort;

namespace SmokeTest.Helpers
{
	public class DataGridHelper
	{
		public IRSAPIClient RsapiClient;
		public IAuditObjectManagerUIService AuditObjectManagerUiService;
		public string RelativityUrl;
		public const String DataGridFieldName = "Extracted Text";

		public DataGridHelper(IRSAPIClient client, IAuditObjectManagerUIService auditObjectManagerUiService, string relativityUrl)
		{

			RsapiClient = client ?? throw new ArgumentNullException(nameof(client));
			AuditObjectManagerUiService = auditObjectManagerUiService ?? throw new ArgumentNullException(nameof(auditObjectManagerUiService));
			RelativityUrl = relativityUrl ?? throw new ArgumentNullException(nameof(relativityUrl));
		}

		public ResultModel VerifyDataGridFunctionality(int workspaceID)
		{
			var retVal = new ResultModel("DataGrid");
			RsapiClient.APIOptions.WorkspaceID = workspaceID;
			try
			{
				// Make sure Data Grid Core Is installed
				var dataGridCoreIsInstalled = CheckIfDataGridCoreIsInstalled();
				if (!dataGridCoreIsInstalled)
				{
					retVal.Success = false;
					retVal.ErrorMessage = "Data Grid Core is not installed";
				}
				else
				{
					// Make sure Data Grid is enabled in the Workspace
					var dataGridIsEnabled = CheckIfDataGridIsEnabled(workspaceID);
					if (!dataGridIsEnabled)
					{
						retVal.Success = false;
						retVal.ErrorMessage = "Data Grid is not enabled in the workspace";
					}
					else
					{
						// Make sure DataGrid is enabled on the extracted text field
						var dataGridIsEnabledOnExtractedTextField = CheckIfDataGridIsEnabledOnExtractedTextField();
						if (dataGridIsEnabledOnExtractedTextField)
						{
							retVal.Success = true;
						}
						else
						{
							retVal.Success = false;
							retVal.ErrorMessage = "Data Grid is not enabled on the extracted text field.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				retVal.ErrorMessage = $@"Error running Data Grid Test: {ex.ToString()}";
				retVal.Success = false;
			}
			return retVal;
		}

		private bool CheckIfDataGridCoreIsInstalled()
		{
			try
			{
				var query = new kCura.Relativity.Client.DTOs.Query<kCura.Relativity.Client.DTOs.RelativityApplication>()
				{
					Fields = FieldValue.NoFields
				};
				var results = RsapiClient.Repositories.RelativityApplication.Query(query);
				return results.Results.Any(x => x.Artifact.Guids.Contains(Constants.Guids.Application.DataGridCore));
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("Error checking if the DataGridCore Application is installed", ex);
			}
		}

		private bool CheckIfDataGridIsEnabled(int workspaceArtifactID)
		{
			try
			{
				var retVal = false;
				RsapiClient.APIOptions.WorkspaceID = -1;
				var workspace = RsapiClient.Repositories.Workspace.ReadSingle(workspaceArtifactID);
				if (workspace.EnableDataGrid == true)
				{
					retVal = true;
				}
				RsapiClient.APIOptions.WorkspaceID = workspaceArtifactID;
				return retVal;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("Error checking if the Data Grid enabled in workspace", ex);
			}
		}

		private bool CheckIfDataGridIsEnabledOnExtractedTextField()
		{
			try
			{
				var retVal = false;
				var sampleDoc = new Document();
				var nameCondition = new kCura.Relativity.Client.TextCondition(FieldFieldNames.Name, TextConditionEnum.EqualTo, DataGridFieldName);
				var objCondition = new kCura.Relativity.Client.ObjectCondition(FieldFieldNames.ObjectType, ObjectConditionEnum.EqualTo, sampleDoc.ArtifactTypeID.GetValueOrDefault());
				var fieldQuery = new kCura.Relativity.Client.DTOs.Query<kCura.Relativity.Client.DTOs.Field>()
				{
					Condition = new kCura.Relativity.Client.CompositeCondition(nameCondition, CompositeConditionEnum.And, objCondition),
					Fields = new List<FieldValue>() { new FieldValue(FieldFieldNames.EnableDataGrid) }
				};
				var result = RsapiClient.Repositories.Field.Query(fieldQuery);
				if (result.Success && result.Results.Any())
				{
					var extractedTextField = result.Results[0].Artifact;
					if (extractedTextField.EnableDataGrid.GetValueOrDefault())
					{
						retVal = true;
					}
				}

				return retVal;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("Error checking if the Data Grid enabled in workspace", ex);
			}
		}

		private string GenerateAuthToken()
		{
			try
			{
				RsapiClient.GenerateRelativityAuthenticationToken(RsapiClient.APIOptions);
				return RsapiClient.APIOptions.Token;
			}
			catch (Exception ex)
			{
				throw new Exception("Error Generating Auth Token", ex);
			}
		}
	}
}
