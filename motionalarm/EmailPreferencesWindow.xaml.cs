
namespace motionalarm
{

    using System.Windows.Forms;
    using app.motionalarm.configuration;
    using System.Windows;
    using app.motionalarm;

    /// <summary>
    /// Interaction logic for EmailPreferencesWindow.xaml
    /// </summary>
    public partial class EmailPreferencesWindow : System.Windows.Window
    {
        public EmailPreferencesWindow()
        {
            InitializeComponent();
            this.buttonOk.Click += new RoutedEventHandler(buttonOk_Click);
            this.checkBoxEnableEmail.Checked += new RoutedEventHandler(checkBoxEnableEmail_Checked);
            // set defaults
            setDefaults();
        }

        void setDefaults()
        {
            EmailPreferences currentPreferences = Config.current.emailPreferences;

            if (currentPreferences.enableEmail)
            {
                this.checkBoxEnableEmail.IsChecked = true;
                if (currentPreferences.addresses != null)
                {
                    this.textBoxEmails.Text = "";
                    foreach (string address in currentPreferences.addresses)
                    {
                        this.textBoxEmails.Text += address + ", ";
                    }
                    this.textBoxEmails.Text = this.textBoxEmails.Text.TrimEnd(',');
                }
            }
            else
            {
                // the email list
                this.textBoxEmails.Text = "(ENTER EMAILS HERE) - that will receive this notification.  Separate each email address with a comma (,).";
            }
            // sets the subject default
            this.textBoxSubject.Text = Config.current.emailPreferences.subjectLine;
            // the body of the email
            this.textBoxBody.Text = Config.current.emailPreferences.body;
        }

        void checkBoxEnableEmail_Checked(object sender, RoutedEventArgs e)
        {
            if (alreadyCleared)
            {
                return;
            }
            if (this.checkBoxEnableEmail.IsChecked == true)
            {
                this.textBoxEmails.Clear();
                this.alreadyCleared = true;
            }
        }

        void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (checkOk())
            {
                buildValues();
                this.DialogResult = true;
            }
        }

        bool checkOk()
        {
            if (checkBoxEnableEmail.IsChecked == true)
            {
                // email
                if (this.textBoxEmails.Text.Length == 0)
                {
                    app.motionalarm.Globals.MessageBox.show(
                        "Please enter an email address to send to if you are enabling this feature.",
                        "Email Error"
                    );
                    return false;
                }
            }
            return true;
        }

        void buildValues()
        {
            _emailPreferences = new EmailPreferences();
            _emailPreferences.enableEmail = (bool)this.checkBoxEnableEmail.IsChecked;

            _emailPreferences.body = this.textBoxBody.Text;
            _emailPreferences.subjectLine = this.textBoxSubject.Text;
            _emailPreferences.addresses = new System.Collections.Generic.List<string>(
                    this.textBoxEmails.Text.Split(new string[] { "," },
                    System.StringSplitOptions.RemoveEmptyEntries)
                );

            // lastly, check if there are addresses to send to
            // we cannot send anything if there are no addresses
            if (_emailPreferences.enableEmail)
            {
                _emailPreferences.enableEmail = (_emailPreferences.addresses.Count != 0);
            }
        }

        /// <summary>
        /// Gets the preferences established by the dialog.
        /// </summary>
        public EmailPreferences emailPreferences
        {
            get
            {
                return _emailPreferences;
            }
        }
        private EmailPreferences _emailPreferences = new EmailPreferences();

        private bool alreadyCleared = false;
    }
}
