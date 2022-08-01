
namespace app.motionalarm.configuration
{
    public class PasswordPreferences
    {
        /// <summary>
        /// Sets default properties
        /// </summary>
        public PasswordPreferences()
        {
            this.password = string.Empty; // empty password
            this.attemptsAllowed = 2; // 2 attempts
            this.countDownSeconds = 10; // 10 seconds
            this.notifyImmediately = false; // set this to true to not prompt a password
        }

        /// <summary>
        /// Gets or sets the actual password.
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// Gets or sets the number of allowed attempts.
        /// </summary>
        public byte attemptsAllowed { get; set; }
        /// <summary>
        /// Gets or sets the seconds that are allowed before raising the alarm.
        /// (After an intrusion was confirmed.
        /// </summary>
        public int countDownSeconds { get; set; }
        /// <summary>
        /// Set to true if want to notify immediately if an intrusion attempt
        /// occured, and skip the allowed password entry entirely.
        /// </summary>
        public bool notifyImmediately { get; set; }
    }
}
