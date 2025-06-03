IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("contentplatform-db")
    // Currently adding a data volume causes a bug, so it's commented out
    //.WithDataVolume()
    .WithPgAdmin();

var rabbitMq = builder.AddRabbitMQ("contentplatform-mq")
    .WithManagementPlugin();
var cache = builder.AddRedis("contentplatform-cache");

builder.AddProject<Projects.IotPlatform_Api>("contentplatform-api")
    .WithReference(postgres)
    .WithReference(rabbitMq)
    .WithReference(cache)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(cache)
    ;

builder.AddProject<Projects.IotPlatform_Reporting_Api>("contentplatform-reporting-api")
    .WithReference(postgres)
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(cache);

builder.AddProject<Projects.IotPlatform_Presentation>("contentplatform-presentation");

builder.Build().Run();
