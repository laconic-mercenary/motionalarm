
namespace app.motionalarm.scanning
{

    using System.Drawing;
    using System.Windows.Media.Imaging;
    using AForge.Video;
    using AForge.Video.DirectShow;
    using AForge.Vision.Motion;
    using app.motionalarm.configuration;
    using app.motionalarm.imaging;

    public class Scanner
    {
        #region ((CTOR/DTOR))

        /// <summary>
        /// Initializes the scanner making it ready for camera shooting and motion detection.
        /// </summary>
        /// <param name="windowHwnd"></param>
        /// <param name="preferences"></param>
        public Scanner(ScanningPreferences preferences)
        {   // pass to our init method
            initialize(preferences);
        }

        /// <summary>
        /// Guaranteed to get called for cleanup
        /// </summary>
        ~Scanner()
        {
            // dispose
            destroy();
        }
        #endregion

        #region ((Scanner Operation Methods))

        /// <summary>
        /// Starts the camera and releases the motion detector lock.
        /// Call stop() to pause the camera and detection.
        /// </summary>
        public void start()
        {   // star the image grabber
            if (this.scannerState == ScannerState.ready || this.scannerState == ScannerState.stopped)
            {
                // reset the detector
                this.motionDetector.Reset();
                // 
                lock (lockObject)
                {
                    // set to normal
                    this._detectionState = DetectionState.normal;
                    // start the camera
                    this.device.Start();
                    // pause for the camera to get focus
                    // System.Threading.Thread.Sleep(Globals.Scanning.scannerCoolDownTime); // this is necessary...
                    // unlock the detector
                    this.scannerCoolDownTimer.Start();
                    // this.motionDetectionLock = false;
                }
                changeState(ScannerState.scanning);
            }
            else
            {
                throw new ScannerException(
                    this, "Cannot start scanner in state: " + this.scannerState.ToString());
            }
        }

        /// <summary>
        /// Stops the camera and the motion detector.
        /// Call start to resume the process.
        /// </summary>
        public void stop()
        {   // stop the image grabber
            if (this.scannerState == ScannerState.scanning)
            {
                // enforce a lock on our camera and timer
                if (this.scannerCoolDownTimer != null)
                {
                    this.scannerCoolDownTimer.Stop();
                }
                // 
                lock (lockObject)
                {
                    // set the detection lock
                    this.motionDetectionLock = true;
                }
                // reset detector
                this.motionDetector.Reset();
                // reset to a normal state, only if the threshold is broken...
                // we may need to preserve the state if an intrusion was confirmed...
                if (this.detectionState == DetectionState.thresholdBroken)
                {
                    this._detectionState = DetectionState.normal;
                }
                // stop the camera
                this.device.SignalToStop();
                // stop the timer
                this.intrusionConfirmedTimer.Stop();
                // 
                changeState(ScannerState.stopped);
            }
        }

        /// <summary>
        /// Stops the detection process and camera, and then 
        /// re-initializes the scanner.
        /// </summary>
        public void reset()
        {   // stop detecting first...
            stop();
            // re-initialize
            initialize(this.scanningPreferences);
        }

        /// <summary>
        /// Disposes the scanner.  Will need to reinstantiate a new scanner
        /// in order to make use of this object again.
        /// </summary>
        public void destroy()
        {
            //
            // release the camera
            // lock down our components
            lock (lockObject)
            {
                if (this.device != null)
                {
                    this.device.SignalToStop();
                    this.device = null;
                }
            }
            //
            // release the motion detector
            if (this.motionDetector != null)
            {
                this.motionDetector.Reset();
                this.motionDetector = null;
            }

            if (this.scannerCoolDownTimer != null)
            {
                this.scannerCoolDownTimer.Stop();
                this.scannerCoolDownTimer.Dispose();
                this.scannerCoolDownTimer = null;
            }

            //
            // dispose the timer
            if (this.intrusionConfirmedTimer != null)
            {
                this.intrusionConfirmedTimer.Stop();
                this.intrusionConfirmedTimer.Dispose();
                this.intrusionConfirmedTimer = null;
            }
            // release the handle
            this._scannerLastFrame = null;
            this._detectionState = DetectionState.normal;
            //
            // set defunct states
            changeState(ScannerState.error);
            this.currentE = null;
            this.scanningPreferences = null;
        }

        //
        // Private

