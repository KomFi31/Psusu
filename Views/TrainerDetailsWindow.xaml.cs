using FitnessCentar.Models;
using System.Windows;

namespace FitnessCentar.Views
{
    public partial class TrainerDetailsWindow : Window //Prikaz informacija o treneru.
    {
        public TrainerDetailsWindow(Trainer trainer)
        {
            InitializeComponent();

            // Prozor dobija izabranog trenera i samo prikazuje njegove podatke.
            TrainerIdTextBlock.Text = trainer.TrainerId.ToString();
            FirstNameTextBlock.Text = trainer.FirstName;
            LastNameTextBlock.Text = trainer.LastName;
            SpecializationTextBlock.Text = trainer.Specialization;
            YearsOfExperienceTextBlock.Text = trainer.YearsOfExperience.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}