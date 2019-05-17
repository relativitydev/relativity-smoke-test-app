using Emailer;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using SmokeTest.Exceptions;
using SmokeTest.Helpers;
using SmokeTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SmokeTest.Agents
{
	[kCura.Agent.CustomAttributes.Name(Constants.SmokeTestAnalysisAgentName)]
	[System.Runtime.InteropServices.Guid("37A4759D-C537-42A5-B77D-D516D19EA5DB")]
	public class SmokeTestAnalysisAgent : kCura.Agent.AgentBase
	{
		private DateTime _lastEmailSentDateTime;
		private IAPILog _logger;

		public SmokeTestAnalysisAgent()
		{
			_lastEmailSentDateTime = DateTime.Now.AddHours(-25);
		}

		public override void Execute()
		{
			_logger = Helper.GetLoggerFactory().GetLogger();
			bool emailSent = false;

			try
			{
				RaiseMessage("Running Smoke tests analysis.", 1);
				ExecutionIdentity systemExecutionIdentity = ExecutionIdentity.System;
				IRSAPIClient rsapiClient = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(systemExecutionIdentity);
				IDBContext eddsDbContext = Helper.GetDBContext(-1);
				List<int> workspaceArtifactIds = RetrieveAllApplicationWorkspaces(eddsDbContext, Constants.Guids.Application.SmokeTest);
				int workspaceCount = workspaceArtifactIds.Count;
				RaiseMessage($"Found '{workspaceCount}' applicable workspaces.", 1);

				if (workspaceCount > 0)
				{
					foreach (int currentWorkspaceArtifactId in workspaceArtifactIds)
					{
						if (currentWorkspaceArtifactId != -1)
						{
							try
							{
								RaiseMessage($"Running Smoke tests analysis in Workspace [{currentWorkspaceArtifactId}]", 1);
								IRdoHelper rdoHelper = new RdoHelper();

								int allTestsWithNewStatusCount = rdoHelper.GetTestRdoRecordsCountWithStatus(rsapiClient, currentWorkspaceArtifactId, Constants.Status.TestRdo.New);
								int allTestsWithRunningTestStatusCount = rdoHelper.GetTestRdoRecordsCountWithStatus(rsapiClient, currentWorkspaceArtifactId, Constants.Status.TestRdo.RunningTest);
								if (allTestsWithNewStatusCount > 0 || allTestsWithRunningTestStatusCount > 0)
								{
									// Some Smoke Tests still running
									RaiseMessage($"Skipped analysis. Smoke tests still have to run or already running in the Workspace [{currentWorkspaceArtifactId}]", 1);
								}
								else
								{
									// All Smoke Tests finished running
									List<RDO> allTestRdoRecords = rdoHelper.RetrieveAllTestRdos(rsapiClient, currentWorkspaceArtifactId);
									int testsTotalCount = allTestRdoRecords.Count;
									if (testsTotalCount > 0)
									{
										int testsSuccessCount = allTestRdoRecords.Count(x => x.Fields.Get(Constants.Guids.Fields.Test.Status).ToString() == Constants.Status.TestRdo.Success);
										int testsFailCount = allTestRdoRecords.Count(x => x.Fields.Get(Constants.Guids.Fields.Test.Status).ToString() == Constants.Status.TestRdo.Fail);

										DateTime currentDate = DateTime.Now.Date;
										if (currentDate > _lastEmailSentDateTime.Date)
										{
											SendEmail(currentWorkspaceArtifactId, testsTotalCount, testsSuccessCount, testsFailCount, allTestRdoRecords);
											emailSent = true;
										}
										else
										{
											RaiseMessage($"Email Skipped. Email already sent for today. [Last Email Sent Time: {_lastEmailSentDateTime}]", 1);
											emailSent = false;
										}
									}
									else
									{
										RaiseMessage($"Skipped analysis. No Smoke tests in Workspace [{currentWorkspaceArtifactId}]", 1);
									}
								}

							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Smoke Test Runner Agent");
							}
							finally
							{
								RaiseMessage($"Finished running Smoke tests analysis in Workspace [{currentWorkspaceArtifactId}]", 1);
								rsapiClient?.Dispose();
							}

						}
					}
				}
				else
				{
					RaiseMessage("Smoke Test application is not installed in any workspace.", 1);
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when running the Smoke tests.", ex);
			}
			finally
			{
				// If atleast one email is sent, set lastEmailSentDateTime
				if (emailSent)
				{
					_lastEmailSentDateTime = DateTime.Now;
				}

				RaiseMessage("Finished Smoke tests analysis.", 1);
			}
		}

		private List<int> RetrieveAllApplicationWorkspaces(IDBContext eddsDbContext, Guid applicationGuid)
		{
			List<int> workspaceArtifactIds = new List<int>();

			try
			{
				DataTable dataTable = SqlHelper.RetrieveApplicationWorkspaces(eddsDbContext, applicationGuid);

				foreach (DataRow dataRow in dataTable.Rows)
				{
					int currentWorkspaceArtifactId = (int)dataRow["ArtifactID"];
					if (currentWorkspaceArtifactId > 0)
					{
						workspaceArtifactIds.Add(currentWorkspaceArtifactId);
					}
				}
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error when querying for workspaces where Smoke Test application is installed.", ex);
			}

			return workspaceArtifactIds;
		}

		private void SendEmail(int workspaceArtifactId, int testsTotalCount, int testsSuccessCount, int testsFailCount, List<RDO> allTestRdoRecords)
		{
			string emailSubject = BuildEmailSubject();
			string emailBody = BuildEmailBody(workspaceArtifactId, testsTotalCount, testsSuccessCount, testsFailCount, allTestRdoRecords);
			SmtpClientSettings smtpClientSettings = GetSmtpClientSettings();
			EmailService emailService = GetEmailService(smtpClientSettings);
			Email email = new Email(smtpClientSettings.EmailFrom, smtpClientSettings.EmailTo, emailSubject, emailBody);
			email.Send(emailService);
		}

		private EmailService GetEmailService(SmtpClientSettings settings)
		{
			return new EmailService(settings.SmtpServer, settings.SmtpUsername, settings.SmtpPassword, settings.SmtpPort);
		}

		private SmtpClientSettings GetSmtpClientSettings()
		{
			SmtpClientSettings smtpClientSettings = new SmtpClientSettings(Helper.GetServicesManager(), ExecutionIdentity.System);
			smtpClientSettings.GetSettings();
			return smtpClientSettings;
		}

		private string BuildEmailSubject()
		{
			try
			{
				string emailSubject = $"Smoke Tests Report - {DateTime.Now}";
				return emailSubject;
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when building email subject.", ex);
			}
		}

		private string BuildEmailBody(int workspaceArtifactId, int testsTotalCount, int testsSuccessCount, int testsFailCount, List<RDO> allTestRdoRecords)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();

				stringBuilder.AppendLine("<html>");
				stringBuilder.AppendLine("<head>");
				stringBuilder.AppendLine("<style>");
				stringBuilder.AppendLine("table { font - family: arial, sans - serif; border - collapse: collapse; width: 100 %; }");
				stringBuilder.AppendLine("td, th { border: 1px solid #dddddd; text - align: left; padding: 8px; }");
				stringBuilder.AppendLine("tr:nth-child(even) { background - color: #dddddd; }");
				stringBuilder.AppendLine("</style>");
				stringBuilder.AppendLine("</head>");
				stringBuilder.AppendLine("<body>");
				stringBuilder.AppendLine($"<h2>Workspace ArtifactId: {workspaceArtifactId}</h2>");
				stringBuilder.AppendLine("<br/>");
				stringBuilder.AppendLine($"<div style=\"color: blue; font-weight: bold;\"> Total Tests: {testsTotalCount}</div>");
				stringBuilder.AppendLine($"<div style=\"color: green; font-weight: bold;\"> Success: {testsSuccessCount}</div>");
				stringBuilder.AppendLine($"<div style=\"color: red; font-weight: bold;\"> Fail: {testsFailCount}</div>");
				stringBuilder.AppendLine("<br/>");
				stringBuilder.AppendLine("<table>");
				stringBuilder.AppendLine("<tr>");
				stringBuilder.AppendLine("<th>Name</th>");
				stringBuilder.AppendLine("<th>Status</th>");
				stringBuilder.AppendLine("<th>Timestamp</th>");
				stringBuilder.AppendLine("<th>Error</th>");
				stringBuilder.AppendLine("<th>Error Details</th>");
				stringBuilder.AppendLine("</tr>");

				foreach (RDO testRdo in allTestRdoRecords)
				{
					string name = testRdo.Fields.Get(Constants.Guids.Fields.Test.Name).ValueAsFixedLengthText;
					string status = testRdo.Fields.Get(Constants.Guids.Fields.Test.Status).ValueAsFixedLengthText;
					string timestamp = testRdo.Fields.Get(Constants.Guids.Fields.Test.SystemLastModifiedOn).ValueAsDate.ToString();
					string error = testRdo.Fields.Get(Constants.Guids.Fields.Test.Error).ValueAsLongText;
					string errorDetails = testRdo.Fields.Get(Constants.Guids.Fields.Test.ErrorDetails).ValueAsLongText;

					stringBuilder.AppendLine("<tr>");
					stringBuilder.AppendLine($"<td>{name}</td>");
					stringBuilder.AppendLine($"<td>{status}</td>");
					stringBuilder.AppendLine($"<td>{timestamp}</td>");
					stringBuilder.AppendLine($"<td>{error}</td>");
					stringBuilder.AppendLine($"<td>{errorDetails}</td>");
					stringBuilder.AppendLine("</tr>");
				}

				stringBuilder.AppendLine("</table>");
				stringBuilder.AppendLine("</body>");
				stringBuilder.AppendLine("</html>");

				return stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				throw new SmokeTestException("An error occured when building email body.", ex);
			}
		}

		public override string Name => Constants.SmokeTestAnalysisAgentName;
	}
}
