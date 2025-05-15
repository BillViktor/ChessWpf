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
            Hide();
            ShowSettings(true);
            Show();
        }

        /// <summary>
        /// Starts the game in two player mode (locally)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwoPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            ShowSettings(false);
            Show();
        }

        private void ShowSettings(bool aSinglePlayer)
        {
            SettingWindow sSettingsWindow = new SettingWindow(aSinglePlayer);
            _ = sSettingsWindow.ShowDialog();
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
