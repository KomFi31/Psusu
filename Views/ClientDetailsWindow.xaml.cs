using FitnessCentar.Models;
using System.Windows;

namespace FitnessCentar.Views
{
    public partial class ClientDetailsWindow : Window
    {
        public ClientDetailsWindow(Client client)
        {
            InitializeComponent();

            // Prozor prikazuje podatke o klijentu i njegovom povezanom treneru.
            ClientIdTextBlock.Text = client.ClientId.ToString();
            FirstNameTextBlock.Text = client.FirstName;
            LastNameTextBlock.Text = client.LastName;
            AgeTextBlock.Text = client.Age.ToString();
            WeightTextBlock.Text = $"{client.Weight} kg";
            GoalTextBlock.Text = client.Goal;
            PhoneNumberTextBlock.Text = client.PhoneNumber;
            MembershipStartDateTextBlock.Text = client.MembershipStartDate.ToString("dd.MM.yyyy");

            TrainerTextBlock.Text = client.Trainer != null
                ? client.Trainer.ToString()
                : "Trener nije učitan";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}