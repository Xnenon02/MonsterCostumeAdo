using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MonsterCostumeAdo.Data;
using Microsoft.EntityFrameworkCore;

namespace MonsterCostumeAdo.Services;

public static class DisplayService
{
    public static void ShowCostumes()
    {
        using var context = new MonsterCostumeContext();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n=== KOSTYMER I LAGER ===");
        Console.ResetColor();

        var costumes = context.Costumes
            .OrderBy(c => c.Name)
            .ToList();

        foreach (var c in costumes)
        {
            Console.WriteLine($"[{c.Id}] {c.Name} ({c.Monster}) - {c.Stock} st - {c.Price:0.00} gc");
        }
    }

    public static void ShowSales()
    {
        using var context = new MonsterCostumeContext();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n=== GJORDA FÖRSÄLJNINGAR ===");
        Console.ResetColor();

        var sales = context.CostumeSales
            .Include(s => s.Costume)
            .OrderByDescending(s => s.SaleDate)
            .ToList();

        foreach (var s in sales)
        {
            var total = s.Costume != null ? s.Costume.Price * s.Quantity : 0;
            var notes = string.IsNullOrWhiteSpace(s.Notes) ? "-" : s.Notes;

            Console.WriteLine(
                $"[{s.Id}] {s.SaleDate:yyyy-MM-dd}: {s.CustomerName} köpte {s.Quantity} st {s.Costume?.Name} " +
                $"(summa {total:0.00} gc)" +
                (notes == "-" ? string.Empty : $" – Notering: {notes}"));
        }
    }
}

