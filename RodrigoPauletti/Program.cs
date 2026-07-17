using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var database = new List<(string, string, int, long)>();

app.MapGet("/itens", () => {
    if (database.ToArray().Length == 0) {
        return String.Concat("Empty database.");
    }
    List<string> result = new List<string>();
    result.Add(String.Concat("List of itens:"));
    foreach ((string, string, int, long) item in database) {
        var itemTimestampFormatted = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(item.Item4).ToLocalTime();
        result.Add(String.Concat("Key: ", item.Item1, "\nValue: ", item.Item2, "\nTTL: ", item.Item3, "\nExpires in: ", itemTimestampFormatted));
    }
    return String.Join("\n\n", result);
});

app.MapGet("/itens/{itemKey}", (string itemKey) => {
    var itemFounded = database.Any(pair => pair.Item1 == itemKey);
    if (!itemFounded) {
        return String.Concat("Item ", itemKey, " not found!");
    }
    var itemValue = database.First(pair => pair.Item1 == itemKey).Item2;
    var itemTtl = database.First(pair => pair.Item1 == itemKey).Item3;
    var itemTimestamp = database.First(pair => pair.Item1 == itemKey).Item4;
    var itemTimestampFormatted = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(itemTimestamp).ToLocalTime();
    return String.Concat("Item ", itemKey, " founded!\nValue: ", itemValue, "\nTTL: ", itemTtl, "\nExpires in: ", itemTimestampFormatted);
});

app.MapPost("/itens", async (CreateItemRequest request) => {
    var itemKey = request.Key;
    if (database.Any(pair => pair.Item1 == itemKey)) {
        return String.Concat("The item ", itemKey, " already exists.");
    }

    var itemValue = request.Value;
    var itemTtl = request.Ttl;
    var itemTimestamp = new DateTimeOffset(DateTime.Now.AddSeconds(itemTtl)).ToUnixTimeSeconds();
    database.Add((itemKey, itemValue, itemTtl, itemTimestamp));

    return String.Concat("The item ", itemKey ," was added with the value ", itemValue, " and TTL ", itemTtl,".");
});

app.MapDelete("/itens/{itemKey}", (string itemKey) => {
    var itemFounded = database.Any(pair => pair.Item1 == itemKey);
    if (!itemFounded) {
        return String.Concat("Item ", itemKey, " not found.");
    }
    var itemIndex = database.FindIndex(pair => pair.Item1 == itemKey);
    database.RemoveAt(itemIndex);
    return String.Concat("Item ", itemKey, " was removed.");
});

app.Run();