        /// <summary>
        /// Initializes all components of the scanner and places in a ready state.
        /// </summary>
        /// <param name="windowHwnd"></param>
        /// <param name="preferences"></param>
        private void initialize(ScanningPreferences preferences)
        {
            //
            // check preferences
            if (preferences == null)
            {
                throw new ScannerException(this, "Preferences cannot be null.");
            }
            // init
            if (Globals.Scanning.areDevicesConnected() == false)
            {
                Globals.MessageBox.show(
                    "Please connect a web camera device or your camera drivers were not installed properly.",
                    "ERROR - No Camera detected.", true);
                return;
            }
            // check the device moniker
            if (Config.current.videoPreferences.currentDeviceMoniker == null)
            {
                Globals.MessageBox.show("A camera device was not detected.  Please choose a device by clicking on Video Settings.", "ERROR - No Camera Device", true);
                return;
            }
            else
            {
                // mutex lock the device
                lock (lockObject)
                {
                    // create the device using the selected device moniker
                    this.device = new VideoCaptureDevice(Config.current.videoPreferences.currentDeviceMoniker);
                    // this can only be from 800x600 down to... i dunno, for sure down to 320x240
                    this.device.DesiredFrameSize = Config.current.videoPreferences.resolution;
                    // subscribe to the device's new frame ready event
                    this.device.NewFrame += new AForge.Video.NewFrameEventHandler(imageGrabber_ImageCaptured);
                }
                // check if we need to kill the cool down timer...
                if (this.scannerCoolDownTimer != null)
                {
                    this.scannerCoolDownTimer.Stop();
                    this.scannerCoolDownTimer.Dispose();
                    this.scannerCoolDownTimer = null;
                }
                // re-establish the cool down timer
                this.scannerCoolDownTimer = new System.Timers.Timer();                
                this.scannerCoolDownTimer.Interval = Globals.Scanning.SCANNER_COOL_DOWN_TIME;
                this.scannerCoolDownTimer.Elapsed += new System.Timers.ElapsedEventHandler(scannerCoolDownTimer_Elapsed);
            }

            //
            // initialize the motion detector
            if (this.motionDetector == null)
            {
                this.motionDetector = new MotionDetector(
                    new SimpleBackgroundModelingDetector(),
                    //new TwoFramesDifferenceDetector(true),
                    new MotionAreaHighlighting()
                );
            }
            // also reset the detector
            this.motionDetector.Reset();
            //
            // pass over the preferences
            this.scanningPreferences = preferences;
            // unreference the last frame
            this._scannerLastFrame = null;
            this._detectionState = DetectionState.normal;
            //
            // setup the intrusion detection timer
            this.intrusionConfirmedTimer = new System.Timers.Timer(this.scanningPreferences.intrusionConfirmationWaitTime);
            this.intrusionConfirmedTimer.Elapsed += new System.Timers.ElapsedEventHandler(intrusionConfirmedTimer_Elapsed);
            this.intrusionConfirmedTimer.Interval = this.scanningPreferences.intrusionConfirmationWaitTime; // convert to ms
            //
            // set motion detection lock
            this.motionDetectionLock = true; // calling start() will unlock this
            this.isDetectionEnabled = true;
            //
            // set the new state
            changeState(ScannerState.ready);
        }

        #endregion

        #region ((Private Methods))

        /// <summary>
        /// Handles the intrusion confirmed event.
        /// </summary>
        private void handleIntrusionConfirmed()
        {
            // change the state
            this._detectionState = DetectionState.intrusionConfirmed;
            // stop the scanning process
            stop();
            // fire the main event
            onIntrusionDetected(new IntrusionEventArgs
            {
                imageFrame = this._scannerLastFrame,
                timeOccured = System.DateTime.Now
            });
        }

        /// <summary>
        /// Changes the scanner state with a notify.
        /// </summary>
        /// <param name="state"></param>
        private void changeState(ScannerState state)
        {
            if (onScannerStateChanged != null)
            {
                onScannerStateChanged(state, this.scannerState);
            }
            this._currentState = state;
        }

