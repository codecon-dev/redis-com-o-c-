
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var cacheOptions = new MemoryCacheOptions {
    TrackStatistics = true
};
IMemoryCache cache = new MemoryCache(cacheOptions);

app.MapGet("/stats", IResult () => {
    return Results.Ok(cache.GetCurrentStatistics());
});

app.MapDelete("/entry/{key}", IResult (string key) => {
    if (cache.TryGetValue(key, out Entry<string> entry))
    {
        cache.Remove(key);
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.MapGet("/entry/{key}", IResult (string key) =>
{
    if (cache.TryGetValue(key, out Entry<string> entry))
    {
        return Results.Ok(new EntryDTO{
            Key = key,
            Value = entry.Value,
            TTLInSeconds = entry.ExpiresIn.Seconds
        });
    }

    return Results.NotFound();

})
.WithName("GetEntry");

app.MapPost("/entry", IResult (EntryDTO dto) =>
{
    var ttl = TimeSpan.FromSeconds(dto.TTLInSeconds);
    Entry<string> entry = new Entry<string> {
        Value = dto.Value,
        ExpirationUTC = DateTime.UtcNow.Add(ttl)
    };
    var cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(ttl);

    cache.Set(dto.Key, entry, cacheEntryOptions);

    return Results.NoContent();
})
.WithName("InsertEntry");

app.Run();
