using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;

namespace BingoClient
{
    public partial class MainWindow : Window
    {
        public static HubConnection Connection;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7117/bingo")
                .WithAutomaticReconnect()
                .Build();

            try
            {
                await Connection.StartAsync();
                MainFrame.Navigate(new Pages.MainMenu());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (Connection != null)
                await Connection.DisposeAsync();

            base.OnClosed(e);
        }
    }
}