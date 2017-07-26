using Relativity.Productions.Services;
using SmokeTest.Models;

namespace SmokeTest.Interfaces
{
    public interface IProductionHelper
    {
        int CreateAndRunProductionSet(ProductionModel productionModel);
        int CreateProductionSetBasic(ProductionModel productionModel);
        int DeleteProductionSet(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId);
        void RunProductionSet(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId);
        void StageProductionSet(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId);
    }
}