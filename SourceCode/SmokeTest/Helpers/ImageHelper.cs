using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SmokeTest.Helpers
{
    public class ImageHelper : IImageHelper
    {
        public ResultModel ImageDocuments(IRSAPIClient rsapiClient, int workspaceArtifactId)
        {
            if (rsapiClient == null)
            {
                throw new ArgumentNullException(nameof(rsapiClient));
            }
            if (workspaceArtifactId < 1)
            {
                throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
            }

            ResultModel resultModel = new ResultModel("Image");

            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

                try
                {
                    // Create Imaging Set record
                    string imagingSetName = $"{Constants.Prefix} - Imaging Set - {DateTime.Now}";

                    int imagingSetArtifactId = CreateImagingSetRecord(rsapiClient, workspaceArtifactId, imagingSetName, Constants.BasicDefaultimagingProfileName, Constants.AllDocumentsSavedSearchName);

                    // Create Imaging Set Scheduler record
                    string imagingSetSchedulerName = $"{Constants.Prefix} - Imaging Set Scheduler - {DateTime.Now}";
                    int imagingSetSchedulerArtifactId = CreateImagingSetSchedulerRecord(rsapiClient, workspaceArtifactId, imagingSetSchedulerName, imagingSetArtifactId);

                    // Wait to check if documents are imaged
                    bool areDocumentsImaged = CheckIfImagingSetIsCompleted(rsapiClient, workspaceArtifactId, imagingSetArtifactId);
                    if (!areDocumentsImaged)
                    {
                        throw new SmokeTestException("Documents were not successfully imaged. Imaging Set did not Complete.");
                    }

                    // Set resultModel properties
                    resultModel.Success = true;
                    resultModel.ArtifactId = imagingSetSchedulerArtifactId;
                }
                catch (Exception ex)
                {
                    throw new SmokeTestException("An error occured when imaging documents.", ex);
                }
            }
            catch (Exception ex)
            {
                resultModel.Success = false;
                resultModel.ErrorMessage = ex.ToString();
            }

            return resultModel;
        }

        private int GetImagingProfileArtifactId(IRSAPIClient rsapiClient, int workspaceArtifactId, string imagingProfileName)
        {
            string errorContext = $"An error occured when querying for '{imagingProfileName}' Imaging Profile.";
            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

                TextCondition nameTextCondition = new TextCondition(ArtifactFieldNames.TextIdentifier, TextConditionEnum.EqualTo, "Basic Default");
                Query<RDO> imagingProfileRdoQuery = new Query<RDO>
                {
                    ArtifactTypeGuid = Constants.Guids.ObjectType.ImagingProfile,
                    Condition = nameTextCondition,
                    Fields = FieldValue.NoFields
                };
                QueryResultSet<RDO> imagingProfileRdoQueryResultSet;

                try
                {
                    imagingProfileRdoQueryResultSet = rsapiClient.Repositories.RDO.Query(imagingProfileRdoQuery);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{errorContext}. Query.", ex);
                }

                if (!imagingProfileRdoQueryResultSet.Success)
                {
                    throw new Exception($"{errorContext}. ErrorMessage = {imagingProfileRdoQueryResultSet.Message}");
                }

                int imagingProfileArtifactId = imagingProfileRdoQueryResultSet.Results.Single().Artifact.ArtifactID;
                return imagingProfileArtifactId;
            }
            catch (Exception ex)
            {
                throw new Exception(errorContext, ex);
            }
        }

        private int CreateImagingSetRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, string imagingSetName, string imagingProfileName, string savedSearchName)
        {
            string errorContext = "An error occured when creating Imaging Set Record";
            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

                RDO imagingSetRdo = new RDO
                {
                    ArtifactTypeGuids = new List<Guid>
                        {
                            Constants.Guids.ObjectType.ImagingSet
                        }
                };

                imagingSetRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSet.Name, imagingSetName));

                int allDocumentsSavedSearchArtifactId = SavedSearchHelper.GetSavedSearchArtifactId(rsapiClient, workspaceArtifactId, savedSearchName);
                RDO savedSearchRdo = new RDO(allDocumentsSavedSearchArtifactId);
                imagingSetRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSet.DataSource, savedSearchRdo));

                int imagingProfileArtifactId = GetImagingProfileArtifactId(rsapiClient, workspaceArtifactId, imagingProfileName);
                RDO imagingProfileRdo = new RDO(imagingProfileArtifactId);
                imagingSetRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSet.ImagingProfile, imagingProfileRdo));

                imagingSetRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSet.EmailNotificationRecipients, "chandra.alimeti@relativity.com"));
                imagingSetRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSet.Status, "Staging"));

                try
                {
                    int imagingSetArtifactId = rsapiClient.Repositories.RDO.CreateSingle(imagingSetRdo);
                    return imagingSetArtifactId;
                }
                catch (Exception ex)
                {
                    throw new Exception($"{errorContext}. CreateSingle.", ex);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorContext}.", ex);
            }
        }

        private int CreateImagingSetSchedulerRecord(IRSAPIClient rsapiClient, int workspaceArtifactId, string imagingSetSchedulerName, int imagingSetArtifactId)
        {
            string errorContext = "An error occured when creating Imaging Set Scheduler Record";
            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

                RDO imagingSetSchedulerRdo = new RDO
                {
                    ArtifactTypeGuids = new List<Guid>
                        {
                            Constants.Guids.ObjectType.ImagingSetScheduler
                        }
                };

                imagingSetSchedulerRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSetScheduler.Name, imagingSetSchedulerName));

                RDO imagingSetRdo = new RDO(imagingSetArtifactId);
                imagingSetSchedulerRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSetScheduler.ImagingSet, imagingSetRdo));

                MultiChoiceFieldValueList frequencyMultiChoices = new MultiChoiceFieldValueList(
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Sunday),
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Monday),
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Tuesday),
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Wednesday),
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Thursday),
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Friday),
                    new kCura.Relativity.Client.DTOs.Choice(Constants.Guids.Choices.ImagingSetSchedulerFrequency.Saturday)
                )
                {
                    UpdateBehavior = MultiChoiceUpdateBehavior.Replace
                };

                imagingSetSchedulerRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSetScheduler.Frequency, frequencyMultiChoices));
                DateTime scheduledTime = DateTime.Now.AddMinutes(1);
                string scheduledTimeString = scheduledTime.ToString("HH:mm");
                imagingSetSchedulerRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSetScheduler.Time, scheduledTimeString));

                imagingSetSchedulerRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSetScheduler.NextRun, scheduledTime));
                //imagingSetSchedulerRdo.Fields.Add(new FieldValue(Imaging_Set_Scheduler_Field_Status_Guid, "Waiting"));
                imagingSetSchedulerRdo.Fields.Add(new FieldValue(Constants.Guids.Fields.ImagingSetScheduler.LockImagesForQc, false));

                try
                {
                    int imagingSetSchedulerArtifactId = rsapiClient.Repositories.RDO.CreateSingle(imagingSetSchedulerRdo);
                    return imagingSetSchedulerArtifactId;
                }
                catch (Exception ex)
                {
                    throw new Exception($"{errorContext}. CreateSingle.", ex);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorContext}.", ex);
            }
        }

        private string GetImagingSetStatus(IRSAPIClient rsapiClient, int workspaceArtifactId, int imagingSetArtifactId)
        {
            string errorContext = $"An error occured when querying for Imaging Set Status [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}].";
            try
            {
                rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
                RDO imagingSet;

                try
                {
                    imagingSet = rsapiClient.Repositories.RDO.ReadSingle(imagingSetArtifactId);
                }
                catch (Exception ex)
                {
                    throw new Exception($"{errorContext}. ReadSingle.", ex);
                }

                string status = imagingSet.Fields.First(x => x.Guids.Contains(Constants.Guids.Fields.ImagingSet.Status)).ToString();
                return status;
            }
            catch (Exception ex)
            {
                throw new Exception(errorContext, ex);
            }
        }

        private bool CheckIfImagingSetIsCompleted(IRSAPIClient rsapiClient, int workspaceArtifactId, int imagingSetArtifactId)
        {
            // Keep checking the status value for 15 mins.
            const int count = 5;
            const int waitingMinutes = 3;

            for (int i = 1; i <= count; i++)
            {
                string status = GetImagingSetStatus(rsapiClient, workspaceArtifactId, imagingSetArtifactId);
                if (status.Equals(Constants.ImagingSetStatus.Completed))
                {
                    return true;
                }
                Thread.Sleep(waitingMinutes * 60 * 1000);
            }

            return false;
        }
    }
}
