namespace Storm.Api.Swaggers
{
	public class SwaggerModuleDescription
	{
		public SwaggerModuleDescription(string moduleName, params string[] matchExpressions)
		{
			MatchExpressions = matchExpressions;
			ModuleName = moduleName;
		}

		internal string[] MatchExpressions { get; }

		internal string ModuleName { get; }
	}
}