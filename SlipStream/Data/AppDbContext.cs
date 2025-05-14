using System;
using Microsoft.EntityFrameworkCore;

namespace SlipStream.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Session> Sessions { get; set; }
    public DbSet<Entities.Telemetry> Telemetries { get; set; }
    public DbSet<Entities.Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        string name = "Anish";
        string password = "Password";
        modelBuilder.Entity<Entities.Vehicle>().HasData(
            new Entities.Vehicle
            {
                Id = 1,
                Name = name,
                Password = password
            }
        );
    }
}

