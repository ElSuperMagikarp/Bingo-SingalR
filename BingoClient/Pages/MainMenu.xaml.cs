using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Windows.Controls;

namespace BingoClient.Pages
{
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
            SetupSignalR();
        }

        private void SetupSignalR()
        {
            var conn = MainWindow.Connection;

            conn.On<string, bool>("RoomJoined", (roomId, isHost) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    // Register player immediately after joining
                    await MainWindow.Connection.InvokeAsync("RegisterPlayer");

                    // Then navigate to the game screen
                    NavigationService.Navigate(new GameScreen(roomId, isHost));
                });
            });

            conn.On<string>("Message", msg =>
            {
                Dispatcher.Invoke(() => MessageBox.Show(msg));
            });
        }

        private async void CreateRoom_Click(object sender, RoutedEventArgs e)
        {
            string roomId = RoomInput.Text;

            if (string.IsNullOrWhiteSpace(roomId))
            {
                MessageBox.Show("Enter room id");
                return;
            }

            await MainWindow.Connection.InvokeAsync("CreateRoom", roomId);
        }

        private async void JoinRoom_Click(object sender, RoutedEventArgs e)
        {
            string roomId = RoomInput.Text;

            if (string.IsNullOrWhiteSpace(roomId))
            {
                MessageBox.Show("Enter room id");
                return;
            }

            await MainWindow.Connection.InvokeAsync("JoinRoom", roomId);
        }
    }
}