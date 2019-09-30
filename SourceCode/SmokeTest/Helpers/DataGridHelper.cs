using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Relativity.Compute.dtSearch.Services.Interfaces;
using Relativity.Compute.dtSearch.Services.Interfaces.Models;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.SearchIndex;
using Relativity.Services.User;
using SmokeTest.Exceptions;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SmokeTest.Helpers
{
	public class DataGridHelper
	{
		public IRSAPIClient RsapiClient;
		public IKeywordSearchManager KeywordSearchManager;
		public IdtSearchManager DtSearchManager;
		public IDtSearchIndexManager DtSearchIndexManager;
		public string RelativityUrl;
		public const String DataGridFieldName = "Extracted Text";
		public const String DataGridFieldValue = "DataGridTestRandomFieldValue";

		public DataGridHelper(IRSAPIClient client, IKeywordSearchManager keywordSearchManager, IdtSearchManager dtSearchManager, IDtSearchIndexManager dtSearchIndexManager, string relativityUrl)
		{

			RsapiClient = client ?? throw new ArgumentNullException(nameof(client));
			KeywordSearchManager = keywordSearchManager ?? throw new ArgumentNullException(nameof(keywordSearchManager));
			DtSearchManager = dtSearchManager ?? throw new ArgumentNullException(nameof(dtSearchManager));
			DtSearchIndexManager = dtSearchIndexManager ?? throw new ArgumentNullException(nameof(dtSearchIndexManager));
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
						if (!dataGridIsEnabledOnExtractedTextField)
						{
							retVal.Success = false;
							retVal.ErrorMessage = "Data Grid is not enabled on the extracted text field.";
						}
						else
						{
							// Create Keyword Search
							int keywordSearchArtifactId = CreateKeywordSearch(workspaceID).Result;

							// Create dtSearch Index
							int dtSearchIndexArtifactId = CreateDtSearchIndex(workspaceID, keywordSearchArtifactId).Result;

							// Build dtSearch Index
							BuildDtSearchIndex(workspaceID, dtSearchIndexArtifactId).Wait();
							WaitForDtSearchIndexToBeActive(workspaceID, dtSearchIndexArtifactId).Wait();

							// Create dtSearch
							int dtSearchId = CreateDtSearch(workspaceID).Result;

							List<int> documents = GetDocumentsFromSearch(dtSearchId, workspaceID).Result;

							// Delete Keyword Search, dtSearch, and dtSearchIndex
							DeleteKeywordSearch(workspaceID, keywordSearchArtifactId).Wait();
							DeleteDtSearch(workspaceID, dtSearchId).Wait();
							DeleteDtSearchIndex(workspaceID, dtSearchIndexArtifactId).Wait();

							if (documents.Count > 0)
							{
								retVal.Success = true;

								//Commenting the code to check for admin audits. Not sure if this is required to verify dtSearch is working 9/30/2019 (Chandra)
								////Check that admin audits exist
								//if (CheckIfAdminAuditsExist())
								//{
								//	retVal.Success = true;
								//}
								//else
								//{
								//	retVal.Success = false;
								//	retVal.ErrorMessage = "Admin Audits do not exist";
								//}
							}
							else
							{
								retVal.Success = false;
								retVal.ErrorMessage = "dtSearch failed to return documents.";
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

		private async Task<int> CreateKeywordSearch(int workspaceId)
		{
			try
			{
				SearchContainerRef searchFolder = new SearchContainerRef();

				KeywordSearch keywordSearch = new KeywordSearch
				{
					Name = "Smoke Test - KeywordSearch",
					SearchContainer = searchFolder
				};

				// Get all the query fields available to the current user.
				SearchResultViewFields searchResultViewFields = await KeywordSearchManager.GetFieldsForSearchResultViewAsync(workspaceId, 10);

				// Set the owner to the current user, in this case "Admin, Relativity," or "0" for public.
				List<UserRef> searchOwners = await KeywordSearchManager.GetSearchOwnersAsync(workspaceId);
				keywordSearch.Owner = searchOwners.First(o => o.Name == "Public");

				// Add the fields to the Fields collection.
				// If a field Name, ArtifactID, Guid, or ViewFieldID is known, a field can be set with that information as well.

				FieldRef fieldRef = searchResultViewFields.FieldsNotIncluded.First(f => f.Name == "Edit");
				keywordSearch.Fields.Add(fieldRef);

				fieldRef = searchResultViewFields.FieldsNotIncluded.First(f => f.Name == "File Icon");
				keywordSearch.Fields.Add(fieldRef);

				fieldRef = searchResultViewFields.FieldsNotIncluded.First(f => f.Name == "Control Number");
				keywordSearch.Fields.Add(fieldRef);

				// Create a Criteria for the field named "Extracted Text" where the value is set

				Criteria criteria = new Criteria
				{
					Condition = new CriteriaCondition(
						new FieldRef
						{
							Name = DataGridFieldName
						}, CriteriaConditionEnum.IsSet)
				};

				// Add the search condition criteria to the collection.
				keywordSearch.SearchCriteria.Conditions.Add(criteria);

				// Add a note.
				keywordSearch.Notes = "Smoke Test - Keyword Search";
				keywordSearch.ArtifactTypeID = 10;

				// Create the search.
				int keywordSearchArtifactId = await KeywordSearchManager.CreateSingleAsync(workspaceId, keywordSearch);

				if (keywordSearchArtifactId == 0)
				{
					throw new Exception("Failed to create the Keyword Search");
				}

				return keywordSearchArtifactId;
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when creating Keyword Search", ex);
			}
		}

		private async Task DeleteKeywordSearch(int workspaceId, int keywordSearchArtifactId)
		{
			try
			{
				await KeywordSearchManager.DeleteSingleAsync(workspaceId, keywordSearchArtifactId);
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when deleting Keyword Search", ex);
			}
		}

		private async Task<int> CreateDtSearchIndex(int workspaceId, int keywordSearchArtifactId)
		{
			try
			{
				int indexShareCodeArtifactId = DtSearchIndexManager.GetIndexShareLocationAsync(workspaceId).Result[0].ID;

				DtSearchIndexRequest dtSearchIndexRequest = new DtSearchIndexRequest
				{
					Name = "Smoke Test - DtSearch Index",
					Order = 10,
					SearchSearchID = keywordSearchArtifactId,
					RecognizeDates = true,
					SkipNumericValues = true,
					IndexShareCodeArtifactID = indexShareCodeArtifactId,
					EmailAddress = "",
					NoiseWords = "",
					AlphabetText = "",
					DirtySettings = "",
					SubIndexSize = 250000,
					FragmentationThreshold = 9,
					Priority = 9
				};

				int dtSearchIndexArtifactId = await DtSearchIndexManager.CreateAsync(workspaceId, dtSearchIndexRequest);

				if (dtSearchIndexArtifactId == 0)
				{
					throw new Exception("Failed to create the DtSearch Index");
				}

				return dtSearchIndexArtifactId;
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when creating DtSearch Index", ex);
			}
		}

		private async Task DeleteDtSearchIndex(int workspaceId, int dtSearchIndexArtifactId)
		{
			try
			{
				await DtSearchIndexManager.DeleteAsync(workspaceId, dtSearchIndexArtifactId);
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when deleting DtSearch Index");
			}
		}

		private async Task BuildDtSearchIndex(int workspaceID, int dtSearchIndexArtifactId)
		{
			try
			{
				await DtSearchIndexManager.FullBuildIndexAsync(workspaceID, dtSearchIndexArtifactId, true);
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when building DtSearch Index", ex);
			}
		}

		private async Task WaitForDtSearchIndexToBeActive(int workspaceID, int dtSearchIndexArtifactId)
		{
			try
			{
				const int count = 60;
				const int waitingSeconds = 1;
				int i = 0;
				while (i < count)
				{
					DtSearchIndexStatus dtSearchIndexStatus = await DtSearchIndexManager.GetIndexIDAndStatusAsync(workspaceID, dtSearchIndexArtifactId);
					if (dtSearchIndexStatus.Status == "Indexed")
					{
						break;
					}
					Thread.Sleep(waitingSeconds * 5 * 1000);
					i++;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when waiting for the DtSearch Index to be Active");
			}
		}

		private async Task<int> CreateDtSearch(int workspaceID)
		{
			try
			{
				string searchName = "Smoke Test - dtSearch";
				SearchContainerRef searchContainerRef = new SearchContainerRef();
				dtSearch dtSearch = new dtSearch
				{
					Name = searchName,
					SearchContainer = searchContainerRef
				};

				// Get all the query fields available to the current user.
				SearchResultViewFields searchResultViewFields = await DtSearchManager.GetFieldsForSearchResultViewAsync(workspaceID, 10);

				// Get a dtSearch SearchIndex and set it.
				List<SearchIndexRef> searchIndexes = await DtSearchManager.GetSearchIndexesAsync(workspaceID);
				dtSearch.SearchIndex = searchIndexes.FirstOrDefault();

				// Set the owner to "Public".
				List<UserRef> searchOwners = await DtSearchManager.GetSearchOwnersAsync(workspaceID);

				dtSearch.Owner = searchOwners.First(o => o.Name == "Public");


				FieldRef field = searchResultViewFields.FieldsNotIncluded.First(f => f.Name == "Edit");
				dtSearch.Fields.Add(field);

				field = searchResultViewFields.FieldsNotIncluded.First(f => f.Name == "File Icon");
				dtSearch.Fields.Add(field);

				field = searchResultViewFields.FieldsNotIncluded.First(f => f.Name == "Control Number");
				dtSearch.Fields.Add(field);

				// Create a Criteria for the field named "Extracted Text" where the value is set

				Criteria criteria = new Criteria
				{
					Condition = new CriteriaCondition(new FieldRef
					{
						Name = DataGridFieldName
					}, CriteriaConditionEnum.IsSet)
				};

				// Add the search condition criteria to the collection.
				dtSearch.SearchCriteria.Conditions.Add(criteria);

				// Add a note.
				dtSearch.Notes = "Smoke Test - dtSearch Search";
				dtSearch.ArtifactTypeID = 10;

				// Create the search.
				int dtSearchArtifactId = await DtSearchManager.CreateSingleAsync(workspaceID, dtSearch);

				if (dtSearchArtifactId == 0)
				{
					throw new Exception("Failed to create the DtSearch");
				}

				return dtSearchArtifactId;
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when creating DtSearch", ex);
			}
		}

		private async Task DeleteDtSearch(int workspaceId, int dtSearchId)
		{
			try
			{
				await DtSearchManager.DeleteSingleAsync(workspaceId, dtSearchId);
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured deleting a DtSearch");
			}
		}

		private async Task<List<int>> GetDocumentsFromSearch(int dtSearchId, int workspaceID)
		{
			RsapiClient.APIOptions.WorkspaceID = workspaceID;
			List<int> documentsToTag = new List<int>();

			Query<Document> documentQuery = new Query<Document>
			{
				Condition = new SavedSearchCondition(dtSearchId),
				Fields = FieldValue.AllFields
			};

			ResultSet<Document> docResults = await Task.Run(() => RsapiClient.Repositories.Document.Query(documentQuery));
			foreach (Result<Document> singleDoc in docResults.Results)
			{
				// This loop will add the artifactID of each document that met our search Criteria and as such should be tagged responsive or w/e
				documentsToTag.Add(singleDoc.Artifact.ArtifactID);
			}

			return documentsToTag;
		}

		private bool CheckIfAdminAuditsExist()
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(RelativityUrl);
			client.DefaultRequestHeaders.Add("X-CSRF-Header", string.Empty);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAuthToken());
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			string url = $@"/Relativity.Rest/API/Relativity.Objects.Audits/workspaces/-1/audits/query";
			AuditQueryRequestModel auditQueryRequestModel = new AuditQueryRequestModel
			{
				request = new Request
				{
					objectType = new Objecttype
					{
						artifactTypeID = 0
					},
					fields = new[]
					{
						new Models.Field()
						{
							Name = "Action",
							Guids = {},
							ArtifactID = 0
						},
					},
					condition = "",
					rowCondition = "",
					sorts = { },
					relationalField = null,
					searchProviderCondition = null,
					includeIdWindow = true,
					convertNumberFieldValuesToString = true,
					isAdHocQuery = false,
					activeArtifactId = null,
					queryHint = null,
					executingViewId = 0
				},
				start = 1,
				length = 25
			};
			string requestString = JsonConvert.SerializeObject(auditQueryRequestModel);

			StringContent content = new StringContent(requestString);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			HttpResponseMessage response = client.PostAsync(url, content).Result;
			bool success = response.StatusCode == HttpStatusCode.OK;
			if (!success)
			{
				throw new Exception("An error occured querying for Admin Audits");
			}

			string result = response.Content.ReadAsStringAsync().Result;
			JObject resultObject = JObject.Parse(result);
			bool doAdminAuditsExist = resultObject["TotalCount"].Value<int>() > 0;
			return doAdminAuditsExist;
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
