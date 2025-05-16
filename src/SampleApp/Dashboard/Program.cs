var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.Sample>("api")
       // Functions App will launch with default port, not the launchSettings.json port
       // So we request a proxy to forward 4242 to the default function app port instead.
       // This will only work if you have a single Function App.
       // See https://github.com/dotnet/aspire/issues/8589
       //.WithArgs("--port", "4242")
       .WithHttpEndpoint(4242, 7071, "apiproxy")
       .WithEnvironment("AzureWebJobsStorage", "UseDevelopmentStorage=true")
       // Disable Azure SDK telemetry since we're running locally
       .WithEnvironment("AZURE_SDK_TELEMETRY_ENABLED", "false")
       .WithExternalHttpEndpoints();

builder.Build().Run();
