using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Productions.Services;
using Relativity.Services.Field;
using Relativity.Services.Search;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SmokeTest.Helpers
{
	public class ProductionHelper : IProductionHelper
	{
		public void StageProductionSet(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId)
		{
			productionManager.StageProductionAsync(
					workspaceArtifactID: workspaceArtifactId,
					productionArtifactID: productionSetArtifactId).Wait();
		}

		public void RunProductionSet(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId)
		{
			productionManager.RunProductionAsync(
					workspaceArtifactID: workspaceArtifactId,
					productionArtifactID: productionSetArtifactId,
					suppressWarnings: true).Wait();
		}

		public int CreateAndRunProductionSet(ProductionModel productionModel)
		{
			try
			{

				//create production object
				Console.WriteLine("Creating production object.....");
				Production production = CreateProductionObject(productionModel: productionModel);

				//create production set
				Console.WriteLine("Creating production set.....");
				int productionSetArtifactId = CreateProductionSet(
						productionManager: productionModel.ClientModel.ProductionManager,
						workspaceArtifactId: productionModel.WorkspaceArtifactId,
						production: production);
				Console.WriteLine($"Production Set created. [ProductionSetArtifactId= {productionSetArtifactId}].....");

				//create markup set
				Console.WriteLine("Creating markup set.....");
				int markupSetArtifactId = CreateMarkupSet(
						rsapiClient: productionModel.ClientModel.RsapiClient,
						workspaceArtifactId: productionModel.WorkspaceArtifactId,
						name: productionModel.MarkupSetModel.Name,
						markupSetOrder: productionModel.MarkupSetModel.MarkupSetOrder,
						redactionText: productionModel.MarkupSetModel.RedactionText);
				Console.WriteLine($"Markup Set created. [MarkupSetArtifactId= {markupSetArtifactId}].....");

				//add data source to production set with production type
				Console.WriteLine("Creating and adding data source to production set.....");
				CreateAndAddDataSourceToProduction(
						productionDataSourceManager: productionModel.ClientModel.ProductionDataSourceManager,
						workspaceArtifactId: productionModel.WorkspaceArtifactId,
						productionSetArtifactId: productionSetArtifactId,
						productionType: productionModel.ProductionType,
						productionDataSourceName: productionModel.ProductionDataSourceName,
						savedSearchArtifactId: productionModel.SavedSearchArtifactId,
						markupSetArtifactId: markupSetArtifactId);

				//run production set
				RunAndWaitForProductionSetToComplete(
						productionManager: productionModel.ClientModel.ProductionManager,
						workspaceArtifactId: productionModel.WorkspaceArtifactId,
						productionSetArtifactId: productionSetArtifactId,
						stagingAndProductionWaitTimeOutInSeconds: productionModel.StagingAndProductionWaitTimeOutInSeconds);

				return productionSetArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when creating and running Production Set.", ex);
			}
		}

		public int CreateProductionSetBasic(ProductionModel productionModel)
		{
			try
			{
				//create production object
				Console.WriteLine("Creating production object.....");
				Production production = CreateProductionObject(productionModel);

				//create production set
				Console.WriteLine("Creating production set.....");
				int productionSetArtifactId = CreateProductionSet(
						productionModel.ClientModel.ProductionManager,
						productionModel.WorkspaceArtifactId,
						production);
				Console.WriteLine($"Production Set created. [ProductionSetArtifactId= {productionSetArtifactId}].....");

				return productionSetArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when creating the Production Set.", ex);
			}
		}
		public int DeleteProductionSet(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId)
		{
			try
			{
				//delete production set
				Console.WriteLine("Deleting production set.....");
				productionManager.DeleteSingleAsync(workspaceArtifactId, productionSetArtifactId).Wait();
				Console.WriteLine($"Production Set Deleted. [ProductionSetArtifactId= {productionSetArtifactId}].....");

				return productionSetArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when deleting the Production Set.", ex);
			}
		}

		private Production CreateProductionObject(ProductionModel productionModel)
		{
			Production production = new Production
			{
				Name = productionModel.ProductionName,
				Numbering = new PageLevelNumbering
				{
					AttachmentRelationalField = new FieldRef(productionModel.AttachmentRelationalFieldArtifactId),
					BatesPrefix = productionModel.BatesPrefix,
					BatesSuffix = productionModel.BatesSuffix,
					BatesStartNumber = productionModel.BatesStartNumber,
					NumberOfDigitsForDocumentNumbering = productionModel.NumberOfDigitsForDocumentNumbering
				},
				Details = new ProductionDetails
				{
					BrandingFontSize = productionModel.BrandingFontSize,
					ScaleBrandingFont = productionModel.ScaleBrandingFont,
					EmailRecipients = productionModel.EmailRecipients
				}
			};

			return production;
		}

		private ProductionDataSource CreateProductionDataSourceObject(ProductionType productionType, string productionDataSourceName, int savedSearchArtifactId, int markupSetArtifactId, bool burnRedactions)
		{
			ProductionDataSource productionDataSource = new ProductionDataSource
			{
				Name = productionDataSourceName,
				SavedSearch = new SavedSearchRef
				{
					ArtifactID = savedSearchArtifactId
				},
				ProductionType = productionType,
				BurnRedactions = burnRedactions,
				MarkupSet = new MarkupSetRef(markupSetArtifactId)
			};

			return productionDataSource;
		}

		private int CreateProductionSet(IProductionManager productionManager, int workspaceArtifactId, Production production)
		{
			try
			{
				int productionId = productionManager.CreateSingleAsync(workspaceArtifactId, production).Result;
				return productionId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when creating Production Set.", ex);
			}
		}

		private int CreateMarkupSet(IRSAPIClient rsapiClient, int workspaceArtifactId, string name, int markupSetOrder, string redactionText = "who cares")
		{
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;
			try
			{
				MarkupSet markup = new MarkupSet
				{
					Name = name,
					Order = markupSetOrder,
					RedactionText = new MultiLineStringList
										{
												redactionText
										}
				};

				WriteResultSet<MarkupSet> markupResult = rsapiClient.Repositories.MarkupSet.Create(markup);
				int markupSetArtifactId = markupResult.Results.Single().Artifact.ArtifactID;
				return markupSetArtifactId;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when creating Markup Set.", ex);
			}
		}

		private void CreateAndAddDataSourceToProduction(IProductionDataSourceManager productionDataSourceManager, int workspaceArtifactId, int productionSetArtifactId, ProductionType productionType, string productionDataSourceName, int savedSearchArtifactId, int markupSetArtifactId)
		{
			try
			{
				ProductionDataSource productionDataSource = CreateProductionDataSourceObject(productionType, productionDataSourceName, savedSearchArtifactId, markupSetArtifactId, burnRedactions: false);
				productionDataSourceManager.CreateSingleAsync(workspaceArtifactId, productionSetArtifactId, productionDataSource).Wait();
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when adding Data Source to Production", ex);
			}
		}

		private void RunAndWaitForProductionSetToBeStaged(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId, int stagingAndProductionWaitTimeOutInSeconds)
		{
			try
			{
				Console.WriteLine("Production Set staging.....");
				StageProductionSet(productionManager, workspaceArtifactId, productionSetArtifactId);
				WaitForStatusToBe(productionManager, ProductionStatus.Staged, productionSetArtifactId, workspaceArtifactId, stagingAndProductionWaitTimeOutInSeconds);
				Console.WriteLine("Production Set staged.....");
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when staging production.", ex);
			}
		}

		private void RunAndWaitForProductionSetToComplete(IProductionManager productionManager, int workspaceArtifactId, int productionSetArtifactId, int stagingAndProductionWaitTimeOutInSeconds)
		{
			try
			{
				RunAndWaitForProductionSetToBeStaged(productionManager, workspaceArtifactId, productionSetArtifactId, stagingAndProductionWaitTimeOutInSeconds);
				Console.WriteLine("Production Set producing.....");
				RunProductionSet(productionManager, workspaceArtifactId, productionSetArtifactId);
				WaitForStatusToBe(productionManager, ProductionStatus.Produced, productionSetArtifactId, workspaceArtifactId, stagingAndProductionWaitTimeOutInSeconds);
				Console.WriteLine("Production Set produced.....");
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when staging production.", ex);
			}
		}

		private void WaitForStatusToBe(IProductionManager productionManager, ProductionStatus productionStatus, int productionSetArtifactId, int workspaceArtifactId, int timeout)
		{
			int retry = 1;
			Production prod = new Production();
			while (retry != 0 && retry < 10)
			{
				try
				{
					var prodTask = productionManager.ReadSingleAsync(workspaceArtifactId, productionSetArtifactId);
					prodTask.Wait();
					retry = 0;
					prod = prodTask.Result;
				}
				catch
				{
					retry += 1;
				}
			}

			Stopwatch s = new Stopwatch();
			s.Start();
			while (!prod.ProductionMetadata.Status.Equals(productionStatus))
			{
				if (s.Elapsed > TimeSpan.FromSeconds(timeout))
				{
					throw new TimeoutException($"Failed to get to status: {productionStatus}. Current status: {prod.ProductionMetadata.Status}. \n LastRunError: {prod.ProductionMetadata.LastRunError}");
				}
				try
				{
					Task<Production> prodTask = productionManager.ReadSingleAsync(workspaceArtifactId, productionSetArtifactId);
					prodTask.Wait();
					prod = prodTask.Result;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to read production {productionSetArtifactId} \nException: {ex.Message} \nInner Exception: {ex.InnerException}");
					throw;
				}
			}
			s.Stop();
		}
	}
}
