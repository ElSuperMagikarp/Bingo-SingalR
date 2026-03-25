using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BingoClient.Pages
{
    /// <summary>
    /// Interaction logic for GameScreen.xaml
    /// </summary>
    public partial class GameScreen : Page
    {
        public GameScreen()
        {
            InitializeComponent();
        }

        private void fiveNumbersButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("fiveNumbersButton_Click");
        }

        private void bingoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("bingoButton_Click");
        }

        private void numberClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("numberClick");
        }
    }
}