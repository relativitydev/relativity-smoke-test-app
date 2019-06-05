using kCura.Relativity.Client;
using Relativity.API;
using Relativity.DocumentViewer.Services;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
	public interface IViewerHelper
	{
		ResultModel ConvertDocumentsForViewer(int workspaceArtifactId);
	}
}