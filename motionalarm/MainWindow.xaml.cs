namespace motionalarm
{
    using System;
    using System.Speech.Synthesis;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using app.motionalarm;
    using app.motionalarm.configuration;
    using app.motionalarm.imaging;
    using app.motionalarm.logging;
    using app.motionalarm.notification;
    using app.motionalarm.reporting;
    using app.motionalarm.scanning;
    using app.motionalarm.streaming;

    /// <summary>
    /// Represents the top level ui window.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// MainWindow constructor.
        /// </summary>
        public MainWindow()
        {
            // build the interface
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the Register button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonLicense_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Fires when the operating manual button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonDocOperatingManual_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Fires when the quick start button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonQuickStart_Click(object sender, RoutedEventArgs e)
        {
        }

        //
        // Window UI Events
        #region ((Window UI Events))

        /// <summary>
        /// Fires when the Company License button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonCompanyLicense_Click(object sender, RoutedEventArgs e)
        {
            LicenseWindow window = new LicenseWindow();
            window.ShowDialog();
        }

        /// <summary>
        /// Fires when we want to change the video preferences (Video Settings button).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigVideo_Click(object sender, RoutedEventArgs e)
        {
            VideoPreferencesWindow window = new VideoPreferencesWindow();
            if (window.ShowDialog() == true)
            {
                scannerInitialize();
            }
        }

        /// <summary>
        /// Just opens up the folder where the images and reports are stored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ribbonDropButtonReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // just try to open it up... if it doesn't exist it will bomb
                System.Diagnostics.Process.Start(Globals.Storage.appSpecificDataFolder);
            }
            catch
            {
                Globals.MessageBox.show("Currently no directory exists with reports.", "No Reports Available", true);
            }
        }

        /// <summary>
        /// Shows the dialog with system information on it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonSystemInfo_Click(object sender, RoutedEventArgs e)
        {
            SystemInformationWindow window = new SystemInformationWindow();
            window.ShowDialog();
        }

        /// <summary>
        /// Enables or disables the streamer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkBoxStream_Click(object sender, RoutedEventArgs e)
        {
            //
            // check what exactly was done
            if (this.checkBoxStream.IsChecked == true)
            {
                // disable scanning if running
                if (this.scanner != null)
                {
                    this.scanner.isDetectionEnabled = false;
                }
                // start streaming or enable if not already streaming
                if (this.streamer != null)
                {
                    if (this.streamer.sendingPaused == true)
                    {
                        this.streamer.sendingPaused = false;
                    }
                }
            }
            else
            { // un checked the check box
                // disable the streamer
                if (this.streamer != null)
                {
                    this.streamer.sendingPaused = true;
                }
                // enable scanning again
                if (this.scanner != null)
                {
                    this.scanner.isDetectionEnabled = true;
                }
            }
        }

        /// <summary>
        /// Creates a new streamer based on the specified preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigStream_Click(object sender, RoutedEventArgs e)
        {
            StreamPreferencesWindow window = new StreamPreferencesWindow();
            if (window.ShowDialog() == true)
            {
                if (this.streamer == null)
                {
                    try
                    {
                        this.streamer = new StreamManager(window.preferences.localConnectionPort);
                        this.streamer.onStreamCountChanged += new Action(streamer_onStreamCountChanged);
                        updateStreamStatus("ACTIVE", Brushes.Green);
                    }
                    catch(Exception ex)
                    {
                        if (this.streamer != null)
                        {
                            this.streamer.close();
                        }
                        Logger.log("Exception: (streaming config) " + ex.Message);
                        Globals.MessageBox.show("Could not start streaming service.  Check network connection.", "Streaming Error", true);                        
                        return;
                    }
                }
                this.streamer.setPassword(window.preferences.password);
                // update the label
                this.labelStreamSettings.Foreground = updatedBrush;
                this.labelStreamSettings.Content = "Stream ready.";

                if (this.checkBoxStream.IsChecked == true)
                {
                    startStreaming();
                }
                else
                {
                    updateStatusLabel("Click 'Stream'.");
                }
            }
        }

        /// <summary>
        /// Starts the scanner and begins updating the video stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonScanStart_Click(object sender, RoutedEventArgs e)
        {
            // first check if a camera is attached
            if (Globals.Scanning.areDevicesConnected() == false)
            {
                // notify
                Globals.MessageBox.show("ERROR: connect a camera device to the machine.",
                    "Camera Error - No Device", true);
                return;
            }
            else
            {
                // notify
                speakStarting(this.scanningPreferences.startScanningWaitTimeSeconds);
                // provide the time necessary to leave the vicinity
                System.Threading.Thread.Sleep(this.scanningPreferences.startScanningWaitTimeSeconds);
                // speak notification
                speak("Starting");
                // enable/disable based on the state of our streaming check box
                this.scanner.isDetectionEnabled = (this.checkBoxStream.IsChecked == false);
                // start the scanner
                this.scanner.start();
                // to the log
                Logger.log("scanning started");
            }
        } //

        /// <summary>
        /// Stops the scanner.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonScanStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.scanner != null)
            {
                this.scanner.stop();
            }
        } //

        /// <summary>
        /// Resets the scanner.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            // reset the scanner
            if (this.scanner != null)
            {
                this.scanner.reset();
            }
            // nullify the image
            this.imageVideo.Source = null;
        } //

        /// <summary>
        /// Configures new detection parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigDetection_Click(object sender, RoutedEventArgs e)
        {
            // instantiate the window
            DetectionPreferencesWindow window = new DetectionPreferencesWindow();
            // pass over our speech handling method
            window.speechMethod = new DetectionPreferencesWindow.SpeechHandlerMethod(this.speak);
            // show the dialog
            if (window.ShowDialog() == true)
            {
                this.scanningPreferences = window.prefs; // pass over the detection preferences
                Config.current.scanningPreferences = this.scanningPreferences;

                if (!this.scanningPreferences.save())
                {
                    Logger.log("Scanning Preferences failed to save...");
                }
                this.labelDetectionSettings.Foreground = updatedBrush;  // indicate an update
                this.labelDetectionSettings.Content = "Detection settings updated.";
                // stop the scanner and re-initialize
                scannerInitialize();
            }
        } //

        /// <summary>
        /// Sets the voice of the synthesizer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigSpeech_Click(object sender, RoutedEventArgs e)
        {
            SpeechPreferencesWindow window = new SpeechPreferencesWindow(this.speechSynthesizer);
            if (window.ShowDialog() == true)
            {
                this.speechEnabled = window.enableSpeech;
                // select the voice via the synthesizer
                this.speechSynthesizer.SelectVoice(window.voiceInfo);
                // notify of update
                this.labelNoficationSettings.Foreground = updatedBrush;
                if (window.enableSpeech == true)
                {
                    this.labelNoficationSettings.Content = "Voice Updated: " + window.voiceInfo;
                }
                else
                {
                    this.labelNoficationSettings.Content = "(Speech Diabled)";
                }
            }
        } //

        /// <summary>
        /// Sets the new password preferences
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigPassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordPreferencesWindow window = new PasswordPreferencesWindow();
            if (window.ShowDialog() == true)
            {
                // set the preferences
                this.passwordPreferences = window.passwordPreferences;
                // notify of update                
                if (this.passwordPreferences.password.Length != 0)
                {
                    // officially update
                    this.labelPasswordSettings.Foreground = updatedBrush;
                    this.labelPasswordSettings.Content = "Password set.";
                }
            }
        } //

        /// <summary>
        /// Gets the email preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigNotifications_Click(object sender, RoutedEventArgs e)
        {
            EmailPreferencesWindow window = new EmailPreferencesWindow();
            if (window.ShowDialog() == true)
            {
                // pass over the preferences
                this.emailPreferences = window.emailPreferences;
                Config.current.emailPreferences = this.emailPreferences;

                if (this.emailPreferences.save())
                {
                    Logger.log("Notification preferences failed to save.");
                }
                if (this.emailPreferences.enableEmail == true)
                {
                    this.labelNoficationSettings.Foreground = updatedBrush;
                    this.labelNoficationSettings.Content = "Notifications (email) set.";
                }
            }
        } //

        /// <summary>
        /// Opens up the opened log file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonConfigLogging_Click(object sender, RoutedEventArgs e)
        {
            // open the log file
            Logger.isEnabled = (bool)this.buttonConfigLogging.IsChecked;
            updateStatusLabel(Logger.isEnabled ? "Logging enabled." : "Logging disabled.");
        } //

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        } //

        /// <summary>
        /// Opens a dialog showing development information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            Globals.MessageBox.show(
                "(C) Frontier Outpost, LLC" + System.Environment.NewLine +
                "Developed by MLCS" + System.Environment.NewLine +
                Globals.Application.applicationName + " v" + Globals.Application.applicationVersion,
                "About " + Globals.Application.applicationName
            );
        } //

        /// <summary>
        /// This is fired when the mouse enters the area of the reports button in the ribbon drop down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ribbonDropButtonReports_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            showCurrentReports();
        }

        /// <summary>
        /// Fires just before the window closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // the intruder may be trying to close down the application...
            // so let's send a notification... mwa ha ha
            // we can take the chance
            if (this.scanner != null)
            {
                if (this.scanner.detectionState == DetectionState.intrusionConfirmed)
                {
                    // send it out
                    string imagePath = saveImage(this.scanner.currentImage);
                    sendEmail(imagePath);
                }
                //
                if (this.scanner.scannerState == ScannerState.scanning)
                {
                    // shut down the scanner if we are running
                    this.scanner.stop();
                }
                // destroy the scanner
                this.scanner.destroy();
                this.scanner = null;
            }
            // check if streaming...
            if (this.streamer != null)
            {
                this.streamer.flush();
                this.streamer.close();
            }
            // log shut down
            Logger.log("app closed");
        } //


        /// <summary>
        /// Fires when the MainWindow first opens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // initial control states
            this.Title = string.Format("{0} v{2} - {1}",
                Globals.Application.applicationName,
                Globals.Application.companyName,
                Globals.Application.applicationVersion
            );

            //
            // create directories
            if (Globals.Storage.createDirectories() == false)
            {
                Globals.MessageBox.show(
                    "Unable to create the required folders for storing images and reports." + System.Environment.NewLine +
                    "Try moving the application to a location that does not require administrative roles to run.",
                    "Directory Creation Error", true);
                return;
            }

            // read from the database to populate the configuration values
            readConfiguration();
            //
            // scanner setup
            scannerInitialize();
            //
            // emailer setup
            this.emailer = new Emailer();
            this.emailer.setPreferences(this.emailPreferences);
            this.emailer.isEnabled = false; // turn it off for now

            // speech setup
            this.speechSynthesizer = new SpeechSynthesizer();

            //
            // enable reporting
            Reporting.isEnabled = true;

            // notify of streamer (no activity)
            updateStreamStatus("OFFLINE", Brushes.Gray);

            // blank out our led indicator
            this.binaryLEDThreshold.setLensColorPulse(library.LedIndicator.LensColor.BLANK);
            
            // show the recent reports in the drop down ribbon button pane
            showCurrentReports();

            // setup logging
            Logger.setDir(Globals.Storage.appSpecificDataFolder);
            Logger.log("app starting.");
        } //

        #endregion

        //
        // Scanner Events
        #region ((Scanner Events))

        /// <summary>
        /// Fires when the threshold is broken.
        /// </summary>
        /// <param name="e"></param>
        void scanner_onThresholdBroken(ThresholdBrokenEventArgs e)
        {
            // this won't continuosly be called
            // just each time the threshold is broken
            ++this.thresholdBrokenOccurences;
            this.labelNumToleranceExceeded.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                this.labelNumToleranceExceeded.Content = this.thresholdBrokenOccurences;
            }));
        } //

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="e"></param>
        void scanner_onThresholdChanged(ThresholdBrokenEventArgs e)
        {
            // this is continually updated
            this.progressBarThreshold.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                // update the progress bar
                this.progressBarThreshold.Maximum = e.maxThreshold;
                this.progressBarThreshold.Value = e.currentThreshold;
                //
                // indicate with LED if we are currently past threshold
                // to make it flash only once per treshold break, we 
                // use 2 booleans (ledNormalLock, ledBrokenLock) to enforce this
                if (e.maxThreshold <= e.currentThreshold)
                {
                    ledNormalLock = false;
                    if (ledBrokenLock == false)
                    {
                        this.binaryLEDThreshold.setLensColorPulse(library.LedIndicator.LensColor.ORANGE);
                        ledBrokenLock = true;
                    }
                }
                else
                {
                    ledBrokenLock = false;
                    if (ledNormalLock == false)
                    {
                        this.binaryLEDThreshold.setLensColor(library.LedIndicator.LensColor.BLANK);
                        //this.binaryLEDThreshold.setLensColorPulse(library.LedIndicator.LensColor.BLANK);
                        ledNormalLock = true;
                    }
                }
            }));
        } //

        /// <summary>
        /// Fires when a state change is detected in the scanner
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="oldState"></param>
        void scanner_onScannerStateChanged(ScannerState newState, ScannerState oldState)
        {
            if (this.CheckAccess())
            {
                changeScannerControls(newState);
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    changeScannerControls(newState);
                }));
            }
        } //

        /// <summary>
        /// Fires when an intruder is confirmed and 
        /// we need to prompt of a password or notify.
        /// </summary>
        /// <param name="e"></param>
        void scanner_onIntrusionDetected(IntrusionEventArgs e)
        {
            // update the label
            ++this.confirmedIntrusionOccurences;
            this.labelNumConfirmedIntrusions.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                this.labelNumConfirmedIntrusions.Content = this.confirmedIntrusionOccurences;
            }));
            //
            // handle the notification
            // note that the scanner will automatically turn itself off
            if (this.CheckAccess())
            {
                //
                // get the image
                handleNotify(e.imageFrame);
            }
            else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    //
                    // get the image
                    handleNotify(e.imageFrame);
                }));
            }
            // log
            Logger.log("intrusion confirmed.");
        } //

        /// <summary>
        /// Fires when an image needs to be put on the video source.
        /// </summary>
        /// <param name="image"></param>
        void scanner_onCameraImageReady(byte[] imageRaw)
        {
            // update the video image, because we have to invoke(), we have to do it this way...
            BitmapSource img = null; // this is here so that we can save it if we are streaming
            if (this.imageVideo.CheckAccess())
            {
                BitmapSource image = imageRaw.createImage(); // extension method
                img = image;
                this.imageVideo.Source = image;
            }
            else
            { // invoke on GUI thread
                this.imageVideo.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    BitmapSource image = imageRaw.createImage();
                    this.imageVideo.Source = image;
                    img = (BitmapSource)this.imageVideo.Source;
                }));
            }
            // check if we're streaming
            if (this.streamer != null)
            {
                // unfortunately we have to do it this way in streaming...
                // the convert to raw directly from the camera image just simply didn't work...
                if (this.streamer.sendingPaused == false)
                {
                    // first save the bitmap 
                    string imgPath = Globals.Storage.tempImagesFolder + "\\strm1.jpg";
                    // save it to a jpeg
                    img.toJpegFile(imgPath);
                    // get the raw data
                    // System.Threading.Thread.Sleep(300); // pause briefly
                    // extract the raw bytes
                    byte[] data = FileManipulator.fileToRaw(imgPath); // get the data
                    // queue this data into our streamer
                    this.streamer.queueData(data);
                }
            }
        } //

        #endregion

        //
        // Streaming Events
        #region ((Streaming Events))

        /// <summary>
        /// Gets fired when a new client connects or disconnects.
        /// </summary>
        void streamer_onStreamCountChanged()
        {
            if (this.labelStreamCount.CheckAccess())
            {
                this.labelStreamCount.Content = this.streamer.streamCount.ToString();
            }
            else
            {
                this.labelStreamCount.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    this.labelStreamCount.Content = this.streamer.streamCount.ToString();
                }));
            }
        }

        #endregion //

        //
        // Window Methods
        #region ((Window Methods))

        /// <summary>
        /// Use with the onScannerStateChanged event.
        /// </summary>
        /// <param name="currentState"></param>
        private void changeScannerControls(ScannerState currentState)
        {
            switch (currentState)
            {
                case ScannerState.error:
                    setScannerWhenError();
                    updateStatusLabel("Scanner error - (reset scanner).");
                    break;
                case ScannerState.ready:
                    setScannerWhenStopped();
                    updateStatusLabel("Scanner ready.");
                    break;
                case ScannerState.scanning:
                    setScannerWhenStarting();
                    updateStatusLabel("Scanner started");
                    break;
                case ScannerState.stopped:
                    setScannerWhenStopped();
                    updateStatusLabel("Scanner stopped.");
                    break;
            }
        } //

        /// <summary>
        /// Completely disables the scanner.
        /// </summary>
        private void disableScanningAll()
        {
            this.buttonReset.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                this.buttonReset.IsEnabled = false;
                this.buttonScanSop.IsEnabled = false;
                this.buttonScanStart.IsEnabled = false;
            }));
        } //

        /// <summary>
        /// Sets the approprate state of controls for the 
        /// scanner while it's scanning.
        /// </summary>
        private void setScannerWhenStarting()
        {
            // use invoke here
            this.buttonReset.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                this.buttonReset.IsEnabled = true;
                this.buttonScanSop.IsEnabled = true;
                this.buttonScanStart.IsEnabled = false;
            }));
        } //

        /// <summary>
        /// Sets the approproate state of the scanner when stopped.
        /// </summary>
        private void setScannerWhenStopped()
        {
            // use invoke here
            this.buttonScanSop.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                this.buttonScanStart.IsEnabled = true;
                this.buttonScanSop.IsEnabled = false;
                this.buttonReset.IsEnabled = false;
                this.progressBarThreshold.Value = 0.0;
                this.binaryLEDThreshold.setLensColor(library.LedIndicator.LensColor.BLANK);
            }));
        } //

        /// <summary>
        /// Sets the scanner state when it's errored out.
        /// </summary>
        private void setScannerWhenError()
        {
            // use invoke here
            this.buttonScanSop.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                this.buttonScanStart.IsEnabled = false;
                this.buttonScanSop.IsEnabled = false;
                this.buttonReset.IsEnabled = true;
            }));
        } //

        #endregion

        //
        // Private Methods
        #region ((Private Methods))

        /// <summary>
        /// Initializes and also reinitializes of the scanner needs i.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="prefs"></param>
        private void scannerInitialize()
        {
            // first check for webcam devices
            if (Globals.Scanning.areDevicesConnected() == false)
            {
                // notify
                Globals.MessageBox.show("Please connect a camera device (USB) before starting the scan.", "No Camera Device Found");
                // break out of here
                return;
            }
            // check the state
            if (this.scanner != null)
            {
                // just reset the scanner
                this.scanner.reset();
            }
            else
            {
                // instantiate the scanner
                this.scanner = new Scanner(this.scanningPreferences);
                // subscribe to scanner events
                this.scanner.onCameraImageReady += new Scanner.ImageReadyEvent(scanner_onCameraImageReady);
                this.scanner.onIntrusionDetected += new Scanner.IntrusionDetectedEvent(scanner_onIntrusionDetected);
                this.scanner.onScannerStateChanged += new Scanner.ScannerStateChangedEvent(scanner_onScannerStateChanged);
                this.scanner.onThresholdBroken += new Scanner.ThresholdBrokenEvent(scanner_onThresholdBroken);
                this.scanner.onThresholdChanged += new Scanner.ThresholdBrokenEvent(scanner_onThresholdChanged);
            }
            // update to the user
            updateStatusLabel("Scanner ready.");
        }

        /// <summary>
        /// Safely updates the streaming status label.
        /// </summary>
        /// <param name="status"></param>
        private void updateStreamStatus(string status, Brush fontBrush = null)
        {
            if (fontBrush == null)
            {
                fontBrush = Brushes.White;
            }
            if (this.labelStreamStatus.CheckAccess())
            {
                this.labelStreamStatus.Foreground = fontBrush;
                this.labelStreamStatus.Content = status;
            }
            else
            {
                this.labelStreamStatus.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    this.labelStreamStatus.Foreground = fontBrush;
                    this.labelStreamStatus.Content = status;
                }));
            }
        }

        /// <summary>
        /// Reads from the configuration storage and populates
        /// the configuration fields for all preferences.
        /// </summary>
        private void readConfiguration()
        {
            if (this.emailPreferences.read())
            {
                this.labelNoficationSettings.Content = "Preferences Loaded";
                Config.current.emailPreferences = this.emailPreferences;
            }
            if (this.scanningPreferences.read())
            {
                // notify using labels
                this.labelDetectionSettings.Content = "Preferences Loaded";
                Config.current.scanningPreferences = this.scanningPreferences;
            }
            
            //this.labelStreamSettings.Content = "Preferences Loaded";
        }

        /// <summary>
        /// Will be called when it's time for nontifications.
        /// </summary>
        /// <param name="photo"></param>
        private void handleNotify(object photo)
        {
            // save the image
            string imagePath = saveImage(photo);
            // check if the user specified to notify / email them immediately without warning
            if (this.passwordPreferences.notifyImmediately)
            {
                //
                sendEmail(imagePath);
                createReport(imagePath);
                speakWarning();
            }
            else
            {
                // speak then prompt for password
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    speakEnterPassword();
                    PasswordEntryWindow passwordPrompt = new PasswordEntryWindow(this.passwordPreferences);
                    passwordPrompt.ShowDialog();
                    //
                    if (passwordPrompt.alarmRaised)
                    { // failed to get the password right...
                        // send notification!
                        sendEmail(imagePath);
                        createReport(imagePath);
                        speakWarning();
                    }
                    else
                    {
                        speakAuthorized(); // the appeared to have entered in the password correctly
                    }
                }));
            }

            if (this.buttonAutoStart.IsChecked == true)
            {
                System.Threading.Thread.Sleep(this.scanningPreferences.startScanningWaitTimeSeconds);
                // speak notification
                speak("Starting");
                // enable/disable based on the state of our streaming check box
                this.scanner.isDetectionEnabled = (this.checkBoxStream.IsChecked == false);
                // start the scanner
                this.scanner.start();
                // to the log
                Logger.log("scanning started");
            }
            else
            {
                // update the status label
                updateStatusLabel("Press start to begin scanning again.", false);
            }
        }

        /// <summary>
        /// Checks if email should be enabled, if so then send the notification.
        /// </summary>
        private void sendEmail(string photoFilePath)
        {
            // check if email was enabled in the dialog
            if (this.emailPreferences.enableEmail)
            {
                // check for the file's photo path
                if (photoFilePath != null)
                {
                    //
                    // check if an attachment path was specified
                    this.emailPreferences.attachmentPaths.Clear();
                    this.emailPreferences.attachmentPaths.Add(photoFilePath);
                }
                this.emailer.setPreferences(this.emailPreferences);
                this.emailer.isEnabled = true;
                // send the email
                try
                {
                    this.emailer.notifyAsync();
                    updateStatusLabel("Email sent.");
                }
                catch (Exception ex)
                {
                    //
                    Logger.log("Exception (sending email): " + ex.Message);
                    // maybe it's the attachments?
                    this.emailer.removeAttachments();
                    try
                    {
                        //
                        // try again... try everything possible to notify
                        this.emailer.notify();
                    }
                    catch (Exception ex2)
                    {
                        // apparently nature itself is against us... or some information wasn't specified.
                        updateStatusLabel("Email not sent");
                        Logger.log("Exception (sending email - attempt 2): " + ex2.Message);
                    }
                }
            } // email enabled
        } //

        /// <summary>
        /// Saves the image in the app data folder and returns the path to the image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private string saveImage(object image)
        {
            BitmapSource bitmap = image as BitmapSource;
            string path = Globals.Storage.appSpecificDataFolder + "\\intruder_" + Reporting.getFileFriendlyDateString() + ".jpg";
            try
            {
                bitmap.toJpegFile(path);
                return path;
            }
            catch (Exception ex)
            {
                Logger.log("Exception in saving image: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Dumps an html report to the specified directory
        /// </summary>
        /// <param name="imagePath"></param>
        private void createReport(string imagePath)
        {
            // create/save the report
            try
            {
                app.motionalarm.reporting.Reporting.create(
                    Globals.Storage.reportsFolder + "\\report_" + Reporting.getFileFriendlyDateString() + ".htm",
                    imagePath
                );
            }
            catch (Exception ex)
            {
                Logger.log("Exception in creating HTML report: " + ex.Message);
                Globals.MessageBox.show(ex.Message, "Error Creating Report", true);
            }
        }

        /// <summary>
        /// Populates the drop down ribbon portion with a list of reports.
        /// </summary>
        private void showCurrentReports()
        {
            // this will not bomb if something happens
            string[] reportNames = Reporting.getAllReportNames(Globals.Storage.reportsFolder);
            // check how to update the label
            if (this.labelReportsList.CheckAccess())
            {
                // first clear the content
                this.labelReportsList.Content =
                    reportNames.Length == 0 ? "[No Reports Found]" : reportNames.Length.ToString() + " reports.  (Click Here to View).";
            }
            else
            {
                this.labelReportsList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    // first clear the content
                    // first clear the content
                    this.labelReportsList.Content =
                        reportNames.Length == 0 ? "[No Reports Found]" : reportNames.Length.ToString() + " reports.  (Click Here to View).";

                }));
            }
        }

        /// <summary>
        /// Speaks the enter password warning asynchronously.
        /// </summary>
        private void speakEnterPassword()
        {
            if (speechEnabled == true)
            {
                this.speechSynthesizer.SpeakAsync("Please enter the password to disable notification.");
            }
        } //

        /// <summary>
        /// Call this when an intruder is confirmed.
        /// </summary>
        private void speakWarning()
        {
            if (speechEnabled == true)
            {
                string message = this.scanningPreferences.intrudersName.Length == 0 ? "" : this.scanningPreferences.intrudersName + ".  ";
                message += this.scanningPreferences.message;
                this.speechSynthesizer.SpeakAsync(message);
            }
        } //

        /// <summary>
        /// Call this when the password they entered is correct.
        /// </summary>
        private void speakAuthorized()
        {
            if (speechEnabled == true)
            {
                this.speechSynthesizer.SpeakAsync(
                    "Password Accepted! ... Press start to begin scanning."
                );
            }
        } //

        /// <summary>
        /// What is spoken when the scanner starts.
        /// </summary>
        /// <param name="timeLength"></param>
        private void speakStarting(int timeLength)
        {
            this.speechSynthesizer.SpeakAsync(
                "Scanning will begin in " + (timeLength / 1000).ToString() + " seconds." +
                "... Please leave the area."
            );
        }

        /// <summary>
        /// Speaks the specified sentence async.
        /// </summary>
        /// <param name="sentence"></param>
        private void speak(string sentence, bool isTest = false)
        {
            if (isTest == true)
            {   // if testing... then make sure to speak irregardless
                this.speechSynthesizer.SpeakAsync(sentence);
            }
            else
            {
                if (speechEnabled == true)
                {
                    this.speechSynthesizer.SpeakAsync(sentence);
                }
            }
        }

        /// <summary>
        /// Handles errors when starting to stream.
        /// </summary>
        private void startStreaming()
        {
            try
            {
                this.streamer.startAccepting();
                updateStatusLabel("Streaming started.");
            }
            catch (Exception ex)
            {
                Logger.log("Exception in starting streamer: " + ex.Message);
                Globals.MessageBox.show("Could not begin streaming\n" + ex.Message, "Streaming Error.", true);
                updateStatusLabel("Streaming error.");                
                this.streamer.close();
                updateStreamStatus("ERROR", Brushes.Red);
            }
        }

        /// <summary>
        /// Safely updates the status message label.
        /// </summary>
        /// <param name="status"></param>
        private void updateStatusLabel(string status, bool includeTime = true)
        {
            if (this.labelStatusMessage.CheckAccess())
            {
                this.labelStatusMessage.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    string time = string.Format("[{0}:{1}:{2}]", System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second);
                    this.labelStatusMessage.Content = status + (includeTime ? time : "");
                }));
            }
            else
            {
                this.labelStatusMessage.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    string time = string.Format("[{0}:{1}:{2}]", System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second);
                    this.labelStatusMessage.Content = status + (includeTime ? time : "");
                }));
            }
        }

        #endregion

        //
        // Fields
        #region ((Fields))

        bool ledNormalLock = false;
        bool ledBrokenLock = false;
        bool speechEnabled = true;
        Scanner scanner = null;
        StreamManager streamer = null;
        ScanningPreferences scanningPreferences = new ScanningPreferences(); // default
        Emailer emailer = null;
        EmailPreferences emailPreferences = new EmailPreferences();
        SpeechSynthesizer speechSynthesizer = null;
        PasswordPreferences passwordPreferences = new PasswordPreferences();
        uint thresholdBrokenOccurences = 0;
        uint confirmedIntrusionOccurences = 0;
        Brush updatedBrush = Brushes.Silver;

        #endregion
    }
}
