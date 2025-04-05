using System;
using System.Windows;

namespace Chess
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Starts the game in single player mode (against computer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            ChessWindow sChessWindow = new ChessWindow(true, null);
            Hide();
            _ = sChessWindow.ShowDialog();
            Show();
        }

        /// <summary>
        /// Starts the game in two player mode (locally)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwoPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan sTimeSpan = TimeSpan.FromMinutes(5);

            ChessWindow sChessWindow = new ChessWindow(false, sTimeSpan);
            Hide();
            _ = sChessWindow.ShowDialog();
            Show();
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
