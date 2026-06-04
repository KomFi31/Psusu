using FitnessCentar.Models;
using FitnessCentar.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace FitnessCentar.Views //Svrha ovog fajla je da bi omogucio interakciju klijentskog prozora i programa / baze.
{
    public partial class AddEditClientWindow : Window
    {
        private readonly FitnessService _fitnessService;

        public Client Client { get; private set; }

        public AddEditClientWindow()
        {
            InitializeComponent();

            _fitnessService = new FitnessService();
            Client = new Client();

            TitleTextBlock.Text = "Dodavanje klijenta";
            Title = "Dodavanje klijenta";

            LoadTrainers();
            MembershipStartDatePicker.SelectedDate = DateTime.Now;
        }

        public AddEditClientWindow(Client client)
        {
            InitializeComponent();

            _fitnessService = new FitnessService();

            // Kreira se kopija selektovanog klijenta da se izmene potvrde tek klikom na Sačuvaj.
            Client = new Client
            {
                ClientId = client.ClientId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Age = client.Age,
                Weight = client.Weight,
                Goal = client.Goal,
                PhoneNumber = client.PhoneNumber,
                MembershipStartDate = client.MembershipStartDate,
                TrainerId = client.TrainerId
            };

            TitleTextBlock.Text = "Izmena klijenta";
            Title = "Izmena klijenta";

            LoadTrainers();
            FillForm();
        }

        // Učitava trenere u ComboBox kako bi se klijent povezao sa postojećim trenerom.
        private void LoadTrainers()
        {
            List<Trainer> trainers = _fitnessService.GetAllTrainers();

            TrainerComboBox.ItemsSource = trainers;
            TrainerComboBox.SelectedValuePath = "TrainerId";

            if (trainers.Count > 0 && TrainerComboBox.SelectedItem == null)
            {
                TrainerComboBox.SelectedIndex = 0;
            }
        }

        // Popunjava formu kada se menja postojeći klijent.
        private void FillForm()
        {
            FirstNameTextBox.Text = Client.FirstName;
            LastNameTextBox.Text = Client.LastName;
            AgeTextBox.Text = Client.Age.ToString();
            WeightTextBox.Text = Client.Weight.ToString(CultureInfo.InvariantCulture);
            GoalTextBox.Text = Client.Goal;
            PhoneNumberTextBox.Text = Client.PhoneNumber;
            MembershipStartDatePicker.SelectedDate = Client.MembershipStartDate;
            TrainerComboBox.SelectedValue = Client.TrainerId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            Client.FirstName = FirstNameTextBox.Text.Trim();
            Client.LastName = LastNameTextBox.Text.Trim();
            Client.Age = int.Parse(AgeTextBox.Text.Trim());
            Client.Weight = double.Parse(WeightTextBox.Text.Trim(), CultureInfo.InvariantCulture);
            Client.Goal = GoalTextBox.Text.Trim();
            Client.PhoneNumber = PhoneNumberTextBox.Text.Trim();
            Client.MembershipStartDate = MembershipStartDatePicker.SelectedDate!.Value;
            Client.TrainerId = (int)TrainerComboBox.SelectedValue;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Osnovne provere unosa pre slanja podataka ka glavnom prozoru.
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                ShowValidationMessage("Ime klijenta je obavezno.");
                FirstNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                ShowValidationMessage("Prezime klijenta je obavezno.");
                LastNameTextBox.Focus();
                return false;
            }

            if (!int.TryParse(AgeTextBox.Text.Trim(), out int age))
            {
                ShowValidationMessage("Godine moraju biti ceo broj.");
                AgeTextBox.Focus();
                return false;
            }

            if (age < 14 || age > 100)
            {
                ShowValidationMessage("Godine moraju biti između 14 i 100.");
                AgeTextBox.Focus();
                return false;
            }

            if (!double.TryParse(WeightTextBox.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double weight))
            {
                ShowValidationMessage("Težina mora biti broj. Koristite tačku za decimalni zapis, npr. 82.5.");
                WeightTextBox.Focus();
                return false;
            }

            if (weight < 30 || weight > 250)
            {
                ShowValidationMessage("Težina mora biti između 30 i 250 kg.");
                WeightTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(GoalTextBox.Text))
            {
                ShowValidationMessage("Cilj treninga je obavezan.");
                GoalTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text))
            {
                ShowValidationMessage("Broj telefona je obavezan.");
                PhoneNumberTextBox.Focus();
                return false;
            }

            if (MembershipStartDatePicker.SelectedDate == null)
            {
                ShowValidationMessage("Datum početka članstva je obavezan.");
                MembershipStartDatePicker.Focus();
                return false;
            }

            if (TrainerComboBox.SelectedValue == null)
            {
                ShowValidationMessage("Morate izabrati trenera.");
                TrainerComboBox.Focus();
                return false;
            }

            return true;
        }

        private void ShowValidationMessage(string message)
        {
            MessageBox.Show(
                message,
                "Nevalidan unos",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }
}