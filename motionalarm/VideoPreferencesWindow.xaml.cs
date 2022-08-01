using System.Windows;
using app.motionalarm;
using app.motionalarm.configuration;

namespace motionalarm
{
    /// <summary>
    /// Interaction logic for VideoPreferencesWindow.xaml
    /// </summary>
    public partial class VideoPreferencesWindow : Window
    {

        public VideoPreferencesWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // get the device to stream
            if (_hasDevices == false)
            {
                Config.current.videoPreferences.currentDeviceMoniker = null;
            }
            else
            {
                // first... check to see if what they selected is still valid
                string[] names = Globals.Scanning.getDeviceNames();
                bool ok = false;
                if (names != null)
                {
                    foreach (string name in names)
                    {
                        if (((string)this.comboBoxDevice.SelectedItem) == name)
                        {
                            // good to go
                            ok = true;
                            break;
                        }
                    }
                }
                //
                if (ok != true)
                {
                    Globals.MessageBox.show("A device disconnect was detected.  Failed to get device.", "Device not found", true);
                    this.DialogResult = false;
                    return;
                }
                //
                // the index for the device moniker will be the same index as
                // the corresponding name was inserted into... yeah
                string[] monikers = Globals.Scanning.getDevicesMonikers();
                Config.current.videoPreferences.currentDeviceMoniker = monikers[this.comboBoxDevice.SelectedIndex];
            }
            // get the resolution
            string res = this.comboBoxResolution.SelectedItem.ToString();
            if (res.Contains("800"))
            {
                Config.current.videoPreferences.resolution = new System.Drawing.Size(800, 600);
            }
            else if (res.Contains("640"))
            {
                Config.current.videoPreferences.resolution = new System.Drawing.Size(640, 480);
            }
            else
            {
                Config.current.videoPreferences.resolution = new System.Drawing.Size(320, 240);
            }
            // close the dialog
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // get the device list
            string[] names = Globals.Scanning.getDeviceNames();
            if (names != null)
            {
                _hasDevices = true;
                foreach (string name in names)
                {
                    this.comboBoxDevice.Items.Add(name);
                }
                // just get the first item in the list
                this.comboBoxDevice.SelectedIndex = 0;
            }

            // add the resolutions
            // this.comboBoxResolution.Items.Add("1024x768"); // (max can go is 800 x 600)
            this.comboBoxResolution.Items.Add("800x600");
            this.comboBoxResolution.Items.Add("640x480");
            this.comboBoxResolution.Items.Add("320x240");

            // select initial state
            this.comboBoxResolution.SelectedIndex = 1; // 640x480 is a good default choice
        }

        private bool _hasDevices = false;
    }
}
