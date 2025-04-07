using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

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
