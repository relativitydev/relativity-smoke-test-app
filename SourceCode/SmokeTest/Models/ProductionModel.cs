using Relativity.Productions.Services;
using System;

namespace SmokeTest.Models
{
    public class ProductionModel
    {
        public int WorkspaceArtifactId { get; set; }
        public string ProductionName { get; set; }
        public int AttachmentRelationalFieldArtifactId { get; set; }
        public string BatesPrefix { get; set; }
        public string BatesSuffix { get; set; }
        public int BatesStartNumber { get; set; }
        public int NumberOfDigitsForDocumentNumbering { get; set; }
        public int BrandingFontSize { get; set; }
        public bool ScaleBrandingFont { get; set; }
        public string EmailRecipients { get; set; }
        public ProductionType ProductionType { get; set; }
        public string ProductionDataSourceName { get; set; }
        public int SavedSearchArtifactId { get; set; }
        public MarkupSetModel MarkupSetModel { get; set; }
        public ClientModel ClientModel { get; set; }
        public int StagingAndProductionWaitTimeOutInSeconds { get; set; }

        public ProductionModel(int workspaceArtifactId, string productionName, int attachmentRelationalFieldArtifactId, string batesPrefix, string batesSuffix, int batesStartNumber, int numberOfDigitsForDocumentNumbering, int brandingFontSize, bool scaleBrandingFont, string emailRecipients, ProductionType productionType, string productionDataSourceName, int savedSearchArtifactId, MarkupSetModel markupSetModel, ClientModel clientModel, int stagingAndProductionWaitTimeOutInSeconds)
        {
            if (productionName == null)
            {
                throw new ArgumentNullException(nameof(productionName));
            }

            if (batesPrefix == null)
            {
                throw new ArgumentNullException(nameof(batesPrefix));
            }

            if (batesSuffix == null)
            {
                throw new ArgumentNullException(nameof(batesSuffix));
            }

            if (emailRecipients == null)
            {
                throw new ArgumentNullException(nameof(emailRecipients));
            }

            if (productionDataSourceName == null)
            {
                throw new ArgumentNullException(nameof(productionDataSourceName));
            }

            WorkspaceArtifactId = workspaceArtifactId;
            ProductionName = productionName;
            AttachmentRelationalFieldArtifactId = attachmentRelationalFieldArtifactId;
            BatesPrefix = batesPrefix;
            BatesSuffix = batesSuffix;
            BatesStartNumber = batesStartNumber;
            NumberOfDigitsForDocumentNumbering = numberOfDigitsForDocumentNumbering;
            BrandingFontSize = brandingFontSize;
            ScaleBrandingFont = scaleBrandingFont;
            EmailRecipients = emailRecipients;
            ProductionType = productionType;
            ProductionDataSourceName = productionDataSourceName;
            SavedSearchArtifactId = savedSearchArtifactId;
            MarkupSetModel = markupSetModel;
            ClientModel = clientModel;
            StagingAndProductionWaitTimeOutInSeconds = stagingAndProductionWaitTimeOutInSeconds;
        }
    }
}
