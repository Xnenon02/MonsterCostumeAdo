using MonsterCostumeAdo.Data;
using MonsterCostumeAdo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCostumeAdo.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var context = new MonsterCostumeContext();

        // Create database if not exists
        context.Database.EnsureCreated();

        // Seed data only if empty
        if (context.Costumes.Any()) return;

        var costumes = new List<Costume>
        {
            new() { Name = "Night Stalker Cloak", Monster = "Vampire", Stock = 12, Price = 149.90m },
            new() { Name = "Lunar Howler Pelt", Monster = "Werewolf", Stock = 8, Price = 179.50m },
            new() { Name = "Spectral Veil", Monster = "Ghost", Stock = 20, Price = 89.00m }
        };

        context.Costumes.AddRange(costumes);
        context.SaveChanges();

        var sales = new List<CostumeSale>
        {
            new() { CostumeId = costumes[0].Id, CustomerName = "Count Mortimer", Quantity = 2, SaleDate = DateTime.Today.AddDays(-2), Notes = "Extra långa ärmar" },
            new() { CostumeId = costumes[1].Id, CustomerName = "Lady Lycan", Quantity = 1, SaleDate = DateTime.Today.AddDays(-1) },
            new() { CostumeId = costumes[2].Id, CustomerName = "Casper Jr.", Quantity = 3, SaleDate = DateTime.Today, Notes = "Behöver fler nitar" }
        };

        context.CostumeSales.AddRange(sales);
        context.SaveChanges();

        Console.WriteLine("✅ Database seeded successfully with EF Core!");
    }
}

