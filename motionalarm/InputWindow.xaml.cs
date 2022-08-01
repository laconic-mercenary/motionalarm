namespace motionalarm {

    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : System.Windows.Window {

        /// <summary>
        /// Ctor
        /// </summary>
        public InputWindow(string title) {
            InitializeComponent();
            this.label1.Content = title;
            
        }

        /// <summary>
        /// Gets what was in the input text box.
        /// </summary>
        public string inputValue {
            get {
                return _input;
            }
        }

        /// <summary>
        /// Fires just before it closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            _input = new string(this.textBoxEntry.Text.ToCharArray());
        }

        private string _input = null;

        private void buttonOk_Click(object sender, System.Windows.RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, System.Windows.RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
