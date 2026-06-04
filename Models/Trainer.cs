using System.ComponentModel.DataAnnotations;

namespace FitnessCentar.Models
{
    public class Trainer
    {
        // Primarni ključ u tabeli Trainers.
        public int TrainerId { get; set; }

        //Quality of life error detection radi izbegavanja besmislenih podataka.

        [Required(ErrorMessage = "Ime trenera je obavezno.")]
        [StringLength(50, ErrorMessage = "Ime može imati najviše 50 karaktera.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime trenera je obavezno.")]
        [StringLength(50, ErrorMessage = "Prezime može imati najviše 50 karaktera.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specijalizacija je obavezna.")]
        [StringLength(100, ErrorMessage = "Specijalizacija može imati najviše 100 karaktera.")]
        public string Specialization { get; set; } = string.Empty; //Pocetna vrednost je prazan string.

        [Range(0, 50, ErrorMessage = "Godine iskustva moraju biti između 0 i 50.")]
        public int YearsOfExperience { get; set; }

        // Navigaciono svojstvo: jedan trener može imati više klijenata.
        public ICollection<Client> Clients { get; set; } = new List<Client>();

        // Koristi se za lep prikaz trenera u ComboBox kontroli.
        public override string ToString()
        {
            return $"{FirstName} {LastName} - {Specialization}";
        }
    }
}