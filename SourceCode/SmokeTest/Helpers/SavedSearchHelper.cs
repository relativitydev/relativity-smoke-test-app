using kCura.Relativity.Client;
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
    }
}
