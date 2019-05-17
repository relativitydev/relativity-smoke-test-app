using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using SmokeTest.Exceptions;
using SmokeTest.Interfaces;
using SmokeTest.Models;
using System;

namespace SmokeTest.Helpers
{
	public class FieldHelper : IFieldHelper
	{
		public ResultModel CreateSingleChoiceDocumentField(IRSAPIClient rsapiClient, int workspaceArtifactId, string fieldName)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
			}
			if (fieldName == null)
			{
				throw new ArgumentNullException(nameof(fieldName));
			}

			ResultModel resultModel = new ResultModel("Field");
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				kCura.Relativity.Client.DTOs.Field fieldDto = new kCura.Relativity.Client.DTOs.Field
				{
					Name = fieldName,
					ObjectType = new ObjectType
					{
						DescriptorArtifactTypeID = (int)ArtifactType.Document
					},
					FieldTypeID = FieldType.SingleChoice,
					IsRequired = false,
					Unicode = false,
					AvailableInFieldTree = false,
					OpenToAssociations = false,
					Linked = false,
					AllowSortTally = true,
					Wrapping = true,
					AllowGroupBy = false,
					AllowPivot = false,
					IgnoreWarnings = true,
					Width = ""
				};

				try
				{
					int newSingleChoiceFieldArtifactId = rsapiClient.Repositories.Field.CreateSingle(fieldDto);
					resultModel.Success = true;
					resultModel.ArtifactId = newSingleChoiceFieldArtifactId;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when creating single choice document field. [{nameof(fieldName)} = {fieldName}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}

		public ResultModel DeleteSingleChoiceDocumentField(IRSAPIClient rsapiClient, int workspaceArtifactId, int fieldArtifactId)
		{
			if (rsapiClient == null)
			{
				throw new ArgumentNullException(nameof(rsapiClient));
			}
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(workspaceArtifactId)} should be a positive number.");
			}
			if (fieldArtifactId < 1)
			{
				throw new ArgumentException($"{nameof(fieldArtifactId)} should be a positive number.");
			}

			ResultModel resultModel = new ResultModel("Field");
			rsapiClient.APIOptions.WorkspaceID = workspaceArtifactId;

			try
			{
				try
				{
					rsapiClient.Repositories.Field.DeleteSingle(fieldArtifactId);
					resultModel.Success = true;
				}
				catch (Exception ex)
				{
					throw new SmokeTestException($"An error occured when deleting single choice document field. [{nameof(fieldArtifactId)} = {fieldArtifactId}]", ex);
				}
			}
			catch (Exception ex)
			{
				resultModel.Success = false;
				resultModel.ErrorMessage = ex.ToString();
			}

			return resultModel;
		}
	}
}