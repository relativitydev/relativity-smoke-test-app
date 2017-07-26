using kCura.Relativity.Client;
using Relativity.Productions.Services;
using System;

namespace SmokeTest.Models
{
    public class ClientModel
    {
        public IRSAPIClient RsapiClient { get; set; }
        public IProductionManager ProductionManager { get; set; }
        public IProductionDataSourceManager ProductionDataSourceManager { get; set; }

        public ClientModel(IRSAPIClient rsapiClient, IProductionManager productionManager, IProductionDataSourceManager productionDataSourceManager)
        {
            if (rsapiClient == null)
            {
                throw new ArgumentNullException(nameof(rsapiClient));
            }

            if (productionManager == null)
            {
                throw new ArgumentNullException(nameof(productionManager));
            }

            if (productionDataSourceManager == null)
            {
                throw new ArgumentNullException(nameof(productionDataSourceManager));
            }

            RsapiClient = rsapiClient;
            ProductionManager = productionManager;
            ProductionDataSourceManager = productionDataSourceManager;
        }
    }
}
