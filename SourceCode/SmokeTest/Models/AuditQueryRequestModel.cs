using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmokeTest.Models
{
	public class AuditQueryRequestModel
	{
		public Request request { get; set; }
		public int start { get; set; }
		public int length { get; set; }
	}

	public class Request
	{
		public Objecttype objectType { get; set; }
		public Field[] fields { get; set; }
		public string condition { get; set; }
		public string rowCondition { get; set; }
		public object[] sorts { get; set; }
		public object relationalField { get; set; }
		public object searchProviderCondition { get; set; }
		public bool includeIdWindow { get; set; }
		public bool convertNumberFieldValuesToString { get; set; }
		public bool isAdHocQuery { get; set; }
		public object activeArtifactId { get; set; }
		public object queryHint { get; set; }
		public int executingViewId { get; set; }
	}

	public class Objecttype
	{
		public int artifactTypeID { get; set; }
	}

	public class Field
	{
		public string Name { get; set; }
		public object[] Guids { get; set; }
		public int ArtifactID { get; set; }
	}

}
