#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Xunit;

public class SecretsFactAttribute : FactAttribute
{
    public static IConfiguration Configuration { get; set; } = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddUserSecrets<SecretsFactAttribute>()
        .Build();

    public SecretsFactAttribute(params string[] secrets)
    {
        var missing = new HashSet<string>();

        foreach (var secret in secrets)
        {
            if (string.IsNullOrEmpty(Configuration[secret]))
                missing.Add(secret);
        }

        if (missing.Count > 0)
            Skip = "Missing user secrets: " + string.Join(',', missing);
    }
}

public class LocalFactAttribute : SecretsFactAttribute
{
    public LocalFactAttribute(params string[] secrets) : base(secrets)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            Skip = "Non-CI test";
    }
}

public class CIFactAttribute : FactAttribute
{
    public CIFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            Skip = "CI-only test";
    }
}

public class RuntimeFactAttribute : FactAttribute
{
    /// <summary>
    /// Use <c>nameof(OSPLatform.Windows|Linux|OSX|FreeBSD)</c>
    /// </summary>
    public RuntimeFactAttribute(string osPlatform)
    {
        if (osPlatform != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Create(osPlatform)))
            Skip = $"Only running on {osPlatform}.";
    }

    public RuntimeFactAttribute(Architecture architecture)
    {
        if (RuntimeInformation.ProcessArchitecture != architecture)
            Skip = $"Requires {architecture} but was {RuntimeInformation.ProcessArchitecture}.";
    }

    /// <summary>
    /// Empty constructor for use in combination with RuntimeIdentifier property.
    /// </summary>
    public RuntimeFactAttribute() { }

    /// <summary>
    /// Sets the runtime identifier the test requires to run.
    /// </summary>
    public string? RuntimeIdentifier
    {
        get => RuntimeInformation.RuntimeIdentifier;
        set
        {
            if (value != null && RuntimeInformation.RuntimeIdentifier != value)
                Skip += $"Requires {value} but was {RuntimeInformation.RuntimeIdentifier}.";
        }
    }
}

public class RuntimeTheoryAttribute : TheoryAttribute
{
    /// <summary>
    /// Use <c>nameof(OSPLatform.Windows|Linux|OSX|FreeBSD)</c>
    /// </summary>
    public RuntimeTheoryAttribute(string osPlatform)
    {
        if (osPlatform != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Create(osPlatform)))
            Skip = $"Only running on {osPlatform}.";
    }

    public RuntimeTheoryAttribute(Architecture architecture)
    {
        if (RuntimeInformation.ProcessArchitecture != architecture)
            Skip = $"Requires {architecture} but was {RuntimeInformation.ProcessArchitecture}.";
    }

    /// <summary>
    /// Empty constructor for use in combination with RuntimeIdentifier property.
    /// </summary>
    public RuntimeTheoryAttribute() { }

    /// <summary>
    /// Sets the runtime identifier the test requires to run.
    /// </summary>
    public string? RuntimeIdentifier
    {
        get => RuntimeInformation.RuntimeIdentifier;
        set
        {
            if (value != null && RuntimeInformation.RuntimeIdentifier != value)
                Skip += $"Requires {value} but was {RuntimeInformation.RuntimeIdentifier}.";
        }
    }
}

public class SecretsTheoryAttribute : TheoryAttribute
{
    public SecretsTheoryAttribute(params string[] secrets)
    {
        var missing = new HashSet<string>();

        foreach (var secret in secrets)
        {
            if (string.IsNullOrEmpty(SecretsFactAttribute.Configuration[secret]))
                missing.Add(secret);
        }

        if (missing.Count > 0)
            Skip = "Missing user secrets: " + string.Join(',', missing);
    }
}

public class LocalTheoryAttribute : SecretsTheoryAttribute
{
    public LocalTheoryAttribute(params string[] secrets) : base(secrets)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            Skip = "Non-CI test";
    }
}

public class CITheoryAttribute : SecretsTheoryAttribute
{
    public CITheoryAttribute(params string[] secrets) : base(secrets)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
            Skip = "CI-only test";
    }
}

public class DebuggerFactAttribute : FactAttribute
{
    public DebuggerFactAttribute()
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            Skip = "Only running in the debugger";
    }
}

public class DebuggerTheoryAttribute : TheoryAttribute
{
    public DebuggerTheoryAttribute()
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            Skip = "Only running in the debugger";
    }
}
