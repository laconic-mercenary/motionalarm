
namespace motionalarm
{
    using System.Windows;
    using app.motionalarm;
    using System.Speech.Synthesis;
    using app.motionalarm.configuration;

    /// <summary>
    /// Interaction logic for VideoDetectionPreferencesWindow.xaml
    /// </summary>
    public partial class DetectionPreferencesWindow : Window
    {

        /// <summary>
        /// Creates the detection preferences window.
        /// </summary>
        public DetectionPreferencesWindow()
        {
            InitializeComponent();
            setDefaults();
        }

        /// <summary>
        /// Fills the controls with default values.
        /// </summary>
        private void setDefaults()
        {
            if (Config.current.scanningPreferences != null)
            {
                this.textBoxBmpTolerance.Text = Config.current.scanningPreferences.bitmapDifferenceTolerancePercentage.ToString();
                this.textBoxConfirmationWaitLength.Text = (Config.current.scanningPreferences.intrusionConfirmationWaitTime / 1000).ToString();
                this.textBoxStartTime.Text = (Config.current.scanningPreferences.startScanningWaitTimeSeconds / 1000).ToString(); // seconds            
                this.textBoxMessage.Text = Config.current.scanningPreferences.message;
                this.textBoxIntruderName.Text = Config.current.scanningPreferences.intrudersName;
                this.prefs = Config.current.scanningPreferences;
            }
        }

        /// <summary>
        /// Fires when the ok button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (checkOk())
            {
                this.DialogResult = true;
            }
        }

        /// <summary>
        /// Verifies all entries.
        /// </summary>
        /// <returns></returns>
        bool checkOk()
        {
            //
            // check the tolerance specified
            float tolerance = 0.0f;
            if (float.TryParse(this.textBoxBmpTolerance.Text, out tolerance))
            {
                //
                // check, this needs to be unsigned
                if (tolerance <= 0.0f || tolerance > 1.0f)
                {
                    Globals.MessageBox.show(
                        "Tolerance value must be between 0.0 (0%) and 1.0 (100%)",
                        "Invalid Tolerance", true
                    );
                    return false;
                }

            }
            else
            {
                // nope
                Globals.MessageBox.show(
                    "Please enter a value between 0.0 (0%) and 1.0 (100%)",
                    "Invalid Tolerance",
                    true
                );
                return false;
            }

            //
            // convert the waiting time... which can be 0 if need be.
            uint waitTime = 0;
            if (uint.TryParse(this.textBoxStartTime.Text, out waitTime))
            {
                //
                if (waitTime >= 150)
                {
                    // confirm with them that they want to wait that long
                    if (Globals.MessageBox.showYesNo("That's a long time to wait.  Are you sure?", "Wait Time") == false)
                    {
                        return false;
                    }
                }
            }
            else
            {
                Globals.MessageBox.show("Please enter a value in seconds (ex: 10)", "Invalid Wait time specified", true);
                return false;
            }

            //
            // check the detec
            double detectionWaitTime = 0.0;
            if (double.TryParse(this.textBoxConfirmationWaitLength.Text, out detectionWaitTime))
            {
                // check to make sure it's unsigned
                if (detectionWaitTime <= 0)
                {
                    Globals.MessageBox.show("The detection confirmed time cannot be negative!", "Invalid Detection Time specified", true);
                    return false;
                }
            }
            else
            {
                Globals.MessageBox.show("Please enter a numeric value (ex: 4)", "Invalid Detection Time specified", true);
                return false;
            }

            //
            // looks good, pass over the values
            this.prefs.bitmapDifferenceTolerancePercentage = tolerance;
            // this.bitmapTolerance = tolerance;
            // this.startTimeWait = (int)waitTime * 1000; // convert to milliseconds
            this.prefs.startScanningWaitTimeSeconds = (int)waitTime * 1000;
            this.prefs.intrusionConfirmationWaitTime = (int)(detectionWaitTime * 1000);
            // this.intrusionDetectionWaitTime = detectionWaitTime * 1000; // to milliseconds
            // this.intruderName = this.textBoxIntruderName.Text;
            this.prefs.intrudersName = this.textBoxIntruderName.Text;
            this.prefs.message = this.textBoxMessage.Text;
            // this.message = this.textBoxMessage.Text;
            // return true of course
            return true;
        }

        public delegate void SpeechHandlerMethod(string speech, bool isTest = false);

        public app.motionalarm.configuration.ScanningPreferences prefs
        { 
            get; 
            set; 
        }

        public SpeechHandlerMethod speechMethod
        {
            get;
            set;
        }

        public float bitmapTolerance
        {
            get;
            set;
        }

        public int startTimeWait
        {
            get;
            set;
        }

        public double intrusionDetectionWaitTime
        {
            get;
            set;
        }

        public string intruderName
        {
            get;
            set;
        }

        public string message
        {
            get;
            set;
        }

        void buttonListen_Click(object sender, RoutedEventArgs e)
        {
            if (this.speechMethod != null)
            {
                // build the text
                string text = this.textBoxIntruderName.Text + ".  " + this.textBoxMessage.Text;
                // call the speech method from the main thread
                this.speechMethod(text, true);
            }
            else
            {
                Globals.MessageBox.show(
                    "There are no voices installed.  Please select the Speech button" + System.Environment.NewLine +
                    "from the main menu to view a list of installed voices.",
                    "Speech Error", true);
            }
        }
    }
}
