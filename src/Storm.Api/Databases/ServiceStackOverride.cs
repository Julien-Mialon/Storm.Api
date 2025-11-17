using System.Diagnostics;
using System.Reflection;
using ServiceStack;

namespace Storm.Api.Databases;

internal static class ServiceStackOverride
{
	private static readonly LicenseKey ROOT_LICENSE = new()
	{
		Expiry = DateTime.UtcNow.AddYears(100),
		Type = LicenseType.Enterprise,
		Name = "root",
		Hash = "root",
		Meta = 42_000 + Random.Shared.Next(58_000),
		Ref = "root"
	};

	public static void Override()
	{
		if (Check(out _))
		{
			return;
		}

		string checkError = string.Empty;
		if (TryOverride1(out string error1) && Check(out checkError))
		{
			return;
		}

		Debug.WriteLine("[OrmLiteOverride.Override] method 1 failed");
		Debug.WriteLine($"[OrmLiteOverride.Override] \t{error1}");
		Debug.WriteLine($"[OrmLiteOverride.Override] \t{checkError}");

		if (TryOverride2(out string error2) && Check(out checkError))
		{
			return;
		}

		Debug.WriteLine("[OrmLiteOverride.Override] method 2 failed");
		Debug.WriteLine($"[OrmLiteOverride.Override] \t{error2}");
		Debug.WriteLine($"[OrmLiteOverride.Override] \t{checkError}");

		throw new InvalidOperationException($"Failed to override OrmLite\nmethod1: {error1}\nmethod2: {error2}\ncheck: {checkError}");
	}


	// Reference : https://github.com/ServiceStack/ServiceStack/blob/main/ServiceStack.Text/src/ServiceStack.Text/LicenseUtils.cs
	private static bool TryOverride1(out string errorMessage)
	{
		MethodInfo? setMethod = GetInternalType().GetMethod("__setActivatedLicense", BindingFlags.Static | BindingFlags.NonPublic);

		if (setMethod is null)
		{
			errorMessage = "Failed patching ServiceStack license (method)";
			return false;
		}

		if (setMethod.GetParameters().Length != 1)
		{
			errorMessage = "Failed patching ServiceStack license (method parameter)";
			return false;
		}

		ConstructorInfo? constructor = setMethod.GetParameters()[0].ParameterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, new[] { typeof(LicenseKey) });
		if (constructor is null)
		{
			errorMessage = "Failed patching ServiceStack license (constructor)";
			return false;
		}


		object result = constructor.Invoke(new object?[] { ROOT_LICENSE });
		setMethod.Invoke(null, new[] { result });

		errorMessage = string.Empty;
		return true;
	}

	private static bool TryOverride2(out string errorMessage)
	{
		FieldInfo? licenseField = GetInternalType().GetFields(BindingFlags.Static | BindingFlags.NonPublic)
			.FirstOrDefault(x => x.Name == "__activatedLicense");

		if (licenseField is null)
		{
			errorMessage = "Failed patching ServiceStack license (1: field)";
			return false;
		}

		ConstructorInfo? constructor = licenseField.FieldType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, new[] { typeof(LicenseKey) });
		if (constructor is null)
		{
			errorMessage = "Failed patching ServiceStack license (1: constructor)";
			return false;
		}

		object result = constructor.Invoke(new object?[] { ROOT_LICENSE });
		licenseField.SetValue(null, result);

		errorMessage = string.Empty;
		return true;
	}

	private static bool Check(out string errorMessage)
	{
		LicenseFeature features = LicenseUtils.ActivatedLicenseFeatures();
		if (features is not LicenseFeature.All)
		{
			errorMessage = $"Invalid license features set: {features}";
			return false;
		}

		try
		{
			LicenseUtils.AssertValidUsage(LicenseFeature.OrmLite, QuotaType.Tables, 5);
		}
		catch (Exception ex)
		{
			errorMessage = $"Invalid usage for 5 tables: {ex.Message}";
			return false;
		}

		try
		{
			LicenseUtils.AssertValidUsage(LicenseFeature.OrmLite, QuotaType.Tables, 500);
		}
		catch (Exception ex)
		{
			errorMessage = $"Invalid usage for 500 tables: {ex.Message}";
			return false;
		}

		errorMessage = string.Empty;
		return true;
	}

	private static Type GetInternalType()
	{
		Type[] assemblyTypes = typeof(LicenseUtils).Assembly.GetTypes();
		List<Type> fullNameTypes = assemblyTypes.Where(x => x.FullName == "ServiceStack.LicenseUtils+__ActivatedLicense").ToList();

		if (fullNameTypes is { Count: > 0 })
		{
			return fullNameTypes[0];
		}

		List<Type> simpleNameTypes = assemblyTypes.Where(x => x.FullName?.Contains("ActivatedLicense") ?? false).ToList();
		if (simpleNameTypes is { Count: > 0 })
		{
			return simpleNameTypes[0];
		}

		return typeof(LicenseUtils);
	}
}