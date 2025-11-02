using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonsterCostumeAdo.Data;
using Microsoft.EntityFrameworkCore;

namespace MonsterCostumeAdo.Services;

public static class ReportService
{
    // 1️⃣ Totala intäkter per kostym
    public static void ShowRevenuePerCostume()
    {
        using var context = new MonsterCostumeContext();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n=== INTÄKTER PER KOSTYM ===");
        Console.ResetColor();

        var result = context.CostumeSales
            .Include(s => s.Costume)
            .AsEnumerable() // calculate totals in memory
            .GroupBy(s => s.Costume!.Name)
            .Select(g => new
            {
                Costume = g.Key,
                Revenue = g.Sum(x => x.Quantity * x.Costume!.Price),
                Sold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Revenue);

        foreach (var r in result)
        {
            Console.WriteLine($"{r.Costume,-25} | {r.Sold,3} sålda | {r.Revenue,8:0.00} gc");
        }
    }

    // 2️⃣ Filtrera försäljningar på datumintervall
    public static void FilterSalesByDate()
    {
        using var context = new MonsterCostumeContext();

        Console.Write("\nFrån datum (yyyy-MM-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var from))
        {
            Console.WriteLine("❌ Ogiltigt datum.");
            return;
        }

        Console.Write("Till datum (yyyy-MM-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var to))
        {
            Console.WriteLine("❌ Ogiltigt datum.");
            return;
        }

        var sales = context.CostumeSales
            .Include(s => s.Costume)
            .Where(s => s.SaleDate >= from && s.SaleDate <= to)
            .OrderBy(s => s.SaleDate)
            .ToList();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n=== FÖRSÄLJNING {from:yyyy-MM-dd} → {to:yyyy-MM-dd} ===");
        Console.ResetColor();

        if (!sales.Any())
        {
            Console.WriteLine("Inga försäljningar i det intervallet.");
            return;
        }

        foreach (var s in sales)
        {
            var total = s.Costume!.Price * s.Quantity;
            Console.WriteLine($"{s.SaleDate:yyyy-MM-dd} | {s.CustomerName,-15} | {s.Costume!.Name,-25} | {s.Quantity,2} st | {total,8:0.00} gc");
        }

        var grandTotal = sales.Sum(s => s.Costume!.Price * s.Quantity);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nTotalt: {grandTotal:0.00} gc");
        Console.ResetColor();
    }

    // 3️⃣ Lagerlarm
    public static void ShowLowStockAlert()
    {
        using var context = new MonsterCostumeContext();

        Console.Write("\nVisa kostymer under lagergräns (t.ex. 5): ");
        if (!int.TryParse(Console.ReadLine(), out var limit))
        {
            Console.WriteLine("❌ Ogiltigt tal.");
            return;
        }

        var lowStock = context.Costumes
            .Where(c => c.Stock < limit)
            .OrderBy(c => c.Stock)
            .ToList();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n=== KOSTYMER MED LÅGT LAGER (<{limit}) ===");
        Console.ResetColor();

        if (!lowStock.Any())
        {
            Console.WriteLine("✅ Alla kostymer har tillräckligt i lager.");
            return;
        }

        foreach (var c in lowStock)
        {
            Console.WriteLine($"{c.Name,-25} | {c.Stock,2} st kvar");
        }
    }
}

