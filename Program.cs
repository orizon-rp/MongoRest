using System.Runtime.InteropServices;
using MongoDB.Driver;
using MongoRest.Converters;

var builder = WebApplication.CreateBuilder(args);

var connectionString = string.Empty;
var dbName = string.Empty;

#region Environment Variables

void EnsureArgs()
{
    var options = args.Select(arg => arg.Split('=')).ToDictionary(split => split[0].Trim('-'), split => split[1]);

    options.TryGetValue("MONGO_URL", out connectionString);
    options.TryGetValue("MONGO_DB_NAME", out dbName);

    if (string.IsNullOrEmpty(connectionString))
        throw new Exception("MONGO_URL command line argument is not set.");

    if (string.IsNullOrEmpty(dbName))
        throw new Exception("MONGO_DB_NAME command line argument is not set.");
}

void EnsureEnv()
{
    connectionString = Environment.GetEnvironmentVariable("MONGO_URL")!;
    dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME")!;

    if (string.IsNullOrEmpty(connectionString))
        throw new Exception("MONGO_URL environment variable is not set.");

    if (string.IsNullOrEmpty(dbName))
        throw new Exception("MONGO_DB_NAME environment variable is not set.");
}

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    EnsureArgs();
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // If the app is running in a docker container
    if (File.Exists("/.dockerenv")) EnsureEnv();
    // If the app is running locally
    else EnsureArgs();
}

#endregion

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(connectionString));

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(dbName);
});

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new BsonDocumentJsonConverter()); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();

public partial class Program
{
}