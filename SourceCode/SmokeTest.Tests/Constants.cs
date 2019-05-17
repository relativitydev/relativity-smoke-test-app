using Relativity.Services.ServiceProxy;
using System;

namespace SmokeTest.Tests
{
	public class Constants
	{
		public static readonly string ServerName = "172.27.139.228";
		public static readonly int WorkspaceArtifactId = 1017512;
		public static readonly string RelativityLogin = "relativity.admin@relativity.com";
		public static readonly string RelativityPassword = "Test1234!";
		public static readonly string SqlLogin = "eddsdbo";
		public static readonly string SqlPassword = "Test1234!";
		public static readonly Uri RsapiUri = new Uri($"http://{Constants.ServerName}/Relativity.Services");
		public static readonly Uri RestUri = new Uri($"http://{Constants.ServerName}/Relativity.Rest/Api");
		public static readonly UsernamePasswordCredentials RelativityUsernamePasswordCredentials = new UsernamePasswordCredentials(Constants.RelativityLogin, Constants.RelativityPassword);
		public static readonly ServiceFactorySettings ServiceFactorySettings = new ServiceFactorySettings(Constants.RsapiUri, Constants.RestUri,
			Constants.RelativityUsernamePasswordCredentials);
	}
}
