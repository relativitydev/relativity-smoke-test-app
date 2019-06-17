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
		public IRSAPIClient RsapiClient;
		public IImagingProfileManager ImagingProfileManager;
		public IImagingSetManager ImagingSetManager;
		public IImagingJobManager ImagingJobManager;
		public ImageHelper(IRSAPIClient rsapiClient, IImagingProfileManager imagingProfileManager, IImagingSetManager imagingSetManager, IImagingJobManager imagingJobManager)
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

			RsapiClient = rsapiClient;
			ImagingProfileManager = imagingProfileManager;
			ImagingSetManager = imagingSetManager;
			ImagingJobManager = imagingJobManager;
		}
		
		public ResultModel ImageDocuments(int workspaceArtifactId)
		{
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
			}

			ResultModel resultModel = new ResultModel("Image");

			try
			{
				RsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
				int? imagingSetArtifactId = null;
				int? imagingProfileArtifactId = null;

				try
				{
					// Create Imaging Profile record
					string imagingProfileName = $"{Constants.Prefix} - Imaging Profile - {DateTime.Now}";
					imagingProfileArtifactId = CreateImagingProfileRecord(workspaceArtifactId, imagingProfileName);

					// Create Imaging Set record
					string imagingSetName = $"{Constants.Prefix} - Imaging Set - {DateTime.Now}";
					imagingSetArtifactId = CreateImagingSetRecord(workspaceArtifactId, imagingSetName, Constants.AllDocumentsSavedSearchName, imagingProfileArtifactId.Value);

					// Run Imaging Set
					RunImagingSet(workspaceArtifactId, imagingSetArtifactId.Value);

					// Wait to check if documents are imaged
					bool areDocumentsImaged = CheckIfImagingSetIsCompleted(workspaceArtifactId, imagingSetArtifactId.Value);
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
						DeleteImagingSet(workspaceArtifactId, imagingSetArtifactId.Value);
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

		public void DeleteImagingSet(int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when deleting an Imaging Set [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				Console.WriteLine($"Deleting Imaging Set and its associated Imaging Profile. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");

				// Retrieve Imaging Set
				ImagingSet imagingSet = RetrieveImagingSetRecord(workspaceArtifactId, imagingSetArtifactId);

				// Delete Imaging Set
				DeleteImagingSetRecord(workspaceArtifactId, imagingSet.ArtifactID);

				// Delete Imaging Profile
				int imagingProfileArtifactId = imagingSet.ImagingProfile.ArtifactID;
				DeleteImagingProfileRecord(workspaceArtifactId, imagingProfileArtifactId);

				Console.WriteLine($"Deleted Imaging Set and its associated Imaging Profile. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private int CreateImagingProfileRecord(int workspaceArtifactId, string imagingProfileName)
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
					imagingProfileArtifactId = ImagingProfileManager.SaveAsync(imagingProfile, workspaceArtifactId).Result;
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

		private void DeleteImagingProfileRecord(int workspaceArtifactId, int imagingProfileArtifactId)
		{
			string errorContext = $"An error occured when deleting an Imaging Profile record. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]";
			try
			{
				Console.WriteLine($"Deleting Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
				ImagingProfile imagingProfile = RetrieveImagingProfileRecord(workspaceArtifactId, imagingProfileArtifactId);
				if (imagingProfile != null && imagingProfile.ArtifactID > 0)
				{
					try
					{
						ImagingProfileManager.DeleteAsync(imagingProfileArtifactId, workspaceArtifactId).Wait();
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

		private ImagingProfile RetrieveImagingProfileRecord(int workspaceArtifactId, int imagingProfileArtifactId)
		{
			string errorContext = $"An error occured when retriving an Imaging Profile record. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]";
			try
			{
				try
				{
					Console.WriteLine($"Retriving Imaging Profile. [{nameof(imagingProfileArtifactId)}: {imagingProfileArtifactId}]");
					ImagingProfile imagingProfile = ImagingProfileManager.ReadAsync(imagingProfileArtifactId, workspaceArtifactId).Result;
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

		private int CreateImagingSetRecord(int workspaceArtifactId, string imagingSetName, string savedSearchName, int imagingProfileArtifactId)
		{
			string errorContext = $"An error occured when creating an Imaging Set record. [{nameof(imagingSetName)}: {imagingSetName}]";
			try
			{
				int allDocumentsSavedSearchArtifactId = SavedSearchHelper.GetSavedSearchArtifactId(RsapiClient, workspaceArtifactId, savedSearchName);

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
					imagingSetArtifactId = ImagingSetManager.SaveAsync(imagingSet, workspaceArtifactId).Result;
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

		private void DeleteImagingSetRecord(int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when deleting an Imaging Set record. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				Console.WriteLine($"Deleting Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
				ImagingSet imagingSet = RetrieveImagingSetRecord(workspaceArtifactId, imagingSetArtifactId);
				if (imagingSet != null && imagingSet.ArtifactID > 0)
				{
					try
					{
						ImagingSetManager.DeleteAsync(imagingSetArtifactId, workspaceArtifactId).Wait();
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

		private ImagingSet RetrieveImagingSetRecord(int workspaceArtifactId, int imagingSetArtifactId)
		{
			string errorContext = $"An error occured when retriving an Imaging Set record. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]";
			try
			{
				try
				{
					Console.WriteLine($"Retrieving Imaging Set. [{nameof(imagingSetArtifactId)}: {imagingSetArtifactId}]");
					ImagingSet imagingSet = ImagingSetManager.ReadAsync(imagingSetArtifactId, workspaceArtifactId).Result;
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

		private void RunImagingSet(int workspaceArtifactId, int imagingSetArtifactId)
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
				Guid? imagingJobGuid = (ImagingJobManager.RunImagingSetAsync(imagingJob).Result).ImagingJobId;
				Console.WriteLine($"Ran Imaging Job. [{nameof(imagingJobGuid)}]: {imagingJobGuid}");
			}
			catch (Exception ex)
			{
				throw new SmokeTestException($"{errorContext}", ex);
			}
		}

		private bool CheckIfImagingSetIsCompleted(int workspaceArtifactId, int imagingSetArtifactId)
		{
			// Keep checking the status value for 15 mins.
			Console.WriteLine("Checking if the documents in the Imaging Set. Check every 1 minutes for upto 15 minutes.");
			const int count = 90;
			const int waitingSeconds = 10;

			for (int i = 1; i <= count; i++)
			{
				ImagingSet imagingSet = RetrieveImagingSetRecord(workspaceArtifactId, imagingSetArtifactId);
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
