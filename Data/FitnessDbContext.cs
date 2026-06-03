using FitnessCentar.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessCentar.Data
{
    public class FitnessDbContext : DbContext
    {
        // Tabele u bazi podataka.
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // LocalDB baza za studentski projekat.
            // Baza će se automatski napraviti kroz Entity Framework migracije.
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=FitnessCenterDb;Trusted_Connection=True;TrustServerCertificate=True;"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Definisanje relacije 1-N:
            // jedan trener ima više klijenata,
            // jedan klijent ima tačno jednog trenera.
            modelBuilder.Entity<Trainer>()
                .HasMany(trainer => trainer.Clients)
                .WithOne(client => client.Trainer)
                .HasForeignKey(client => client.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed podaci za trenere.
            // Ovo omogućava da baza odmah ima početne podatke.
            modelBuilder.Entity<Trainer>().HasData(
                new Trainer
                {
                    TrainerId = 1,
                    FirstName = "Filip",
                    LastName = "Komarica",
                    Specialization = "Strength Training",
                    YearsOfExperience = 8
                },
                new Trainer
                {
                    TrainerId = 2,
                    FirstName = "Jovana",
                    LastName = "Jokic",
                    Specialization = "Weight Loss",
                    YearsOfExperience = 5
                },
                new Trainer
                {
                    TrainerId = 3,
                    FirstName = "Nikola",
                    LastName = "Markovic",
                    Specialization = "Functional Training",
                    YearsOfExperience = 6
                }
            );

            // Seed podaci za klijente.
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    ClientId = 1,
                    FirstName = "Nikola",
                    LastName = "Jadzic",
                    Age = 23,
                    Weight = 82.5,
                    Goal = "Muscle Gain",
                    PhoneNumber = "0601234567",
                    MembershipStartDate = new DateTime(2026, 1, 10),
                    TrainerId = 1
                },
                new Client
                {
                    ClientId = 2,
                    FirstName = "Nikola",
                    LastName = "Danicic",
                    Age = 29,
                    Weight = 64.0,
                    Goal = "Weight Loss",
                    PhoneNumber = "0619876543",
                    MembershipStartDate = new DateTime(2026, 2, 5),
                    TrainerId = 2
                },
                new Client
                {
                    ClientId = 3,
                    FirstName = "Dalibor",
                    LastName = "Vitorovic",
                    Age = 31,
                    Weight = 91.2,
                    Goal = "Conditioning",
                    PhoneNumber = "062555444",
                    MembershipStartDate = new DateTime(2026, 3, 15),
                    TrainerId = 3
                }
            );
        }
    }
}