using kCura.EventHandler;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Productions.Services;
using Relativity.Services.Agent;
using Relativity.Services.Search;
using SmokeTest.Exceptions;
using SmokeTest.Helpers;
using System;

namespace SmokeTest.EventHandlers
{
    [kCura.EventHandler.CustomAttributes.Description("Smoke Test - Post Install Event Handler")]
    [kCura.EventHandler.CustomAttributes.RunOnce(false)]
    [System.Runtime.InteropServices.Guid("1492127B-AFD9-4774-B445-D7E2777919C3")]
    public class PostInstallEventHandler : kCura.EventHandler.PostInstallEventHandler
    {
        public override Response Execute()
        {
            Response response = new Response
            {
                Message = string.Empty,
                Success = true
            };

            try
            {
                ExecutionIdentity systemExecutionIdentity = ExecutionIdentity.System;
                IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(systemExecutionIdentity);
                IAgentManager agentManager = Helper.GetServicesManager().CreateProxy<IAgentManager>(systemExecutionIdentity);
                IProductionManager productionManager = Helper.GetServicesManager().CreateProxy<IProductionManager>(systemExecutionIdentity);
                IProductionDataSourceManager productionDataSourceManager = Helper.GetServicesManager().CreateProxy<IProductionDataSourceManager>(systemExecutionIdentity);
                IKeywordSearchManager keywordSearchManager = Helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(systemExecutionIdentity);
                int workspaceArtifactId = Helper.GetActiveCaseID();

                if (workspaceArtifactId != -1)
                {
                    IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);
                    int documentIdentifierFieldArtifactId = SqlHelper.GetIdentifierFieldArtifactId(workspaceDbContext, workspaceArtifactId);
                    RsapiTests rsapiTests = new RsapiTests(
                        rsapiClient: rsapiClient,
                        agentManager: agentManager,
                        productionManager: productionManager,
                        productionDataSourceManager: productionDataSourceManager,
                        keywordSearchManager: keywordSearchManager,
                        workspaceArtifactId: workspaceArtifactId,
                        documentIdentifierFieldArtifactId: documentIdentifierFieldArtifactId);
                    rsapiTests.RunAllTests();
                }
            }
            catch (Exception ex)
            {
                throw new SmokeTestException("An error occured in the PostInstallEventHandler.", ex);
            }

            return response;
        }
    }
}
