using MongoDB.Driver;
using MongoRest.Converters;

var builder = WebApplication.CreateBuilder(args);

#region Environment Variables

var connectionString = Environment.GetEnvironmentVariable("MONGO_URL");

if (string.IsNullOrEmpty(connectionString))
    throw new Exception("MONGO_URL environment variable is not set.");

var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME");

if (string.IsNullOrEmpty(dbName))
    throw new Exception("MONGO_DB_NAME environment variable is not set.");

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

public partial class Program
{
}