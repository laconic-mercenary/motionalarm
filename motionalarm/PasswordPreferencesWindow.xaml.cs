
namespace motionalarm {

    using System.Windows;
    using app.motionalarm;
    using app.motionalarm.configuration;

    /// <summary>
    /// Interaction logic for PasswordPreferencesWindow.xaml
    /// </summary>
    public partial class PasswordPreferencesWindow : Window {

        public PasswordPreferencesWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the password preference result from the user
        /// </summary>
        public PasswordPreferences passwordPreferences {
            get {
                return _passwordPreference;
            }
        }
        private PasswordPreferences _passwordPreference = new PasswordPreferences();

        private void checkBox1_Checked(object sender, RoutedEventArgs e) {
            if (checkBox1.IsChecked == true) {
                this.textBox1.IsEnabled = true;
                this.textBoxAttempts.IsEnabled = true;
                this.textBoxTime.IsEnabled = true;
            }
            else if (checkBox1.IsChecked == false) {
                this.textBox1.Clear();
                this.textBox1.IsEnabled = false;
                this.textBoxAttempts.IsEnabled = false;
                this.textBoxTime.IsEnabled = false;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (checkBox1.IsEnabled == true) {
                if (this.textBox1.Text.Length == 0) {
                    Globals.MessageBox.show("Enter a password.","Error", true);
                    return;
                }

                _passwordPreference = new PasswordPreferences();
                _passwordPreference.password = this.textBox1.Text;
                byte attempts = 0;
                byte time = 0;

                if (byte.TryParse(this.textBoxAttempts.Text, out attempts)) {
                    _passwordPreference.attemptsAllowed = attempts;
                }
                else {
                    _passwordPreference = null;
                    Globals.MessageBox.show("Please enter a valid number for attempts (greater than zero).", "Error", true);
                    return;
                }

                if (byte.TryParse(this.textBoxTime.Text, out time)) {
                    _passwordPreference.countDownSeconds = time; // convert to m.s.
                }
                else {
                    _passwordPreference = null;
                    Globals.MessageBox.show("Enter a valid number for time (seconds).","Error",true);
                    return;
                }
                this._passwordPreference.notifyImmediately = false;
                this.DialogResult = true;
            }
            else {
                this._passwordPreference = new PasswordPreferences();
                this._passwordPreference.notifyImmediately = true;
                this.DialogResult = true;
            }
        }
    }
}
