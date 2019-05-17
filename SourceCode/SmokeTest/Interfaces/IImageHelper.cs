using kCura.Relativity.Client;
using Relativity.Imaging.Services.Interfaces;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IImageHelper
	{
		ResultModel ImageDocuments(IRSAPIClient rsapiClient, IImagingProfileManager imagingProfileManager, IImagingSetManager imagingSetManager, IImagingJobManager imagingJobManager, int workspaceArtifactId);

		void DeleteImagingSet(IImagingSetManager imagingSetManager, IImagingProfileManager imagingProfileManager, int workspaceArtifactId, int imagingSetArtifactId);
	}
}