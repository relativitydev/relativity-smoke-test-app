using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Processing.Services;
using Relativity.Services;
using Relativity.Services.ResourcePool;
using SmokeTest.Exceptions;
using SmokeTest.Models;
using RelativityApplication = kCura.Relativity.Client.DTOs.RelativityApplication;

namespace SmokeTest.Helpers
{
    public class ProcessingHelper
    {
        public IRSAPIClient RsapiClient;
        public IProcessingSetManager ProcessingSetManager;
        public IProcessingCustodianManager ProcessingCustodianManager;
        public IProcessingDataSourceManager ProcessingDataSourceManager;
        public IResourcePoolManager ResourcePoolMananger;
        public IProcessingJobManager ProcessingJobManager;

        public ProcessingHelper(IRSAPIClient client, IProcessingCustodianManager processingCustodianManager, IProcessingSetManager processingSetManager, IProcessingDataSourceManager processingDataSourceManager, IResourcePoolManager resourcePoolMananger, IProcessingJobManager processingJobManager)
        {
            RsapiClient = client;
            ProcessingCustodianManager = processingCustodianManager;
            ProcessingSetManager = processingSetManager;
            ProcessingDataSourceManager = processingDataSourceManager;
            ResourcePoolMananger = resourcePoolMananger;
            ProcessingJobManager = processingJobManager;
        }

