using FitnessCentar.Models;
using FitnessCentar.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Linq;

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
            TryParseWeight(WeightTextBox.Text, out double weight);

            Client.Weight = weight;
            Client.FirstName = FirstNameTextBox.Text.Trim();
            Client.LastName = LastNameTextBox.Text.Trim();
            Client.Age = int.Parse(AgeTextBox.Text.Trim());
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

        private bool TryParseWeight(string input, out double weight) //Ovaj dodatak se bavi Quality of Life-om aplikacije
        {
            // Prihvatamo i zarez i tačku kao decimalni separator.
            string normalizedInput = input.Trim().Replace(',', '.');

            return double.TryParse(
                normalizedInput,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out weight
            );
        }

        // Osnovne provere unosa pre slanja podataka ka glavnom prozoru.
        private bool ValidateForm()
        {
            if (FirstNameTextBox.Text.Trim().Length < 2) //Korekcija da nemamo besmislena imena (izvinjavam se strancima)
            {
                ShowValidationMessage("Ime mora imati najmanje 2 karaktera.");
                FirstNameTextBox.Focus();
                return false;
            }

            if (LastNameTextBox.Text.Trim().Length < 2) //Ista prica
            {
                ShowValidationMessage("Prezime mora imati najmanje 2 karaktera.");
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

            if (!TryParseWeight(WeightTextBox.Text, out double weight))
            {
                ShowValidationMessage("Težina mora biti broj. Možete koristiti tačku ili zarez, npr. 82.5 ili 82,5.");
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

            //v za proveru telefona

            if (string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text))
            {
                ShowValidationMessage("Broj telefona je obavezan.");
                PhoneNumberTextBox.Focus();
                return false;
            }

            string phoneNumber = PhoneNumberTextBox.Text.Trim();

            bool hasInvalidPhoneCharacters = phoneNumber.Any(character =>
                !char.IsDigit(character) &&
                character != '+' &&
                character != '-' &&
                character != '/' &&
                character != ' '
            );

            if (hasInvalidPhoneCharacters)
            {
                ShowValidationMessage("Telefon može sadržati samo cifre, razmak i znakove +, - ili /.");
                PhoneNumberTextBox.Focus();
                return false;
            }

            int digitCount = phoneNumber.Count(char.IsDigit);

            if (digitCount < 6)
            {
                ShowValidationMessage("Telefon mora sadržati najmanje 6 cifara.");
                PhoneNumberTextBox.Focus();
                return false;
            }

            //^ za proveru telefona

            if (MembershipStartDatePicker.SelectedDate == null)
            {
                ShowValidationMessage("Datum početka članstva je obavezan.");
                MembershipStartDatePicker.Focus();
                return false;
            }

            if (MembershipStartDatePicker.SelectedDate.Value.Date > DateTime.Now.Date) //provera da se ne unese nevalidan datum tj. u buducnosti
            {
                ShowValidationMessage("Datum početka članstva ne može biti u budućnosti.");
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