using Chess.Models;
using System.Windows;

namespace Chess
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private TimerSetting mTimerSetting = new TimerSetting();
        private bool mSinglePlayer;

        public SettingWindow(bool aSinglePlayer)
        {
            InitializeComponent();

            // You're only able to choose a starting color in SinglePlayer
            if (!aSinglePlayer)
            {
                SinglePlayerColorGrid.Visibility = Visibility.Hidden;
                SinglePlayerMinimaxDepthGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                WhiteButton.IsChecked = true;
            }

            mSinglePlayer = aSinglePlayer;

            InitializeGui();
        }

        /// <summary>
        /// Initializes the GUI elements
        /// </summary>
        private void InitializeGui()
        {
            if(mSinglePlayer)
            {
                SinglePlayerMinimaxDepth.Text = "4";
            }

            WhiteTimerCheckBox.IsChecked = true;
            WhiteTimer.Text = "3";
            WhiteTimerIncrement.Text = "2";

            BlackTimerCheckBox.IsChecked = true;
            BlackTimer.Text = "3";
            BlackTimerIncrement.Text = "2";
        }

        /// <summary>
        /// Closes the settings window without starting the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Event Handlers
        /// <summary>
        /// Starts the game with the selected settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SetTimerSettings()) return;
            if (mSinglePlayer && !SetMinimaxDepth()) return;
            ColorEnum sSinglePlayerColor = GetSelectedColor();
            ChessWindow sChessWindow = new ChessWindow(mSinglePlayer, mTimerSetting, sSinglePlayerColor, int.Parse(SinglePlayerMinimaxDepth.Text));
            _ = sChessWindow.ShowDialog();
            Close();
        }

        /// <summary>
        /// Toggles the visibility of the white timer settings
        /// </summary>
        private void WhiteTimerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (WhiteTimerCheckBox.IsChecked == true)
            {
                WhiteTimerSettings.Visibility = Visibility.Visible;
            }
            else
            {
                WhiteTimerSettings.Visibility = Visibility.Hidden;
            }
        }        

        /// <summary>
        /// Toggles the visibility of the black timer settings
        /// </summary>
        private void BlackTimerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (BlackTimerCheckBox.IsChecked == true)
            {
                BlackTimerSettings.Visibility = Visibility.Visible;
            }
            else
            {
                BlackTimerSettings.Visibility = Visibility.Hidden;
            }
        }
        #endregion


        #region Helpers
        /// <summary>
        /// Sets the timer settings
        /// </summary>
        private bool SetTimerSettings()
        {
            //Validate inputs
            int sWhiteTimer = 0;
            int sBlackTimer = 0;
            int sWhiteIncrement = 0;
            int sBlackIncrement = 0;

            if (WhiteTimerCheckBox.IsChecked == true)
            {
                if (!int.TryParse(WhiteTimer.Text, out sWhiteTimer) || sWhiteTimer < 1 || sWhiteTimer > 60)
                {
                    MessageBox.Show("Invalid white timer value! Must be between 1 and 60 minutes.");
                    return false;
                }
                if (!int.TryParse(WhiteTimerIncrement.Text, out sWhiteIncrement) || sWhiteIncrement < 0 || sWhiteIncrement > 30)
                {
                    MessageBox.Show("Invalid white increment value! Must be between 0 and 30 seconds.");
                    return false;
                }
            }
            if (BlackTimerCheckBox.IsChecked == true)
            {
                if (!int.TryParse(BlackTimer.Text, out sBlackTimer) || sBlackTimer < 1 || sBlackTimer > 60)
                {
                    MessageBox.Show("Invalid black timer value! Must be between 1 and 60 minutes.");
                    return false;
                }
                if (!int.TryParse(BlackTimerIncrement.Text, out sBlackIncrement) || sBlackIncrement < 0 || sBlackIncrement > 30)
                {
                    MessageBox.Show("Invalid black increment value! Must be between 0 and 30 seconds.");
                    return false;
                }
            }

            //Set timer settings
            mTimerSetting = new TimerSetting
            {
                WhiteEnabled = WhiteTimerCheckBox.IsChecked == true,
                WhiteTimer = sWhiteTimer * 60, // Convert to seconds
                WhiteIncrement = sWhiteIncrement,
                BlackEnabled = BlackTimerCheckBox.IsChecked == true,
                BlackTimer = sBlackTimer * 60, // Convert to seconds
                BlackIncrement = sBlackIncrement
            };

            return true;
        }

        /// <summary>
        /// Validates the minimax depth input
        /// </summary>
        /// <returns></returns>
        private bool SetMinimaxDepth()
        {
            if(!int.TryParse(SinglePlayerMinimaxDepth.Text, out int sFoo) || sFoo > 4 || sFoo < 1)
            {
                MessageBox.Show("Invalid minimax depth value! Must be between 1-4.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the color to play as (SinglePlayer)
        /// </summary>
        private ColorEnum GetSelectedColor()
        {
            if (WhiteButton.IsChecked == true)
            {
                return ColorEnum.White;
            }
            else
            {
                return ColorEnum.Black;
            }
        }
        #endregion
    }
}
