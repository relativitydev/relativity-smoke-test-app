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
using System.Linq;
using System.Threading;

namespace SmokeTest.Helpers
{
    public class ViewerHelper : IViewerHelper
    {
        public ResultModel ConvertDocumentsForViewer(IRSAPIClient rsapiClient, IDocumentViewerServiceManager documentViewerServiceManager, IDBContext workspaceDbContext, int workspaceArtifactId)
        {
            if (rsapiClient == null)
            {
                throw new ArgumentNullException(nameof(rsapiClient));
            }
            if (documentViewerServiceManager == null)
            {
                throw new ArgumentNullException(nameof(documentViewerServiceManager));
            }
            if (workspaceDbContext == null)
            {
                throw new ArgumentNullException(nameof(workspaceDbContext));
            }
            if (workspaceArtifactId < 1)
            {
                throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
            }

            string errorContext = "An error occured during document conversion.";
            ResultModel resultModel = new ResultModel("Viewer");

            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

                try
                {
                    // Convert Document
                    int singleDocumentArtifactId = GetSingleDocumentForConversion(rsapiClient, workspaceArtifactId);
                    ViewerContentKey viewerContentKey = GetViewerContentKeyAsync(documentViewerServiceManager, workspaceArtifactId, singleDocumentArtifactId);

                    // Verify if the document has been converted without any errors
                    long? cacheEntryId = viewerContentKey.CacheEntryId;

                    if (cacheEntryId.HasValue)
                    {
                        bool isDocumentConversionSuccessful = VerifyIfDocumentConversionWasSuccessful(workspaceDbContext, cacheEntryId.Value);
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
                    else
                    {
                        throw new SmokeTestException($"{errorContext}. CacheEntryId is NULL.");
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

        private int GetSingleDocumentForConversion(IRSAPIClient rsapiClient, int workspaceArtifactId)
        {
            string errorContext = "An error occured when getting single document for Conversion.";

            try
            {
                int allDocumentsSavedSearchArtifactId = SavedSearchHelper.GetSavedSearchArtifactId(rsapiClient, workspaceArtifactId, Constants.AllDocumentsSavedSearchName);
                List<int> savedSearchDocuments = SavedSearchHelper.QueryDocumentsInSavedSearch(rsapiClient, workspaceArtifactId, allDocumentsSavedSearchArtifactId);
                int singleDocumentArtifactId = savedSearchDocuments.Min();
                return singleDocumentArtifactId;
            }
            catch (Exception ex)
            {
                throw new SmokeTestException(errorContext, ex);
            }
        }

        private ViewerContentKey GetViewerContentKeyAsync(IDocumentViewerServiceManager documentViewerServiceManager, int workspaceArtifactId, int documentArtifactId)
        {
            string errorContext = $"An error occured during document conversion. [{nameof(documentArtifactId)}: {documentArtifactId}]";

            try
            {
                ViewerContentKey viewerContentKey = null;
                GetViewerContentKeyOptions options = new GetViewerContentKeyOptions
                {
                    ForceConversion = false
                };
                GetViewerContentKeyRequest request = new GetViewerContentKeyRequest
                {
                    WorkspaceId = workspaceArtifactId,
                    DocumentIds = new[] { documentArtifactId },
                    ConversionType = ConversionType.Image,
                    Priority = PriorityLevel.OnTheFly,
                    Options = options
                };

                try
                {
                    // Keep checking the value for 5 mins.
                    const int count = 10;
                    const int waitingSeconds = 1;

                    for (int i = 1; i <= count; i++)
                    {
                        viewerContentKey = (documentViewerServiceManager.GetViewerContentKeyAsync(request)).Result;
                        if (viewerContentKey.CacheEntryId != null)
                        {
                            return viewerContentKey;
                        }
                        Thread.Sleep(waitingSeconds * 30 * 1000);
                    }
                }
                catch (Exception ex)
                {
                    throw new SmokeTestException($"{errorContext}. GetViewerContentKeyAsync.", ex);
                }

                return viewerContentKey;
            }
            catch (Exception ex)
            {
                throw new SmokeTestException($"{errorContext}", ex);
            }
        }

        private bool VerifyIfDocumentConversionWasSuccessful(IDBContext workspaceDbContext, long cacheEntryId)
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
                    DataTable dataTable = workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
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
    }
}