        public ResultModel CreateAndRunProcessingSet(int workspaceID)
        {
            var retVal = new ResultModel("ProcessingSet");

            try
            {
                var processingApplicationIsInstalled = CheckIfProcessingIsInstalled();

                if (!processingApplicationIsInstalled)
                {
                    retVal.Success = false;
                    retVal.ErrorMessage = "Processing is not installed in this workspace";
                }
                else
                {
                    // Create Custodian
                    var custodianArtifactID = GetProcessingCustodian(workspaceID);

                    if (custodianArtifactID < 0)
                    {
                        retVal.Success = false;
                        retVal.ErrorMessage = "Unable to Create or query Custodians";
                    }
                    else
                    {
                        // Get the ArtifactID of the Processing Profile
                        var processingProfileArtifactID = GetDefaultProcessingProfileArtifactId(workspaceID);

                        // Disable Nist on the Processing Profile
                        DisableNist(workspaceID, processingProfileArtifactID);

                        // Get a random timezone ArtifactID
                        var timezoneArtifactID = GetTimeZoneArtifactId(workspaceID);

                        // Get the document folder destination ArtifactID
                        var folderArtifactID = GetDefaultFolderArtifactId(workspaceID);

                        // Create Processing Set
                        var processingSetArtifactID = CreateProcessingSet(workspaceID, processingProfileArtifactID);

                        // Get Processing Source Location Path from the choice selected in the default resource pool
                        var proSourceLocationPath = GetProcessingSourceLocationFromResourcePool();

                        // Create random text files in the Processing  Source Location so that something is availble to import into the workspace
                        CreateRandomTextFiles(proSourceLocationPath, Constants.NumberOfTextFilesToCreate);

                        // Create Processing Data Source
                        CreateProcessingDataSource(workspaceID, custodianArtifactID, processingSetArtifactID, timezoneArtifactID, folderArtifactID, proSourceLocationPath);

                        // Submit Discovery Job and wait 5 minutes for it to complete
                        SubmitDiscoveryJob(workspaceID, processingSetArtifactID);
                        var discoverySuccessful = JobCompletedSuccessfully(workspaceID, processingSetArtifactID, 5, Constants.ProcessingJobType.Discover);

                        if (discoverySuccessful == false)
                        {
                            retVal.ErrorMessage = "Timed out while waiting for discovery job to complete";
                            retVal.Success = false;
                        }
                        else
                        {
                            // Submit Publish Job and wait 5 minutes for it to complete
                            SubmitPublishJob(workspaceID, processingSetArtifactID);
                            var publishSuccessful = JobCompletedSuccessfully(workspaceID, processingSetArtifactID, 5, Constants.ProcessingJobType.Publish);
                            if (publishSuccessful == false)
                            {
                                retVal.ErrorMessage = "Timed out while waiting for publish job to complete";
                                retVal.Success = false;
                            }
                            else
                            {
                                retVal.Success = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retVal.ErrorMessage = $@"Error running Processing Set Test: {ex.ToString()}";
                retVal.Success = false;
            }
            
            return retVal;
        }

        public int GetProcessingCustodian(int workspaceId)
        {
            var retVal = -1;
            try
            {
                var availableCustodians = QueryProcessingCustodians(workspaceId);
                if (availableCustodians.Results.Any())
                {
                    retVal = availableCustodians.Results[0].Artifact.ArtifactID;
                }
                else
                {
                    // Create Custodian if none exists
                    ProcessingCustodian processingCustodian = new ProcessingCustodian()
                    {
                        ArtifactID = 0,
                        FirstName = "Data",
                        LastName = "Grid",
                        DocumentNumberingPrefix = "REL"
                    };
                    retVal = ProcessingCustodianManager.SaveAsync(processingCustodian, workspaceId).Result;
                }
            }
            catch (Exception e)
            {
                throw new SmokeTestException("Error creating/querying Custodians", e);
            }
            

            return retVal;
        }

        private kCura.Relativity.Client.DTOs.QueryResultSet<RDO> QueryProcessingCustodians(int workspaceId)
        {
            var query = new kCura.Relativity.Client.DTOs.Query<RDO>()
            {
                ArtifactTypeGuid = Constants.Guids.ObjectType.Custodian,
                Fields = FieldValue.NoFields
            };

            return RsapiClient.Repositories.RDO.Query(query, 1);
        }

        private bool CheckIfProcessingIsInstalled()
        {
            try
            {
                var query = new kCura.Relativity.Client.DTOs.Query<RelativityApplication>()
                {
                    Fields = FieldValue.NoFields
                };
                var results = RsapiClient.Repositories.RelativityApplication.Query(query);
                return results.Results.Any(x => x.Artifact.Guids.Contains(Constants.Guids.Application.Processing));
            }
            catch (Exception ex)
            {
                throw new SmokeTestException("Error checking if the Processing Application is installed", ex);
            }
        }

        private int GetDefaultProcessingProfileArtifactId(int workspaceId)
        {
            try
            {
                var query = new kCura.Relativity.Client.DTOs.Query<kCura.Relativity.Client.DTOs.RDO>()
                {
                    ArtifactTypeGuid = Constants.Guids.ObjectType.ProcessingProfile,
                    Fields = FieldValue.NoFields
                };
                var results = RsapiClient.Repositories.RDO.Query(query);
                return results.Results[0].Artifact.ArtifactID;
            }
            catch (Exception ex)
            {
                throw new SmokeTestException("Error getting Default Processing Profile ArtifactID", ex);
            }
        }

        private int GetTimeZoneArtifactId(int workspaceId)
        {
            try
            {
                var query = new kCura.Relativity.Client.DTOs.Query<kCura.Relativity.Client.DTOs.RDO>()
                {
                    ArtifactTypeGuid = Constants.Guids.ObjectType.RelaticityTimeZone,
                    Fields = FieldValue.NoFields
                };
                var results = RsapiClient.Repositories.RDO.Query(query);
                if (!results.Success)
                {
                    throw new Exception(results.Message);
                }
                return results.Results[0].Artifact.ArtifactID;
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error getting Time Zone ArtifactID {e.ToString()}");
            }
            
        }

        private int GetDefaultFolderArtifactId(int workspaceId)
        {
            try
            {
                var query = new kCura.Relativity.Client.DTOs.Query<kCura.Relativity.Client.DTOs.RDO>()
                {
                    ArtifactTypeID = (int?)ArtifactType.Folder,
                    Fields = FieldValue.NoFields
                };
                var results = RsapiClient.Repositories.RDO.Query(query);
                if (!results.Success)
                {
                    throw new Exception(results.Message);
                }
                return results.Results[0].Artifact.ArtifactID;
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Unable to get the default folder artifactID: {e.ToString()}");
            }
            
        }

        private void DisableNist(int workspaceId, int processingProfileId)
        {
            try
            {
                var profileRdo = new kCura.Relativity.Client.DTOs.RDO(processingProfileId)
                {
                    ArtifactTypeGuids = new List<Guid>() { Constants.Guids.ObjectType.ProcessingProfile },
                    Fields = new List<FieldValue>()
                    {
                        new FieldValue(Constants.Guids.Fields.ProcessingProfile.DeNist) { ValueAsYesNo = false}
                    }
                };
                var results = RsapiClient.Repositories.RDO.Update(profileRdo);
                if (!results.Success)
                {
                    throw new SmokeTestException(results.Message);
                }
            }
            catch (Exception ex)
            {
                throw new SmokeTestException($@"Error Disabling Nist on the default provcessing profile in workspace: {workspaceId}: {ex.ToString()}");
            }
        }

        private int CreateProcessingSet(int workspaceId, int processingProfileId)
        {
            try
            {
                ProcessingSet processingSet = new ProcessingSet()
                {
                    ArtifactID = 0,
                    Name = "Processing Set 1",
                    Profile = new ProcessingProfileRef(processingProfileId)
                };

                return ProcessingSetManager.SaveAsync(processingSet, workspaceId).Result;
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error creating procssing set: {e.ToString()}");
            }
        }

        private string GetProcessingSourceLocationFromResourcePool()
        {
            try
            {
                var condition = new Relativity.Services.TextCondition("Name", Relativity.Services.TextConditionEnum.StartsWith, "Default");
                var query = new Relativity.Services.Query()
                {
                    Condition = condition.ToQueryString()
                };
                var resourcePool = ResourcePoolMananger.QueryAsync(query).Result;
                var results = ResourcePoolMananger.GetProcessingSourceLocationsAsync(new ResourcePoolRef(resourcePool.Results[0].Artifact.ArtifactID)).Result;
                return results[0].Name;
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error finding Processing Source Location: {e.ToString()}");
            }
            
        }

        private void CreateRandomTextFiles(string path, int numberOfFilesToCreate)
        {
            try
            {
                for (var i = 0; i < numberOfFilesToCreate; i++)
                {
                    var filename = Guid.NewGuid() + ".txt";
                    var fullPath = Path.Combine(path, filename);
                    var contents = "file contents: " + filename;
                    System.IO.File.WriteAllText(fullPath, contents);
                }
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error Creating random text files for procssing {e.ToString()}");
            }
            
        }


        private int CreateProcessingDataSource(int workspaceId, int custodianId, int processingSetId, int timeZoneId, int destinationFolderArtifactId, string processingSource)
        {
            try
            {
                ProcessingDataSource processingDataSource = new ProcessingDataSource()
                {
                    ArtifactID = 0,
                    Custodian = custodianId,
                    ProcessingSet = new ProcessingSetRef(processingSetId),
                    InputPath = processingSource,
                    OcrLanguages = new[] { OcrLanguage.English, },
                    TimeZone = timeZoneId,
                    DestinationFolder = destinationFolderArtifactId,
                    DocumentNumberingPrefix = "REL",
                    Name = "Processing Set 1"
                };

                return ProcessingDataSourceManager.SaveAsync(processingDataSource, workspaceId).Result;
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error creating Procssing Data Source: {e.ToString()}");
            }
            
        }

        private void SubmitDiscoveryJob(int workspaceId, int processingSetId)
        {
            try
            {
                DiscoveryJob discoveryJob = new DiscoveryJob()
                {
                    ProcessingSetId = processingSetId,
                    WorkspaceArtifactId = workspaceId
                };

                ProcessingJobManager.SubmitDiscoveryJobsAsync(discoveryJob);
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error Submitting Discovery Job: {e.ToString()}");
            }
            
        }

        private bool JobCompletedSuccessfully(int workspaceId, int processingSetId, int maxWaitInMinutes, Constants.ProcessingJobType jobtype)
        {
            var jobComplete = false;
            var maxTime = (maxWaitInMinutes * 60 * 1000);
            var currentWait = 0;
            var interval = 30 * 1000;

            Guid fieldGuid;
            if (jobtype == Constants.ProcessingJobType.Discover)
            {
                fieldGuid = Constants.Guids.Fields.ProcessingSet.Discover_Status;
            }
            else
            {
                fieldGuid = Constants.Guids.Fields.ProcessingSet.Publish_Status;
            }

            try
            {
                while (currentWait < maxTime && jobComplete == false)
                {
                    Thread.Sleep(interval);
                    var job = RsapiClient.Repositories.RDO.ReadSingle(processingSetId);
                    jobComplete = job[fieldGuid].ValueAsSingleChoice.Name.Contains("Completed");
                    currentWait += interval;
                }

                return jobComplete;
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error Checking for {jobtype.ToString()} Job Completion: {e.ToString()}");
            }
            
        }

        private void SubmitPublishJob(int workspaceId, int processingSetId)
        {
            try
            {
                PublishJob publishJob = new PublishJob()
                {
                    ProcessingSetId = processingSetId,
                    WorkspaceArtifactId = workspaceId
                };

                ProcessingJobManager.SubmitPublishJobsAsync(publishJob);
            }
            catch (Exception e)
            {
                throw new SmokeTestException($@"Error checking submitting publish job: {e.ToString()}");
            }
            
        }
    }
}
