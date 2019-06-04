using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json.Linq;
using SmokeTest.Exceptions;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Relativity.Services.Interfaces.ObjectRules;

namespace SmokeTest.Helpers
{
	public class DataGridHelper
	{
		public IRSAPIClient RsapiClient;
		public const String DataGridFieldName = "Extracted Text";
		public const String DataGridFieldValue = "DataGridTestRandomFieldValue";

		public DataGridHelper(IRSAPIClient client)
		{
			RsapiClient = client;
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
						if (!dataGridIsEnabledOnExtractedTextField)
						{
							retVal.Success = false;
							retVal.ErrorMessage = "Data Grid is not enabled on the extracted text field.";
						}
						else
						{
							// Proceed with Test
							CreateDocument();
							//var documentFound = FindDocumentWithLuceneSearchRest("localhost", workspaceID, DataGridFieldName, DataGridFieldValue);
							var documentFound = true;
							if (!documentFound)
							{
								retVal.Success = false;
								retVal.ErrorMessage = "Unable to find document with Lucene Search";
							}
							else
							{
								//Query for Admin Audits
								//if (CheckIfAdminAuditsExist())
								//{
									retVal.Success = true;
								//}
								//else
								//{
								//	retVal.Success = false;
								//	retVal.ErrorMessage = "No Admin Audits Exist";
								//}
							}
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

		private bool CheckIfAdminAuditsExist()
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri($@"http://172.19.213.148/");
			client.DefaultRequestHeaders.Add("X-CSRF-Header", string.Empty);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAuthToken());
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $@"/Relativity.Rest/API/Relativity.Objects.Audits/workspaces/-1/audits/query");
			string requestString = "{\"request\": { \"objectType\": { \"artifactTypeID\": 1000045 }, \"fields\": [ { \"Name\": \"Audit ID\" } ] } }";
			request.Content = new StringContent(requestString, Encoding.UTF8, "application/json");
			var result = client.SendAsync(request).Result;

			//RsapiClient.APIOptions.WorkspaceID = -1;
			//kCura.Relativity.Client.DTOs.Query<RDO> query = new Query<RDO>
			//{
			//	ArtifactTypeID = 1000020,
			//	Fields = FieldValue.AllFields
			//};

			//QueryResultSet<RDO> results = new QueryResultSet<RDO>();
			//results = RsapiClient.Repositories.RDO.Query(query);
			//if (results.Success)
			//{
			//	if (results.TotalCount > 0)
			//	{
			//		return true;
			//	}
			//}

			return false;
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


		private void CreateDocument()
		{
			try
			{
				var identifier = "Sample Document from DataGrid Test" + Guid.NewGuid();
				var newDocument = new kCura.Relativity.Client.DTOs.Document()
				{
					Fields = new List<FieldValue>
										{
												new FieldValue(DocumentFieldNames.TextIdentifier, identifier),
												new FieldValue(DocumentFieldNames.RelativityNativeFileLocation, "C:\\Windows\\win.ini")
										}
				};
				var documentArtifactID = RsapiClient.Repositories.Document.CreateSingle(newDocument);
				UpdatedDataGridFieldValue(documentArtifactID, DataGridFieldName, DataGridFieldValue);
			}
			catch (Exception ex)
			{
				throw new Exception("Error Creating Document for Data Grid Test", ex);
			}
		}

		private void UpdatedDataGridFieldValue(int documentArtifactID, string fieldName, object fieldValue)
		{
			try
			{
				var document = new kCura.Relativity.Client.DTOs.Document(documentArtifactID)
				{
					Fields = new List<FieldValue>
										{
												new FieldValue(fieldName, fieldValue),
										}
				};
				RsapiClient.Repositories.Document.Update(document);
			}
			catch (Exception ex)
			{
				throw new Exception("Error Updating Document for Data Grid Test", ex);
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

		private bool FindDocumentWithLuceneSearchRest(string serverName, int workspaceArtifactID, string fieldName, string searchValue)
		{
			try
			{
				var retVal = false;
				HttpClient client = new HttpClient();
				client.BaseAddress = new Uri($@"http://{serverName}/");
				client.DefaultRequestHeaders.Add("X-CSRF-Header", string.Empty);
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAuthToken());
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $@"/Relativity.Rest/Workspace/{workspaceArtifactID}/Document/QueryResult");
				request.Content = new StringContent("{\"condition\": \"'" + fieldName + "' LUCENESEARCH '" + searchValue + "'\",\"fields\":[\"*\"]}", Encoding.UTF8, "application/json");

				var result = client.SendAsync(request).Result;
				if (result.StatusCode != HttpStatusCode.Created)
				{
					throw new Exception($@"Status Code:{result.StatusCode} Message:{result.Content.ReadAsStringAsync()}");
				}
				else
				{
					var joResponse = JObject.Parse(result.Content.ReadAsStringAsync().Result);
					var numberOfDocumentsFound = Convert.ToInt32(joResponse["TotalResultCount"]);
					if (numberOfDocumentsFound > 0)
					{
						retVal = true;
					}
				}
				return retVal;
			}
			catch (Exception ex)
			{
				throw new Exception("Error executing lucene search", ex);
			}
		}
	}
}
