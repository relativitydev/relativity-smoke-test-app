using kCura.Relativity.Client;
using Relativity.Imaging.Services.Interfaces;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Threading;

namespace SmokeTest.Helpers
{
	public class ImageHelper : IImageHelper
	{
		public ResultModel ImageDocuments(IRSAPIClient rsapiClient, IImagingProfileManager imagingProfileManager, IImagingSetManager imagingSetManager, IImagingJobManager imagingJobManager, int workspaceArtifactId)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (imagingProfileManager == null)
			{
				throw new ArgumentNullException(nameof(imagingProfileManager));
			}
			if (imagingSetManager == null)
			{
				throw new ArgumentNullException(nameof(imagingSetManager));
			}
			if (imagingJobManager == null)
			{
				throw new ArgumentNullException(nameof(imagingJobManager));
			}
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
			}

			ResultModel resultModel = new ResultModel("Image");

			try
			{
				rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
				int? imagingSetArtifactId = null;

				try
				{
					// Create Imaging Profile record
					string imagingProfileName = $"{Constants.Prefix} - Imaging Profile - {DateTime.Now}";
					int imagingProfileArtifactId = CreateImagingProfileRecord(imagingProfileManager, workspaceArtifactId, imagingProfileName);

					// Create Imaging Set record
					string imagingSetName = $"{Constants.Prefix} - Imaging Set - {DateTime.Now}";
					imagingSetArtifactId = CreateImagingSetRecord(rsapiClient, imagingSetManager, workspaceArtifactId, imagingSetName, Constants.AllDocumentsSavedSearchName, imagingProfileArtifactId);

					// Run Imaging Set
					RunImagingSet(imagingJobManager, workspaceArtifactId, imagingSetArtifactId.Value);

					// Wait to check if documents are imaged
					bool areDocumentsImaged = CheckIfImagingSetIsCompleted(imagingSetManager, workspaceArtifactId, imagingSetArtifactId.Value);
					if (!areDocumentsImaged)
					{
						throw new SmokeTestException("Documents were not successfully imaged. Imaging Set did not Complete.");
					}

					// Set resultModel properties
					resultModel.Success = true;
					resultModel.ArtifactId = imagingSetArtifactId.Value;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException("An error occured when imaging documents.", ex);
				}
				finally
				{
					if (imagingSetArtifactId != null)
					{
						DeleteImagingSet(imagingSetManager, imagingProfileManager, workspaceArtifactId, imagingSetArtifactId.Value);
					}
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		public void DeleteImagingSet(IImagingSetManager imagingSetManager, IImagingProfileManager imagingProfileManager, int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when deleting an Imaging Set [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				Console.WriteLine($"Deleting Imaging Set and its associated Imaging Profile. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");

				// Retrieve Imaging Set
				ImagingSet imagingSet = RetrieveImagingSetRecord(imagingSetManager, workspaceArtifactId, imagingSetArtifactId);

				// Delete Imaging Set
				DeleteImagingSetRecord(imagingSetManager, workspaceArtifactId, imagingSet.ArtifactID);

				// Delete Imaging Profile
				int imagingProfileArtifactId = imagingSet.ImagingProfile.ArtifactID;
				DeleteImagingProfileRecord(imagingProfileManager, workspaceArtifactId, imagingProfileArtifactId);

				Console.WriteLine($"Deleted Imaging Set and its associated Imaging Profile. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private int CreateImagingProfileRecord(IImagingProfileManager imagingProfileManager, int workspaceArtifactId, string imagingProfileName)
		{
			string errorContext = $"An error occured when creating an Imaging Profile record. [{nameof(imagingProfileName)}: {imagingProfileName}]";
			try
			{
				ImagingProfile imagingProfile = new ImagingProfile
				{
					BasicOptions = new BasicImagingEngineOptions
					{
						ImageOutputDpi = 100,
						BasicImageFormat = ImageFormat.Jpeg,
						ImageSize = ImageSize.A4
					},
					Name = imagingProfileName,
					ImagingMethod = ImagingMethod.Basic
				};

				int imagingProfileArtifactId;
				try
				{
					Console.WriteLine($"Creating new Imaging Profile. [{nameof(imagingProfileName)}: {imagingProfileName}]");
					imagingProfileArtifactId = imagingProfileManager.SaveAsync(imagingProfile, workspaceArtifactId).Result;
					Console.WriteLine($"Created new Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. SaveAsync.", ex);
				}
				return imagingProfileArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private void DeleteImagingProfileRecord(IImagingProfileManager imagingProfileManager, int workspaceArtifactId, int imagingProfileArtifactId)
		{
			string errorContext = $"An error occured when deleting an Imaging Profile record. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]";
			try
			{
				Console.WriteLine($"Deleting Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
				ImagingProfile imagingProfile = RetrieveImagingProfileRecord(imagingProfileManager, workspaceArtifactId, imagingProfileArtifactId);
				if (imagingProfile != null && imagingProfile.ArtifactID > 0)
				{
					try
					{
						imagingProfileManager.DeleteAsync(imagingProfileArtifactId, workspaceArtifactId).Wait();
						Console.WriteLine($"Deleted Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
					}
					catch (Exception ex)
					{
						throw new SmokeTestException($"{errorContext}. DeleteAsync.", ex);
					}
				}
				else
				{
					Console.WriteLine($"Imaging Profile doesn't exist. Skipped Deletion. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
				}

			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private ImagingProfile RetrieveImagingProfileRecord(IImagingProfileManager imagingProfileManager, int workspaceArtifactId, int imagingProfileArtifactId)
		{
			string errorContext = $"An error occured when retriving an Imaging Profile record. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]";
			try
			{
				try
				{
					Console.WriteLine($"Retriving Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
					ImagingProfile imagingProfile = imagingProfileManager.ReadAsync(imagingProfileArtifactId, workspaceArtifactId).Result;
					Console.WriteLine($"Retrived Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
					return imagingProfile;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. DeleteAsync.", ex);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private int CreateImagingSetRecord(IRSAPIClient rsapiClient, IImagingSetManager imagingSetManager, int workspaceArtifactId, string imagingSetName, string savedSearchName, int imagingProfileArtifactId)
		{
			string errorContext = $"An error occured when creating an Imaging Set record. [{nameof(imagingSetName)}: {imagingSetName}]";
			try
			{
				int allDocumentsSavedSearchArtifactId = SavedSearchHelper.GetSavedSearchArtifactId(rsapiClient, workspaceArtifactId, savedSearchName);

				ImagingSet imagingSet = new ImagingSet
				{
					DataSource = allDocumentsSavedSearchArtifactId,
					Name = imagingSetName,
					ImagingProfile = new ImagingProfileRef
					{
						ArtifactID = imagingProfileArtifactId
					},
					EmailNotificationRecipients = ""
				};

				int imagingSetArtifactId;
				try
				{
					Console.WriteLine($"Creating new Imaging Set. [{nameof(imagingSetName)}: {imagingSetName}]");
					imagingSetArtifactId = imagingSetManager.SaveAsync(imagingSet, workspaceArtifactId).Result;
					Console.WriteLine($"Created new Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. SaveAsync.", ex);
				}
				return imagingSetArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private void DeleteImagingSetRecord(IImagingSetManager imagingSetManager, int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when deleting an Imaging Set record. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				Console.WriteLine($"Deleting Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
				ImagingSet imagingSet = RetrieveImagingSetRecord(imagingSetManager, workspaceArtifactId, imagingSetArtifactId);
				if (imagingSet != null && imagingSet.ArtifactID > 0)
				{
					try
					{
						imagingSetManager.DeleteAsync(imagingSetArtifactId, workspaceArtifactId).Wait();
						Console.WriteLine($"Deleted Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
					}
					catch (Exception ex)
					{
						throw new SmokeTestException($"{errorContext}. DeleteAsync.", ex);
					}
				}
				else
				{
					Console.WriteLine($"Imaging Set doesn't exist. Skipped Deletion. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private ImagingSet RetrieveImagingSetRecord(IImagingSetManager imagingSetManager, int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when retriving an Imaging Set record. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				try
				{
					Console.WriteLine($"Retrieving Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
					ImagingSet imagingSet = imagingSetManager.ReadAsync(imagingSetArtifactId, workspaceArtifactId).Result;
					Console.WriteLine($"Retrieved Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
					return imagingSet;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"{errorContext}. ReadAsync.", ex);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private void RunImagingSet(IImagingJobManager imagingJobManager, int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when running an Imaging Set [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				ImagingJob imagingJob = new ImagingJob
				{
					ImagingSetId = imagingSetArtifactId,
					WorkspaceId = workspaceArtifactId,
					QcEnabled = false
				};

				// Run Imaging Set
				Console.WriteLine($"Running Imaging Job. [{nameof(imagingSetArtifactId)}]: {imagingSetArtifactId}");
				Guid? imagingJobGuid = (imagingJobManager.RunImagingSetAsync(imagingJob).Result).ImagingJobId;
				Console.WriteLine($"Ran Imaging Job. [{nameof(imagingJobGuid)}]: {imagingJobGuid}");
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private bool CheckIfImagingSetIsCompleted(IImagingSetManager imagingSetManager, int workspaceArtifactId, int imagingSetArtifactId)
		{
			// Keep checking the status value for 15 mins.
			Console.WriteLine("Checking if the documents in the Imaging Set. Check every 1 minutes for upto 15 minutes.");
			const int count = 90;
			const int waitingSeconds = 10;

			for (int i = 1; i <= count; i++)
			{
				ImagingSet imagingSet = RetrieveImagingSetRecord(imagingSetManager, workspaceArtifactId, imagingSetArtifactId);
				ImagingSetStatus imagingSetStatus = imagingSet.Status;
				if (imagingSetStatus != null)
				{
					string status = imagingSetStatus.Status;
					Console.WriteLine($"Imaging Set status: {status}");
					if (status.Equals(Constants.ImagingSetJobStatus.Completed))
					{
						Console.WriteLine("Imaging Set Job Completed.");
						return true;
					}
					if (status.Contains("error"))
					{
						Console.WriteLine("Imaging Set Job has Errors.");
						return false;
					}
				}
				Console.WriteLine("Job still running. Sleeping for 10 seconds.");
				Thread.Sleep(waitingSeconds * 1000);
			}

			return false;
		}
	}
}
