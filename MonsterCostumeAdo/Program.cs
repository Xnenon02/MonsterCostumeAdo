using Microsoft.Data.Sqlite;
using MonsterCostumeAdo.Data;
using MonsterCostumeAdo.Services;
using MonsterCostumeAdo.Models;



namespace MonsterCostumeAdo;

internal static class Program
{
    private static readonly string ConnectionString =
        new SqliteConnectionStringBuilder { DataSource = "monster_costume.db" }.ToString();

    private static void Main()
    {
        DatabaseInitializer.Initialize(); // EF Core handles DB + seeding now
        RunMenu(); // Keep your old menu working
    }


    private static void RunMenu()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n=== MONSTER COSTUME EMPORIUM ===");
            Console.ResetColor();
            Console.WriteLine("1. Visa kostymer i lager");
            Console.WriteLine("2. Visa gjorda försäljningar");
            Console.WriteLine("3. Registrera ny försäljning");
            Console.WriteLine("4. Intäktsöversikt per kostym");
            Console.WriteLine("5. Filtrera försäljningar efter datum");
            Console.WriteLine("6. Lagerlarm (lågt saldo)");
            Console.WriteLine("0. Avsluta");
            Console.Write("Val: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1": DisplayService.ShowCostumes(); break;
                case "2": DisplayService.ShowSales(); break;
                case "3": SaleService.RegisterSale(); break;
                case "4": ReportService.ShowRevenuePerCostume(); break;
                case "5": ReportService.FilterSalesByDate(); break;
                case "6": ReportService.ShowLowStockAlert(); break;
                case "0":
                    Console.WriteLine("Hej då och lycka till på Allhelgona!");
                    return;
                default:
                    Console.WriteLine("Ogiltigt val, försök igen.");
                    break;
            }
        }
    }

    private static void EnsureDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        const string createCostumes =
            """
            CREATE TABLE IF NOT EXISTS Costumes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Monster TEXT NOT NULL,
                Stock INTEGER NOT NULL,
                Price REAL NOT NULL
            );
            """;

        const string createSales =
            """
            CREATE TABLE IF NOT EXISTS CostumeSales (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CostumeId INTEGER NOT NULL,
                CustomerName TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                SaleDate TEXT NOT NULL,
                Notes TEXT,
                FOREIGN KEY (CostumeId) REFERENCES Costumes(Id)
            );
            """;

        using var createCostumesCmd = connection.CreateCommand();
        createCostumesCmd.CommandText = createCostumes;
        createCostumesCmd.ExecuteNonQuery();

        using var createSalesCmd = connection.CreateCommand();
        createSalesCmd.CommandText = createSales;
        createSalesCmd.ExecuteNonQuery();
    }

    private static void SeedDatabaseIfEmpty()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Costumes";
        var costumeCount = Convert.ToInt32(countCommand.ExecuteScalar());

        if (costumeCount > 0)
        {
            return;
        }

        using var transaction = connection.BeginTransaction();

        var insertCostume = connection.CreateCommand();
        insertCostume.Transaction = transaction;
        insertCostume.CommandText =
            "INSERT INTO Costumes (Name, Monster, Stock, Price) VALUES (@name, @monster, @stock, @price)";

        AddCostume(insertCostume, "Night Stalker Cloak", "Vampire", 12, 149.90m);
        AddCostume(insertCostume, "Lunar Howler Pelt", "Werewolf", 8, 179.50m);
        AddCostume(insertCostume, "Spectral Veil", "Ghost", 20, 89.00m);

        var insertSale = connection.CreateCommand();
        insertSale.Transaction = transaction;
        insertSale.CommandText =
            "INSERT INTO CostumeSales (CostumeId, CustomerName, Quantity, SaleDate, Notes) VALUES (@id, @customer, @qty, @date, @notes)";

        AddSale(insertSale, 1, "Count Mortimer", 2, DateTime.Today.AddDays(-2), "Extra långa ärmar");
        AddSale(insertSale, 2, "Lady Lycan", 1, DateTime.Today.AddDays(-1), null);
        AddSale(insertSale, 3, "Casper Jr.", 3, DateTime.Today, "Behöver fler nitar");

        transaction.Commit();
    }

    private static void AddCostume(SqliteCommand command, string name, string monster, int stock, decimal price)
    {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@monster", monster);
        command.Parameters.AddWithValue("@stock", stock);
        command.Parameters.AddWithValue("@price", price);
        command.ExecuteNonQuery();
    }

    private static void AddSale(SqliteCommand command, int costumeId, string customer, int quantity, DateTime date, string? notes)
    {
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@id", costumeId);
        command.Parameters.AddWithValue("@customer", customer);
        command.Parameters.AddWithValue("@qty", quantity);
        command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@notes", notes is null ? DBNull.Value : notes);
        command.ExecuteNonQuery();
    }

    private static void ShowCostumes()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        const string sql =
            "SELECT Id, Name, Monster, Stock, Price FROM Costumes ORDER BY Name";

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n=== KOSTYMER I LAGER ===");
        Console.ResetColor();

        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var name = reader.GetString(1);
            var monster = reader.GetString(2);
            var stock = reader.GetInt32(3);
            var price = Convert.ToDecimal(reader.GetDouble(4));

            Console.WriteLine($"[{id}] {name} ({monster}) - {stock} st - {price:0.00} gc");
        }
    }

    private static void ShowSales()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        const string sql =
            """
            SELECT s.Id, c.Name, s.CustomerName, s.Quantity, s.SaleDate, s.Notes, c.Price
            FROM CostumeSales s
            JOIN Costumes c ON c.Id = s.CostumeId
            ORDER BY s.SaleDate DESC;
            """;

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n=== GJORDA FÖRSÄLJNINGAR ===");
        Console.ResetColor();

        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var costumeName = reader.GetString(1);
            var customer = reader.GetString(2);
            var quantity = reader.GetInt32(3);
            var date = DateTime.Parse(reader.GetString(4));
            var notes = reader.IsDBNull(5) ? "-" : reader.GetString(5);
            var price = Convert.ToDecimal(reader.GetDouble(6));
            var total = price * quantity;

            Console.WriteLine(
                $"[{id}] {date:yyyy-MM-dd}: {customer} köpte {quantity} st {costumeName} (summa {total:0.00} gc)" +
                (notes == "-" ? string.Empty : $" – Notering: {notes}"));
        }
    }

    private static void RegisterSale()
    {
        Console.Write("\nVilken kostym (Id) vill kunden köpa? ");
        if (!int.TryParse(Console.ReadLine(), out var costumeId))
        {
            Console.WriteLine("Du måste skriva ett giltigt Id.");
            return;
        }

        Console.Write("Hur många exemplar? ");
        if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Antalet måste vara ett positivt heltal.");
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

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        // Kontrollera lager
        var stockCommand = connection.CreateCommand();
        stockCommand.CommandText = "SELECT Stock FROM Costumes WHERE Id = @id";
        stockCommand.Parameters.AddWithValue("@id", costumeId);

        var stockResult = stockCommand.ExecuteScalar();
        if (stockResult is null)
        {
            Console.WriteLine("Hittade ingen kostym med det Id:t.");
            return;
        }

        var currentStock = Convert.ToInt32(stockResult);
        if (currentStock < quantity)
        {
            Console.WriteLine("Det finns inte tillräckligt många i lager.");
            return;
        }

        using var transaction = connection.BeginTransaction();

        var updateStock = connection.CreateCommand();
        updateStock.Transaction = transaction;
        updateStock.CommandText = "UPDATE Costumes SET Stock = Stock - @qty WHERE Id = @id";
        updateStock.Parameters.AddWithValue("@qty", quantity);
        updateStock.Parameters.AddWithValue("@id", costumeId);
        updateStock.ExecuteNonQuery();

        var insertSale = connection.CreateCommand();
        insertSale.Transaction = transaction;
        insertSale.CommandText =
            "INSERT INTO CostumeSales (CostumeId, CustomerName, Quantity, SaleDate, Notes) VALUES (@costume, @customer, @qty, @date, @notes)";
        insertSale.Parameters.AddWithValue("@costume", costumeId);
        insertSale.Parameters.AddWithValue("@customer", customer);
        insertSale.Parameters.AddWithValue("@qty", quantity);
        insertSale.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        insertSale.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes);
        insertSale.ExecuteNonQuery();

        transaction.Commit();
        Console.WriteLine("Försäljning sparad!");
    }
}