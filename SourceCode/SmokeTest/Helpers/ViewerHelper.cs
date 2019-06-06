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
		public string RelativityUrl;

		public ViewerHelper(IRSAPIClient rsapiClient, IDocumentViewerServiceManager documentViewerServiceManager, IDBContext workspaceDbContext, string relativityUrl)
		{
			RsapiClient = rsapiClient;
			DocumentViewerServiceManager = documentViewerServiceManager;
			WorkspaceDbContext = workspaceDbContext;
			RelativityUrl = relativityUrl;
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
					long? previousMaxCacheEntryId = GetMaxCacheId(singleDocumentArtifactId);
					GetViewerContentKeyAsync(workspaceArtifactId, singleDocumentArtifactId).Wait();
					Thread.Sleep(30000); // Sleeping 30 seconds to allow for file to cache
					long? newMaxCacheEntryId = GetMaxCacheId(singleDocumentArtifactId);

					// Verify if the document has been converted without any errors
					bool isDocumentConversionSuccessful = VerifyIfDocumentConversionWasSuccessful(previousMaxCacheEntryId, newMaxCacheEntryId);
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

		private async Task GetViewerContentKeyAsync(int workspaceArtifactId, int documentArtifactId)
		{
			string errorContext = $"An error occured during document conversion. [{nameof(documentArtifactId)}: {documentArtifactId}]";

			try
			{
				HttpClient client = new HttpClient();
				client.BaseAddress = new Uri(RelativityUrl);
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
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private long? GetMaxCacheId(int documentId)
		{
			try
			{
				string sql = @"
										SELECT 
												MAX([CacheID]) 
										FROM 
												[EDDSDBO].[ConvertedCacheFile] WITH(NOLOCK)
										WHERE 
												[DocumentID] = @documentId";
				SqlParameter sqlParameter = new SqlParameter
				{
					ParameterName = "@documentId",
					SqlDbType = SqlDbType.Int,
					Value = documentId
				};

				try
				{
					long? cacheId = WorkspaceDbContext.ExecuteSqlStatementAsScalar<long?>(sql, sqlParameter);
					return cacheId;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured calling ExecuteSqlStatementAsScalar.", ex);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"An error occured getting the Max CacheId", ex);
			}
		}

		private bool VerifyIfDocumentConversionWasSuccessful(long? previousMaxCacheEntryId, long? newMaxCacheEntryId)
		{
			if (previousMaxCacheEntryId.HasValue && newMaxCacheEntryId.HasValue)
			{
				return newMaxCacheEntryId.Value > previousMaxCacheEntryId.Value;
			}

			if (newMaxCacheEntryId.HasValue)
			{
				return true;
			}

			return false;
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
