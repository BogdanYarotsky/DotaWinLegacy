﻿using ConsoleTables;
using DotaWin.Data;
using DotaWin.Data.Models;
using DotaWin.Updater.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


var updater = new DotaWinUpdater("B479F4855E8EC7C228DF9045FA77978B");
var updateResult = await updater.RunDailyUpdate();
Console.WriteLine(updateResult);

using var db = new DotaWinDbContext();
var abbadon = await db.Heroes
    .AsNoTracking()
    .Where(h => h.Name == "Pugna")
    .Select(h => new
    {
        h.Name,
        h.Winrate,
        Items = h.HeroItems.Select(i => new
        {
            i.Item.Name,
            i.Item.ItemType,
            i.Item.Price,
            i.Matches,
            i.Winrate,
            AddedWinrate = Math.Round(i.Winrate - h.Winrate, 2),
        })
    })
    .FirstAsync();

Console.WriteLine("Hero: " + abbadon.Name);
Console.WriteLine("Winrate: " + abbadon.Winrate);
var items = abbadon.Items
    .Where(i => i.Price > 500 && i.AddedWinrate > 0 && i.ItemType == DbItem.Type.Core)
    .Select(i => new {i.Name, i.Matches, i.AddedWinrate, WinratePer1000Gold = Math.Round(i.AddedWinrate / i.Price * 1000, 2)})
    .OrderByDescending(item => item.WinratePer1000Gold);
ConsoleTable.From(items).Write();
