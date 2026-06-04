using FitnessCentar.Models;
using FitnessCentar.Services;
using FitnessCentar.Views;
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
            var window = new AddEditTrainerWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _fitnessService.AddTrainer(window.Trainer);

                    LoadData();

                    MessageBox.Show(
                        "Trener je uspešno dodat.",
                        "Uspešno dodavanje",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Greška prilikom dodavanja",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void TrainerDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrainer = GetSelectedTrainer();

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

            var window = new TrainerDetailsWindow(selectedTrainer)
            {
                Owner = this
            };

            window.ShowDialog();
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

            var window = new AddEditTrainerWindow(selectedTrainer)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _fitnessService.UpdateTrainer(window.Trainer);

                    LoadData();

                    MessageBox.Show(
                        "Podaci o treneru su uspešno izmenjeni.",
                        "Uspešna izmena",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Greška prilikom izmene",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
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
            var window = new AddEditClientWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _fitnessService.AddClient(window.Client);

                    LoadData();

                    MessageBox.Show(
                        "Klijent je uspešno dodat.",
                        "Uspešno dodavanje",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Greška prilikom dodavanja",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
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

            var window = new ClientDetailsWindow(selectedClient)
            {
                Owner = this
            };

            window.ShowDialog();
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

            var window = new AddEditClientWindow(selectedClient)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _fitnessService.UpdateClient(window.Client);

                    LoadData();

                    MessageBox.Show(
                        "Podaci o klijentu su uspešno izmenjeni.",
                        "Uspešna izmena",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Greška prilikom izmene",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
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