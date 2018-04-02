using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Search;
using SmokeTest.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmokeTest.Helpers
{
    public class SavedSearchHelper
    {
        public static int CreateSavedSearchWithControlNumbers(IKeywordSearchManager keywordSearchManager, IRSAPIClient rsapiClient, int workspaceArtifactId, int documentIdentifierFieldArtifactId, string searchName, List<string> controlNumbers)
        {
            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
                Task<SearchResultViewFields> resultFields = keywordSearchManager.GetFieldsForSearchResultViewAsync(rsapiClient.APIOptions.WorkspaceID, (int)ArtifactType.Document);
                KeywordSearch search = new KeywordSearch
                {
                    Name = searchName,
                    ArtifactTypeID = (int)ArtifactType.Document
                };
                search.Fields.Add(resultFields.Result.FieldsNotIncluded.First(f => f.ArtifactID == documentIdentifierFieldArtifactId));
                int savedSearchArtifactId = keywordSearchManager.CreateSingleAsync(rsapiClient.APIOptions.WorkspaceID, search).Result;
                return savedSearchArtifactId;
            }
            catch (Exception ex)
            {
                throw new SmokeTestException("An error occured when creating saved search.", ex);
            }
        }

        public static int GetSavedSearchArtifactId(IRSAPIClient rsapiClient, int workspaceArtifactId, string savedSearchName)
        {
            string errorContext = $"An error occured when querying for '{savedSearchName}' Saved Search.";
            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

                Query query = new Query
                {
                    ArtifactTypeID = (int)ArtifactType.Search,
                    Condition = new TextCondition("Name", TextConditionEnum.Like, savedSearchName)
                };
                QueryResult savedSearchQueryResult;

                try
                {
                    savedSearchQueryResult = rsapiClient.Query(rsapiClient.APIOptions, query);
                }
                catch (Exception ex)
                {
                    throw new SmokeTestException($"{errorContext}. Query.", ex);
                }

                if (!savedSearchQueryResult.Success)
                {
                    throw new SmokeTestException($"{errorContext}. ErrorMessage = {savedSearchQueryResult.Message}");
                }
                int savedSearchArtifactId = savedSearchQueryResult.QueryArtifacts[0].ArtifactID;
                return savedSearchArtifactId;
            }
            catch (Exception ex)
            {
                throw new SmokeTestException(errorContext, ex);
            }
        }

        public static List<int> QueryDocumentsInSavedSearch(IRSAPIClient rsapiClient, int workspaceArtifactId, int savedSearchArtifactId)
        {
            rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

            string errorContext = $"An error occured when querying for documents in Saved Search. [SavedSearchArtifactId: {savedSearchArtifactId}]";
            List<int> retVal = new List<int>();

            try
            {
                Query<Document> documentQuery = new Query<Document>
                {
                    Condition = new SavedSearchCondition(savedSearchArtifactId),
                    Fields = FieldValue.SelectedFields
                };
                QueryResultSet<Document> documentQueryResultSet;

                try
                {
                    documentQueryResultSet = rsapiClient.Repositories.Document.Query(documentQuery);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while querying documents.", ex);
                }

                if (documentQueryResultSet.Success)
                {
                    retVal.AddRange(documentQueryResultSet.Results.Select(result => result.Artifact.ArtifactID));
                }
                else
                {
                    throw new Exception("Unable to query documents");
                }
            }
            catch (Exception ex)
            {
                throw new SmokeTestException(errorContext, ex);
            }

            return retVal;
        }

        public static bool DocumentsExistInWorkspace(IRSAPIClient rsapiClient, int workspaceArtifactId)
        {
            bool retVal = false;
            rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

            try
            {
                Query<Document> documentQuery = new Query<Document>
                {
                    Fields = FieldValue.NoFields
                };
                QueryResultSet<Document> documentQueryResultSet;

                try
                {
                    documentQueryResultSet = rsapiClient.Repositories.Document.Query(documentQuery);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while querying documents.", ex);
                }

                if (documentQueryResultSet.Success && documentQueryResultSet.TotalCount > 0)
                {
                    retVal = true;
                }
                else if (documentQueryResultSet.Success == false)
                {
                    throw new Exception($@"Error Querying documents: {documentQueryResultSet.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new SmokeTestException("Error while checking if documents exist in workspace", ex);
            }

            return retVal;
        }
    }
}
