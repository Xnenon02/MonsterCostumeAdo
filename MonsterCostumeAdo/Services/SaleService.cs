using Microsoft.EntityFrameworkCore;
using MonsterCostumeAdo.Data;
using MonsterCostumeAdo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterCostumeAdo.Services;

public static class SaleService
{
    public static void RegisterSale()
    {
        using var context = new MonsterCostumeContext();

        Console.Write("\nVilken kostym (Id) vill kunden köpa? ");
        if (!int.TryParse(Console.ReadLine(), out var costumeId))
        {
            Console.WriteLine("Du måste skriva ett giltigt Id.");
            return;
        }

        var costume = context.Costumes.FirstOrDefault(c => c.Id == costumeId);
        if (costume == null)
        {
            Console.WriteLine("Hittade ingen kostym med det Id:t.");
            return;
        }

        Console.Write("Hur många exemplar? ");
        if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Antalet måste vara ett positivt heltal.");
            return;
        }

        if (costume.Stock < quantity)
        {
            Console.WriteLine("Det finns inte tillräckligt många i lager.");
            return;
        }

        Console.Write("Kundens namn: ");
        var customer = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(customer))
        {
            Console.WriteLine("Kunden måste ha ett namn.");
            return;
        }

        Console.Write("Eventuella noteringar (valfritt): ");
        var notes = Console.ReadLine();

        // ✅ Reduce stock and save both entities in a transaction-safe way
        costume.Stock -= quantity;

        var sale = new CostumeSale
        {
            CostumeId = costume.Id,
            CustomerName = customer,
            Quantity = quantity,
            SaleDate = DateTime.Now,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        };

        context.CostumeSales.Add(sale);
        context.SaveChanges(); // Saves both stock change + new sale

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nFörsäljning sparad! ({customer} köpte {quantity} st {costume.Name})");
        Console.ResetColor();
    }
}

