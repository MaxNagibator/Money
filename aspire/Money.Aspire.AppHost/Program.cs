using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var apiProject = builder.AddProject<Money_Api>("api", launchProfileName: "aspire")
    .WithExternalHttpEndpoints()
    .WithEnvironment("AUTO_MIGRATE", "true")
    .WithReplicas(1);

if (Environment.GetEnvironmentVariable("ASPIRE_POSTGRES") == "true")
{
    var postgres = builder.AddPostgres("money")
        .WithDataVolume(isReadOnly: false);
    var postgresdb = postgres.AddDatabase("ApplicationDbContext");
    apiProject.WithReference(postgresdb);
}

var webProject = builder.AddProject<Money_Web>("web", launchProfileName: "aspire")
    .WithExternalHttpEndpoints()
    .WithReference(apiProject);


var webEndpoint = webProject.GetEndpoint("https");
apiProject.WithEnvironment("CORS_ORIGIN", webEndpoint);

builder.Build().Run();
