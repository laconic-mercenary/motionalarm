namespace motionalarm {

    using System;
    using System.Timers;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;

    using app.motionalarm.configuration;

    /// <summary>
    /// Interaction logic for PasswordEntryWindow.xaml
    /// </summary>
    public partial class PasswordEntryWindow : Window {

        /// <summary>
        /// Creates a dialog that forces a password entry.
        /// </summary>
        /// <param name="preferences"></param>
        public PasswordEntryWindow(PasswordPreferences preferences) {
            //
            // build interface
            InitializeComponent();

            //
            // pass over preferences
            this.prefs = preferences;
            this.secondsLeft = this.prefs.countDownSeconds;
            this.attemptsLeft = this.prefs.attemptsAllowed;

            //
            // construct timer
            countDownTimer = new Timer();
            countDownTimer.Elapsed += new ElapsedEventHandler(countDownTimer_Elapsed);
            countDownTimer.Interval = 1000.0; // 1 second
            
            //
            // subscribe events
            this.buttonRaiseAlarm.Click += new RoutedEventHandler(buttonRaiseAlarm_Click);
            this.buttonSubmit.Click += new RoutedEventHandler(buttonSubmit_Click);
            this.onPasswordCorrect += new PasswordCorrectEvent(PasswordEntryWindow_onPasswordCorrect);
        }

        /// <summary>
        /// On loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PasswordEntryWindow_Loaded(object sender, RoutedEventArgs e) {
            countDownTimer.Start();
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() {
                this.labelTimeLeft.Content = this.secondsLeft;
                this.labelAttempts.Content = this.attemptsLeft;
            }));
        }

        /// <summary>
        /// When the dialog is closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            disposeTimer();
        }

        /// <summary>
        /// Fires when the password entered is correct.
        /// </summary>
        void PasswordEntryWindow_onPasswordCorrect() {
            //
            // good to go
            _alarmRaised = false;
            setDialogResult(true);
        }

        /// <summary>
        /// Fires when the time expires.
        /// </summary>
        void PasswordEntryWindow_onTimeExpired() {
            //
            // dispose the timer
            disposeTimer();
            raiseAlarm();
        }

        public delegate void PasswordCorrectEvent();
        public event PasswordCorrectEvent onPasswordCorrect = null;

        /// <summary>
        /// Fires when the count down timer elapses (with each second)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void countDownTimer_Elapsed(object sender, ElapsedEventArgs e) {
            --this.secondsLeft;
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() {
                this.labelTimeLeft.Content = this.secondsLeft;
                this.labelAttempts.Content = this.attemptsLeft;
            }));
            if (this.secondsLeft <= 0) {
                raiseAlarm();
            }
        }

        /// <summary>
        /// Fires when the submit button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonSubmit_Click(object sender, RoutedEventArgs e) {
            if (!checkPassword()) {
                //
                // FAIL!
                onPasswordFailure();
            }
            this.textBoxPassword.Clear();
        }

        /// <summary>
        /// Fires when the raise alarm button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void buttonRaiseAlarm_Click(object sender, RoutedEventArgs e) {
            raiseAlarm();
        }

        /// <summary>
        /// called when a password was entered that was not correct.
        /// </summary>
        void onPasswordFailure() {
            --this.attemptsLeft;
            this.labelMessage.Foreground = Brushes.Red;
            this.labelMessage.Content = "Incorrect Password";
            if (this.attemptsLeft <= 0) {
                raiseAlarm();
            }
        }

        /// <summary>
        /// Handles alarm raising.
        /// </summary>
        void raiseAlarm() {
            _alarmRaised = true;
            setDialogResult(false);
        }

        /// <summary>
        /// Checks the password and returns true if it matches, else false.
        /// </summary>
        /// <returns></returns>
        bool checkPassword() {
            string finalEntry = this.textBoxPassword.Text;
            if (string.Compare(prefs.password, finalEntry) == 0) {
                //
                // succeeded
                if (onPasswordCorrect != null) {
                    onPasswordCorrect();
                }
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Safely sets the dialog result.
        /// </summary>
        /// <param name="result"></param>
        void setDialogResult(bool? result) {
            if (this.CheckAccess()) {
                disposeTimer();
                this.DialogResult = result;
            }
            else {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() {
                    disposeTimer();
                    this.DialogResult = result;
                }));
            }
        }

        /// <summary>
        /// Gets rid of the timer.
        /// </summary>
        void disposeTimer() {
            if (this.countDownTimer != null) {
                this.countDownTimer.Stop();
                this.countDownTimer.Elapsed -= countDownTimer_Elapsed;
                this.countDownTimer.Dispose();
                this.countDownTimer = null;
            }
        }

        /// <summary>
        /// Gets (readonly) if the alarm
        /// should be raised.
        /// </summary>
        public bool alarmRaised {
            get {
                return _alarmRaised;
            }
        }

        private Timer countDownTimer = null;
        private int secondsLeft = 0;
        private int attemptsLeft = 0;
        private PasswordPreferences prefs;
        private bool _alarmRaised = false;

    }
}
