namespace Storm.Api.SourceGenerators.Tests.Infrastructure;

/// <summary>Reusable source fragments so individual tests only declare what they exercise.</summary>
internal static class Sources
{
	/// <summary>The using block every generated controller relies on.</summary>
	public const string USINGS = """

		using System;
		using System.Net;
		using System.Threading.Tasks;
		using Microsoft.AspNetCore.Mvc;
		using Storm.Api;
		using Storm.Api.CQRS;
		using Storm.Api.CQRS.Domains.Results;
		using Storm.Api.Controllers;
		using Storm.Api.Dtos;
		using Storm.Api.SourceGenerators.ActionMethods;

		""";

	/// <summary>Wraps a body in the standard usings + a <c>Sample</c> namespace.</summary>
	public static string InNamespace(string body) => USINGS + "\nnamespace Sample;\n" + body;
}