        /// <summary>
        /// Method that handles a new image for motion detection purposes.
        /// </summary>
        /// <param name="image"></param>
        private void detectImage(Bitmap image)
        {
            // get the current threshold
            float currentThreshold = -1.0f;
            // mutex lock the detector
            lock (lockObject)
            {
                // get the difference
                currentThreshold = this.motionDetector.ProcessFrame(image);
            }
            // see if we are detecting or not
            if (this.motionDetectionLock == true)
            {
                return; // break out of here
            }
            // verify the situation we are in with the detector
            if (currentThreshold >= this.scanningPreferences.bitmapDifferenceTolerancePercentage)
            {
                // exceeded our threshold
                // fire the event
                if (this.detectionState == DetectionState.normal)
                {
                    // enforce a lock on our timer
                    lock (lockObject)
                    {
                        // start the timer
                        this.intrusionConfirmedTimer.Start();
                    }
                    // fire the event
                    onThresholdBroken(this.currentE);
                    // set our lock
                    this._detectionState = DetectionState.thresholdBroken;
                }
            }
            else
            {
                // all is normal, threshold not exceeded
                this.intrusionConfirmedTimer.Stop();
                this._detectionState = DetectionState.normal;
            }
            // pass over the information
            this.currentE.currentThreshold = currentThreshold;
            this.currentE.maxThreshold = this.scanningPreferences.bitmapDifferenceTolerancePercentage;
            this.currentE.imageFrame = image as object;
            this.currentE.timeOccured = System.DateTime.Now;
            // fire the changed event
            if (onThresholdChanged != null)
            {
                onThresholdChanged(this.currentE);
            }
        }

        #endregion

        #region ((Event Methods))
        //
        // Events

        /// <summary>
        /// Fires when the scanner cool down is done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void scannerCoolDownTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.motionDetectionLock = false;
            this.scannerCoolDownTimer.Stop();
        }

        /// <summary>
        /// Fires when our camera grabs a new image for displaying and detecting.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void imageGrabber_ImageCaptured(object source, NewFrameEventArgs e)
        {
            // check if we even have a frame to use
            if (e.Frame == null)
            {
                return;
            }
            // because we're using separate threads, need to convert this to raw data first
            byte[] data = e.Frame.toBitmapSource().getEncodedImageData(".bmp");
            // create a bitmap source
            BitmapSource wpfImage = data.createImage();
            // check if detecting or not
            if (this.isDetectionEnabled == true)
            {
                // pass to detector
                detectImage(e.Frame);
            }
            // pass over the last image reference
            this._scannerLastFrame = null;
            this._scannerLastFrame = wpfImage;
            // fire the event that will display the image on the video pane
            // and also possibly save it for streaming
            if (onCameraImageReady != null)
            {
                onCameraImageReady(data);
            }
        } //

        /// <summary>
        /// Fires when the scanner has stayed above the threshold
        /// for the time specified in the intrusion confirmed timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void intrusionConfirmedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            handleIntrusionConfirmed();
        } //

        #endregion

        #region ((Events & Delegates))

        public delegate void ThresholdBrokenEvent(ThresholdBrokenEventArgs e);
        public delegate void IntrusionDetectedEvent(IntrusionEventArgs e);
        public delegate void ImageReadyEvent(byte[] imageRaw);
        public delegate void ScannerStateChangedEvent(ScannerState newState, ScannerState oldState);

        public event ThresholdBrokenEvent onThresholdBroken = null;
        public event IntrusionDetectedEvent onIntrusionDetected = null;
        public event ImageReadyEvent onCameraImageReady = null;
        public event ScannerStateChangedEvent onScannerStateChanged = null;
        public event ThresholdBrokenEvent onThresholdChanged = null;

        #endregion

        #region ((Properties & Fields))

        /// <summary>
        /// Gets the current state of the scanner.
        /// </summary>
        public ScannerState scannerState
        {
            get
            {
                return _currentState;
            }
        }
        private ScannerState _currentState = ScannerState.error;

        /// <summary>
        /// Gets the last bitmap image that was captured
        /// this will be null if the scanner wasn't started.
        /// </summary>
        public BitmapSource currentImage
        {
            get
            {
                return _scannerLastFrame;
            }
        }

        /// <summary>
        /// Gets the current detection state (normal, thresholdBroken, intrusionConfirmed).
        /// </summary>
        public DetectionState detectionState
        {
            get
            {
                return _detectionState;
            }
        }
        private DetectionState _detectionState = DetectionState.normal;

        /// <summary>
        /// Gets or sets if detection should be enabled or not.
        /// Setting this to false will make it so that when an 
        /// image is ready, it will not be scanned.
        /// </summary>
        public bool isDetectionEnabled
        {
            get;
            set;
        }

        //
        // Fields
        private ScanningPreferences scanningPreferences = null;
        // private ImagingDevice.ImageGrabber imageGrabber = null;
        private VideoCaptureDevice device = null;
        private MotionDetector motionDetector = null;
        private bool motionDetectionLock = false;
        private ThresholdBrokenEventArgs currentE = new ThresholdBrokenEventArgs();
        private System.Timers.Timer intrusionConfirmedTimer = null;
        private System.Timers.Timer scannerCoolDownTimer = null;
        private BitmapSource _scannerLastFrame = null;
        private object lockObject = new object();
        #endregion
    }
}
