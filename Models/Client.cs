using FitnessCentar.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessCentar.Models
{
    public class Client
    {
        // Primarni ključ u tabeli Clients.
        public int ClientId { get; set; }

        //Quality of life error detection radi izbegavanja besmislenih podataka.

        [Required(ErrorMessage = "Ime klijenta je obavezno.")]
        [StringLength(50, ErrorMessage = "Ime može imati najviše 50 karaktera.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime klijenta je obavezno.")]
        [StringLength(50, ErrorMessage = "Prezime može imati najviše 50 karaktera.")]
        public string LastName { get; set; } = string.Empty;

        [Range(14, 100, ErrorMessage = "Godine klijenta moraju biti između 14 i 100.")]
        public int Age { get; set; }

        [Range(30, 250, ErrorMessage = "Težina mora biti između 30 i 250 kg.")]
        public double Weight { get; set; }

        [Required(ErrorMessage = "Cilj treninga je obavezan.")]
        [StringLength(100, ErrorMessage = "Cilj može imati najviše 100 karaktera.")]
        public string Goal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Broj telefona je obavezan.")]
        [StringLength(30, ErrorMessage = "Broj telefona može imati najviše 30 karaktera.")]
        public string PhoneNumber { get; set; } = string.Empty; //Pocetna vrednost je prazan string.

        public DateTime MembershipStartDate { get; set; } = DateTime.Now;

        // Strani ključ ka treneru.
        public int TrainerId { get; set; }

        // Navigaciono svojstvo: svaki klijent pripada jednom treneru.
        public Trainer? Trainer { get; set; }
    }
}