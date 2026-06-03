using FitnessCentar.Models;
using FitnessCentar.Services;
using System;
using System.Windows;

namespace FitnessCentar
{
    public partial class MainWindow : Window
    {
        private readonly FitnessService _fitnessService;

        public MainWindow()
        {
            InitializeComponent();

            // Servis koristim da logika za rad sa bazom ne bude direktno u prozoru.
            _fitnessService = new FitnessService();

            LoadData();
        }

        // Učitavanje podataka iz baze i osvežavanje oba DataGrid prikaza.
        private void LoadData()
        {
            try
            {
                TrainersDataGrid.ItemsSource = _fitnessService.GetAllTrainers();
                ClientsDataGrid.ItemsSource = _fitnessService.GetAllClients();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Došlo je do greške prilikom učitavanja podataka.\n\n{ex.Message}",
                    "Greška",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private Trainer? GetSelectedTrainer()
        {
            return TrainersDataGrid.SelectedItem as Trainer;
        }

        private Client? GetSelectedClient()
        {
            return ClientsDataGrid.SelectedItem as Client;
        }

        private void AddTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            // Ovaj deo će kasnije otvarati poseban prozor za unos trenera.
            MessageBox.Show(
                "Ovde će biti otvoren prozor za dodavanje trenera.",
                "Dodavanje trenera",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void TrainerDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrainer = GetSelectedTrainer();

            // Korisnik mora prvo da izabere red iz tabele.
            if (selectedTrainer == null)
            {
                MessageBox.Show(
                    "Prvo izaberite trenera iz tabele.",
                    "Nije izabran trener",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MessageBox.Show(
                $"Trener: {selectedTrainer.FirstName} {selectedTrainer.LastName}\n" +
                $"Specijalizacija: {selectedTrainer.Specialization}\n" +
                $"Godine iskustva: {selectedTrainer.YearsOfExperience}\n" +
                $"Broj klijenata: {selectedTrainer.Clients.Count}",
                "Detalji trenera",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void EditTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrainer = GetSelectedTrainer();

            if (selectedTrainer == null)
            {
                MessageBox.Show(
                    "Prvo izaberite trenera kojeg želite da izmenite.",
                    "Nije izabran trener",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            // Kasnije će se ovde proslediti selektovani trener u formu za izmenu.
            MessageBox.Show(
                "Ovde će biti otvoren prozor za izmenu trenera.",
                "Izmena trenera",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void DeleteTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrainer = GetSelectedTrainer();

            if (selectedTrainer == null)
            {
                MessageBox.Show(
                    "Prvo izaberite trenera kojeg želite da obrišete.",
                    "Nije izabran trener",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            // Brisanje je namerno potvrđeno dodatnim dijalogom.
            var result = MessageBox.Show(
                $"Da li ste sigurni da želite da obrišete trenera {selectedTrainer.FirstName} {selectedTrainer.LastName}?",
                "Potvrda brisanja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _fitnessService.DeleteTrainer(selectedTrainer.TrainerId);

                // Posle izmene u bazi ponovo se učitavaju podaci u tabelama.
                LoadData();

                MessageBox.Show(
                    "Trener je uspešno obrisan.",
                    "Uspešno brisanje",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Greška prilikom brisanja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            // Ovaj deo će kasnije otvarati formu za unos novog klijenta.
            MessageBox.Show(
                "Ovde će biti otvoren prozor za dodavanje klijenta.",
                "Dodavanje klijenta",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ClientDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = GetSelectedClient();

            if (selectedClient == null)
            {
                MessageBox.Show(
                    "Prvo izaberite klijenta iz tabele.",
                    "Nije izabran klijent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MessageBox.Show(
                $"Klijent: {selectedClient.FirstName} {selectedClient.LastName}\n" +
                $"Godine: {selectedClient.Age}\n" +
                $"Težina: {selectedClient.Weight} kg\n" +
                $"Cilj: {selectedClient.Goal}\n" +
                $"Telefon: {selectedClient.PhoneNumber}\n" +
                $"Datum članstva: {selectedClient.MembershipStartDate:dd.MM.yyyy}\n" +
                $"Trener: {selectedClient.Trainer}",
                "Detalji klijenta",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void EditClientButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = GetSelectedClient();

            if (selectedClient == null)
            {
                MessageBox.Show(
                    "Prvo izaberite klijenta kojeg želite da izmenite.",
                    "Nije izabran klijent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            // Kasnije će se selektovani klijent proslediti prozoru za izmenu.
            MessageBox.Show(
                "Ovde će biti otvoren prozor za izmenu klijenta.",
                "Izmena klijenta",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = GetSelectedClient();

            if (selectedClient == null)
            {
                MessageBox.Show(
                    "Prvo izaberite klijenta kojeg želite da obrišete.",
                    "Nije izabran klijent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            var result = MessageBox.Show(
                $"Da li ste sigurni da želite da obrišete klijenta {selectedClient.FirstName} {selectedClient.LastName}?",
                "Potvrda brisanja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _fitnessService.DeleteClient(selectedClient.ClientId);

                LoadData();

                MessageBox.Show(
                    "Klijent je uspešno obrisan.",
                    "Uspešno brisanje",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Greška prilikom brisanja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}