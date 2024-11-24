using MongoDB.Driver;
using MongoRest.Converters;

var builder = WebApplication.CreateBuilder(args);

#region Environment Variables

var appPort = Environment.GetEnvironmentVariable("APP_PORT") ?? "443";
var connectionString = Environment.GetEnvironmentVariable("MONGO_URL") ?? "mongodb://localhost:27017";
var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "MongoRest";

Console.WriteLine("App running on port " + appPort);
Console.WriteLine("MongoDB connection url" + connectionString);
Console.WriteLine("MongoDB database name" + dbName);

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

app.Urls.Add($"https://+:{appPort}");

app.Run();

// Only used for unit tests
public partial class Program
{
}
