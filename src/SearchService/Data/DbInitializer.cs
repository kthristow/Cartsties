using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task Initialize(WebApplication app)
    {
        var instance = await DB.InitAsync("SearchDb", MongoClientSettings
             .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await instance.Index<Item>()
            .Key(i => i.Make, KeyType.Text)
            .Key(i => i.Model, KeyType.Text)
            .Key(i => i.Color, KeyType.Text)
            .CreateAsync();

        var count = await instance.CountAsync<Item>();

        using var scope = app.Services.CreateScope();
        var auctionClient = scope.ServiceProvider.GetRequiredService<Services.AuctionSvcHttpClient>();
        var items = await auctionClient.GetItemsForSearchDB();
        await instance.SaveAsync(items);
    }
}
