var builder = DistributedApplication.CreateBuilder(args);

var db = builder
    .AddPostgres("postgres", password: builder.AddParameter("password", "postgres"))
    .WithPgAdmin()
    .WithDataVolume("data")
    .AddDatabase("db");

builder.AddProject<Projects.EasterCMS>("eastercms")
    .WithReference(db).WaitFor(db);

builder.Build().Run();
