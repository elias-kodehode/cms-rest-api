var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
	.AddPostgres("postgres", password: builder.AddParameter("password", "postgres"))
	.WithLifetime(ContainerLifetime.Persistent)
	//.WithPgAdmin()
	.WithDataVolume();

var db = postgres
	.AddDatabase("db");

var pgAdmin = postgres.WithPgAdmin().WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.EasterCMS>("eastercms")
	.WithReference(db).WaitFor(db);

builder.Build().Run();
