var builder = DistributedApplication.CreateBuilder(args);

// Run Azurite via npx instead of container
var storage = builder.AddExecutable("azurite", "npx", ".", "azurite", "--silent", "--location", ".azurite", "--debug", ".azurite/debug.log");

builder.AddAzureFunctionsProject<Projects.Sample>("api")
       .WaitFor(storage)
       .WithEnvironment("AzureWebJobsStorage", "UseDevelopmentStorage=true")
       // Disable Azure SDK telemetry since we're running locally
       .WithEnvironment("AZURE_SDK_TELEMETRY_ENABLED", "false")
       .WithExternalHttpEndpoints();

builder.Build().Run();
