

namespace motionalarm
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Windows;
    using System.Windows.Documents;
    using app.motionalarm;
    using app.motionalarm.streaming;
    using app.motionalarm.configuration;

    /// <summary>
    /// Interaction logic for StreamPreferencesWindow.xaml
    /// </summary>
    public partial class StreamPreferencesWindow : Window
    {

        public StreamPreferencesWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the preferences specified in the dialog.
        /// </summary>
        public StreamPreferences preferences
        {
            get
            {
                return _preferences;
            }
        }

        private app.motionalarm.streaming.StreamPreferences _preferences = null;

        bool checkValues()
        {
            ushort port = 0;
            if (!ushort.TryParse(this.textBoxPortNo.Text, out port))
            {
                Globals.MessageBox.show("Port number must be (0 - 65535)", "Ports.", true);
                return false;
            }            
            _preferences.localConnectionPort = (int)port;
            _preferences.password = this.textBoxPassword.Text;            
            return true;
        }
        
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (checkValues())
            {                
                this.DialogResult = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _preferences = Config.current.streamPreferences;
            this.textBoxPortNo.Text = _preferences.localConnectionPort.ToString();
        }
    }
}
