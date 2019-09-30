using Relativity.Services.ServiceProxy;
using System;

namespace SmokeTest.Tests.Integration
{
	public class TestConstants
	{
		public static readonly string ServerName = "192.168.21.232";
		public static readonly int WorkspaceArtifactId = 1017457;
		public static readonly string RelativityLogin = "relativity.admin@relativity.com";
		public static readonly string RelativityPassword = "Test1234!";
		public static readonly string SqlLogin = "eddsdbo";
		public static readonly string SqlPassword = "Test1234!";
		public static readonly Uri RsapiUri = new Uri($"http://{TestConstants.ServerName}/Relativity.Services");
		public static readonly Uri RestUri = new Uri($"http://{TestConstants.ServerName}/Relativity.Rest/Api");
		public static readonly string RelativityUrl = $"http://{TestConstants.ServerName}/Relativity";
		public static readonly UsernamePasswordCredentials RelativityUsernamePasswordCredentials = new UsernamePasswordCredentials(TestConstants.RelativityLogin, TestConstants.RelativityPassword);
		public static readonly ServiceFactorySettings ServiceFactorySettings = new ServiceFactorySettings(TestConstants.RsapiUri, TestConstants.RestUri,
			TestConstants.RelativityUsernamePasswordCredentials);
	}
}
