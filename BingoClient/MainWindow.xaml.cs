using BingoClient.Pages;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BingoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _connection;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new GameScreen());

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7117/bingo")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("TestEvent", msg =>
            {
                MessageBox.Show($"[SERVER] {msg}");
            });

            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("TestMethod", "missatge de prova");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
            }
        }
        protected override async void OnClosed(EventArgs e)
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            base.OnClosed(e);
        }
    }
}