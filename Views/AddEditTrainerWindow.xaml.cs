using FitnessCentar.Models;
using System;
using System.Windows;

namespace FitnessCentar.Views //Ovaj cs fajl sluzi da bi prozor koji pravimo u xaml mogao da interaguje sa programom.
{
    public partial class AddEditTrainerWindow : Window
    {
        public Trainer Trainer { get; private set; }

        private readonly bool _isEditMode;

        public AddEditTrainerWindow()
        {
            InitializeComponent();

            Trainer = new Trainer();
            _isEditMode = false;

            TitleTextBlock.Text = "Dodavanje trenera";
            Title = "Dodavanje trenera";
        }

        public AddEditTrainerWindow(Trainer trainer)
        {
            InitializeComponent();

            // Pravimo novi objekat na osnovu selektovanog trenera,
            // da se izmene ne bi direktno radile nad objektom iz tabele.
            Trainer = new Trainer
            {
                TrainerId = trainer.TrainerId,
                FirstName = trainer.FirstName,
                LastName = trainer.LastName,
                Specialization = trainer.Specialization,
                YearsOfExperience = trainer.YearsOfExperience
            };

            _isEditMode = true;

            TitleTextBlock.Text = "Izmena trenera";
            Title = "Izmena trenera";

            FillForm();
        }

        // Popunjava formu podacima kada je otvorena izmena postojećeg trenera.
        private void FillForm()
        {
            FirstNameTextBox.Text = Trainer.FirstName;
            LastNameTextBox.Text = Trainer.LastName;
            SpecializationTextBox.Text = Trainer.Specialization;
            YearsOfExperienceTextBox.Text = Trainer.YearsOfExperience.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            Trainer.FirstName = FirstNameTextBox.Text.Trim();
            Trainer.LastName = LastNameTextBox.Text.Trim();
            Trainer.Specialization = SpecializationTextBox.Text.Trim();
            Trainer.YearsOfExperience = int.Parse(YearsOfExperienceTextBox.Text.Trim());

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Osnovna validacija unosa pre slanja podataka ka glavnom prozoru.
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                ShowValidationMessage("Ime trenera je obavezno.");
                FirstNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                ShowValidationMessage("Prezime trenera je obavezno.");
                LastNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(SpecializationTextBox.Text))
            {
                ShowValidationMessage("Specijalizacija je obavezna.");
                SpecializationTextBox.Focus();
                return false;
            }

            if (!int.TryParse(YearsOfExperienceTextBox.Text.Trim(), out int yearsOfExperience))
            {
                ShowValidationMessage("Godine iskustva moraju biti ceo broj.");
                YearsOfExperienceTextBox.Focus();
                return false;
            }

            if (yearsOfExperience < 0 || yearsOfExperience > 50)
            {
                ShowValidationMessage("Godine iskustva moraju biti između 0 i 50.");
                YearsOfExperienceTextBox.Focus();
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