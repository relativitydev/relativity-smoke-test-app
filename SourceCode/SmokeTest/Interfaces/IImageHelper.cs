using kCura.Relativity.Client;
using Relativity.Imaging.Services.Interfaces;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IImageHelper
	{
		ResultModel ImageDocuments(int workspaceArtifactId);

		void DeleteImagingSet(int workspaceArtifactId, int imagingSetArtifactId);
	}
}