using FitnessCentar.Data;
using FitnessCentar.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessCentar.Services
{
    public class FitnessService
    {
        // Učitava sve trenere iz baze i uključuje njihove klijente.
        // Include je potreban jer želimo da prikažemo i povezane podatke iz druge tabele.
        public List<Trainer> GetAllTrainers()
        {
            using var context = new FitnessDbContext();

            return context.Trainers
                .Include(trainer => trainer.Clients)
                .OrderBy(trainer => trainer.LastName)
                .ThenBy(trainer => trainer.FirstName)
                .ToList();
        }

        // Učitava sve klijente zajedno sa trenerom kojem pripadaju.
        // Ovo omogućava prikaz imena trenera u tabeli klijenata.
        public List<Client> GetAllClients()
        {
            using var context = new FitnessDbContext();

            return context.Clients
                .Include(client => client.Trainer)
                .OrderBy(client => client.LastName)
                .ThenBy(client => client.FirstName)
                .ToList();
        }

        // Pronalazi jednog trenera na osnovu primarnog ključa.
        // Koristi se za prikaz detalja ili izmenu postojećeg trenera.
        public Trainer? GetTrainerById(int trainerId)
        {
            using var context = new FitnessDbContext();

            return context.Trainers
                .Include(trainer => trainer.Clients)
                .FirstOrDefault(trainer => trainer.TrainerId == trainerId);
        }

        // Pronalazi jednog klijenta na osnovu primarnog ključa.
        // Uz klijenta se učitava i njegov trener.
        public Client? GetClientById(int clientId)
        {
            using var context = new FitnessDbContext();

            return context.Clients
                .Include(client => client.Trainer)
                .FirstOrDefault(client => client.ClientId == clientId);
        }

        // Dodaje novog trenera u tabelu Trainers.
        public void AddTrainer(Trainer trainer)
        {
            using var context = new FitnessDbContext();

            context.Trainers.Add(trainer);
            context.SaveChanges();
        }

        // Dodaje novog klijenta u tabelu Clients.
        // Pre dodavanja proverava se da li izabrani trener stvarno postoji.
        public void AddClient(Client client)
        {
            using var context = new FitnessDbContext();

            bool trainerExists = context.Trainers
                .Any(trainer => trainer.TrainerId == client.TrainerId);

            if (!trainerExists)
            {
                throw new InvalidOperationException("Izabrani trener ne postoji u bazi.");
            }

            context.Clients.Add(client);
            context.SaveChanges();
        }

        // Menja podatke postojećeg trenera.
        // Prvo se pronalazi originalni zapis iz baze, pa se ažuriraju njegova polja.
        public void UpdateTrainer(Trainer updatedTrainer)
        {
            using var context = new FitnessDbContext();

            var existingTrainer = context.Trainers
                .FirstOrDefault(trainer => trainer.TrainerId == updatedTrainer.TrainerId);

            if (existingTrainer == null)
            {
                throw new InvalidOperationException("Trener nije pronađen u bazi.");
            }

            existingTrainer.FirstName = updatedTrainer.FirstName;
            existingTrainer.LastName = updatedTrainer.LastName;
            existingTrainer.Specialization = updatedTrainer.Specialization;
            existingTrainer.YearsOfExperience = updatedTrainer.YearsOfExperience;

            context.SaveChanges();
        }

        // Menja podatke postojećeg klijenta.
        // Dodatno proverava da li novi izabrani trener postoji u tabeli Trainers.
        public void UpdateClient(Client updatedClient)
        {
            using var context = new FitnessDbContext();

            var existingClient = context.Clients
                .FirstOrDefault(client => client.ClientId == updatedClient.ClientId);

            if (existingClient == null)
            {
                throw new InvalidOperationException("Klijent nije pronađen u bazi.");
            }

            bool trainerExists = context.Trainers
                .Any(trainer => trainer.TrainerId == updatedClient.TrainerId);

            if (!trainerExists)
            {
                throw new InvalidOperationException("Izabrani trener ne postoji u bazi.");
            }

            existingClient.FirstName = updatedClient.FirstName;
            existingClient.LastName = updatedClient.LastName;
            existingClient.Age = updatedClient.Age;
            existingClient.Weight = updatedClient.Weight;
            existingClient.Goal = updatedClient.Goal;
            existingClient.PhoneNumber = updatedClient.PhoneNumber;
            existingClient.MembershipStartDate = updatedClient.MembershipStartDate;
            existingClient.TrainerId = updatedClient.TrainerId;

            context.SaveChanges();
        }

        // Briše trenera samo ako nema dodeljene klijente.
        // Time se čuva referencijalni integritet između tabela Trainers i Clients.
        public void DeleteTrainer(int trainerId)
        {
            using var context = new FitnessDbContext();

            var trainer = context.Trainers
                .Include(trainer => trainer.Clients)
                .FirstOrDefault(trainer => trainer.TrainerId == trainerId);

            if (trainer == null)
            {
                throw new InvalidOperationException("Trener nije pronađen u bazi.");
            }

            if (trainer.Clients.Any())
            {
                throw new InvalidOperationException("Ne možete obrisati trenera koji ima dodeljene klijente.");
            }

            context.Trainers.Remove(trainer);
            context.SaveChanges();
        }

        // Briše klijenta iz tabele Clients.
        public void DeleteClient(int clientId)
        {
            using var context = new FitnessDbContext();

            var client = context.Clients
                .FirstOrDefault(client => client.ClientId == clientId);

            if (client == null)
            {
                throw new InvalidOperationException("Klijent nije pronađen u bazi.");
            }

            context.Clients.Remove(client);
            context.SaveChanges();
        }
    }
}