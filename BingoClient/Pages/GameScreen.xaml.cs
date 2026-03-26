using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BingoClient.Pages
{
    public partial class GameScreen : Page
    {
        private string _roomId;
        private bool _isHost;

        private List<int> myNumbers = new();

        public GameScreen(string roomId, bool isHost)
        {
            InitializeComponent();

            _roomId = roomId;
            _isHost = isHost;

            RoomText.Text = roomId;

            if (!_isHost)
                StartButton.Visibility = Visibility.Collapsed;

            SetupSignalR();
        }

        private async void GameScreen_Loaded(object sender, RoutedEventArgs e)
        {
            await MainWindow.Connection.InvokeAsync("RegisterPlayer");
        }

        private void SetupSignalR()
        {
            var conn = MainWindow.Connection;

            conn.On("GameStarted", () =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Game Started");
                });
            });

            conn.On<List<int>>("ReceiveNumbers", nums =>
            {
                Dispatcher.Invoke(() =>
                {
                    myNumbers = nums;
                    BingoCardGrid.Children.Clear();

                    foreach (var num in myNumbers)
                    {
                        var btn = new Button
                        {
                            Content = num.ToString(),
                            Margin = new Thickness(5),
                            Width = 60,
                            Height = 60
                        };
                        btn.Click += BingoNumberButton_Click;
                        BingoCardGrid.Children.Add(btn);
                    }
                });
            });

            conn.On<int>("NumberCalled", num =>
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentNumber.Text = num.ToString();
                });
            });

            conn.On("SuccessfulFiveNumbers", () =>
            {
                Dispatcher.Invoke(() => MessageBox.Show("5 Numbers Winner"));
            });

            conn.On("SuccessfulBingo", () =>
            {
                Dispatcher.Invoke(() => MessageBox.Show("Bingo Winner"));
            });

            conn.On<string>("Message", msg =>
            {
                Dispatcher.Invoke(() => MessageBox.Show(msg));
            });
        }

        private async void StartGame_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Connection.InvokeAsync("StartGame");
        }

        private async void fiveNumbersButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Connection.InvokeAsync("CallFiveNumbers");
        }

        private async void bingoButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.Connection.InvokeAsync("CallBingo");
        }

        private void BingoNumberButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            if (btn.Background == System.Windows.Media.Brushes.LightGreen)
                btn.ClearValue(Button.BackgroundProperty);
            else
                btn.Background = System.Windows.Media.Brushes.LightGreen;
        }
    }
}