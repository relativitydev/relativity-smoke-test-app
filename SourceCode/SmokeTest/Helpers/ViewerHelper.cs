using kCura.Relativity.Client;
using Relativity.API;
using Relativity.DocumentViewer.Services;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmokeTest.Helpers
{
	public class ViewerHelper : IViewerHelper
	{
		public IRSAPIClient RsapiClient;
		public IDocumentViewerServiceManager DocumentViewerServiceManager;
		public IDBContext WorkspaceDbContext;

		public ViewerHelper(IRSAPIClient rsapiClient, IDocumentViewerServiceManager documentViewerServiceManager, IDBContext workspaceDbContext)
		{
			RsapiClient = rsapiClient;
			DocumentViewerServiceManager = documentViewerServiceManager;
			WorkspaceDbContext = workspaceDbContext;
		}

		public ResultModel ConvertDocumentsForViewer(int workspaceArtifactId)
		{
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
			}

			string errorContext = "An error occured during document conversion.";
			ResultModel resultModel = new ResultModel("Viewer");

			try
			{
				RsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

				try
				{
					// Convert Document
					int singleDocumentArtifactId = GetSingleDocumentForConversion(workspaceArtifactId);
					int cacheEntryId = GetViewerContentKeyAsync(workspaceArtifactId, singleDocumentArtifactId).Result;

					// Verify if the document has been converted without any errors
					//long? cacheEntryId = viewerContentKey.CacheEntryId;
					bool isDocumentConversionSuccessful = VerifyIfDocumentConversionWasSuccessful(cacheEntryId);
					if (isDocumentConversionSuccessful)
					{
						// Set resultModel properties
						resultModel.Success = true;
						resultModel.ArtifactId = singleDocumentArtifactId;
					}
					else
					{
						throw new SmokeTestException($"{errorContext}. Document Conversion not successful.");
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		private int GetSingleDocumentForConversion(int workspaceArtifactId)
		{
			string errorContext = "An error occured when getting single document for Conversion.";

			try
			{
				int allDocumentsSavedSearchArtifactId = SavedSearchHelper.GetSavedSearchArtifactId(RsapiClient, workspaceArtifactId, Constants.AllDocumentsSavedSearchName);
				List<int> savedSearchDocuments = SavedSearchHelper.QueryDocumentsInSavedSearch(RsapiClient, workspaceArtifactId, allDocumentsSavedSearchArtifactId);
				int singleDocumentArtifactId = savedSearchDocuments.Min();
				return singleDocumentArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException(errorContext, ex);
			}
		}

		private async Task<int> GetViewerContentKeyAsync(int workspaceArtifactId, int documentArtifactId)
		{
			string errorContext = $"An error occured during document conversion. [{nameof(documentArtifactId)}: {documentArtifactId}]";

			try
			{
				try
				{
					//JObject resultObject = await CallGetViewerContentKeyAsync(workspaceArtifactId, documentArtifactId);
					//IDictionary<string, JToken> dictionary = resultObject;
					//if (dictionary.ContainsKey("CacheEntryId"))
					//{
					//	cacheId = resultObject["CacheEntryId"].Value<int>();
					//	return cacheId
					//}
					// Keep checking the value for 5 mins.
					const int count = 10;
					const int waitingSeconds = 1;

					for (int i = 1; i <= count; i++)
					{
						JObject resultObject = await CallGetViewerContentKeyAsync(workspaceArtifactId, documentArtifactId);
						IDictionary<string, JToken> dictionary = resultObject;
						if (dictionary.ContainsKey("CacheEntryId"))
						{
							int cacheId = resultObject["CacheEntryId"].Value<int>();
							return cacheId;
						}
						Thread.Sleep(waitingSeconds * 30 * 1000);
					}

					throw new Exception("Error getting CacheEntryId");
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. GetViewerContentKeyAsync.", ex);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private async Task<JObject> CallGetViewerContentKeyAsync(int workspaceArtifactId, int documentArtifactId)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri($@"http://172.19.213.148/Relativity");
			client.DefaultRequestHeaders.Add("X-CSRF-Header", string.Empty);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAuthToken());
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			ViewerContentKey viewerContentKey = null;
			GetViewerContentKeyOptions options = new GetViewerContentKeyOptions
			{
				ForceConversion = true,
				ClientId = "DocumentViewer.Conversion.PreConvert",
			};
			GetViewerContentKeyRequest parameters = new GetViewerContentKeyRequest
			{
				WorkspaceId = workspaceArtifactId,
				DocumentIds = new[] { documentArtifactId },
				ConversionType = ConversionType.Image,
				Priority = PriorityLevel.OnTheFly,
				Options = options,
			};

			dynamic requestParameters = new ExpandoObject();
			requestParameters.request = parameters;
			string jsonParameters = JsonConvert.SerializeObject(requestParameters);

			StringContent parameterStringContent = new StringContent(jsonParameters, System.Text.Encoding.UTF8, "application/json");

			//Make the HTTP request.
			String dvsUrl = "Relativity.REST/api/Relativity.DocumentViewer.Services.IDocumentViewerServiceModule/DocumentViewerServiceManager/GetViewerContentKeyAsync";
			HttpResponseMessage response = await client.PostAsync(dvsUrl, parameterStringContent);
			bool success = response.StatusCode == HttpStatusCode.OK;
			if (!success)
			{
				throw new Exception("An error occured Getting Viewer Content Key");
			}
			string result = response.Content.ReadAsStringAsync().Result;
			JObject resultObject = JObject.Parse(result);
			return resultObject;
		}

		private bool VerifyIfDocumentConversionWasSuccessful(long cacheEntryId)
		{
			string errorContext = $"An error occured when checking Document Conversion status in ConvertedCacheFile table. [{nameof(cacheEntryId)}: {cacheEntryId}]";

			try
			{
				string sql = @"
										SELECT 
												TOP 1 [ErrorID] 
										FROM 
												[EDDSDBO].[ConvertedCacheFile] WITH(NOLOCK) 
										WHERE 
												[CacheID] = @cacheEntryId";

				List<SqlParameter> sqlParams = new List<SqlParameter>
								{
										new SqlParameter("@cacheEntryId", SqlDbType.BigInt) {Value = cacheEntryId}
								};

				try
				{
					DataTable dataTable = WorkspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
					if (dataTable != null)
					{
						object errorId = dataTable.Rows[0][0];
						if (errorId == DBNull.Value)
						{
							return true;
						}
					}
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. ExecuteSqlStatementAsScalar.", ex);
				}

				return false;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
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
