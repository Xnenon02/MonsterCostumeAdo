using Microsoft.EntityFrameworkCore;
using MonsterCostumeAdo.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MonsterCostumeAdo.Data;

public class MonsterCostumeContext : DbContext
{
    public DbSet<Costume> Costumes { get; set; } = null!;
    public DbSet<CostumeSale> CostumeSales { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=monster_costume.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Costume>()
            .HasMany(c => c.CostumeSales)
            .WithOne(s => s.Costume)
            .HasForeignKey(s => s.CostumeId);
    }
}
