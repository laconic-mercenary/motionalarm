namespace app.motionalarm.configuration
{
    using app.motionalarm.streaming;

    /// <summary>
    /// Holds preferences that can be made persistant.  Is specific to this 
    /// application only.
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Reads from a data source and updates all fields in the current instance.
        /// </summary>
        /// <param name="path"></param>
        public virtual void read(string path = null)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Saves all the current configuration data for persistance.  
        /// </summary>
        public virtual void save()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Initializes all preferences to default.
        /// </summary>
        public AppConfiguration()
        {
            this.emailPreferences = new EmailPreferences();
            this.scanningPreferences = new ScanningPreferences();
            this.streamPreferences = new StreamPreferences();
            this.videoPreferences = new VideoPreferences();
        }

        /// <summary>
        /// Gets or sets the current stream preferences used by the application.
        /// </summary>
        public StreamPreferences streamPreferences
        { get; set; }

        /// <summary>
        /// Gets or sets the current email preferences used by the application.
        /// </summary>
        public EmailPreferences emailPreferences
        { get; set; }

        /// <summary>
        /// Gets or sets the current scanning preferences used by the application.
        /// </summary>
        public ScanningPreferences scanningPreferences
        { get; set; }

        /// <summary>
        /// Gets or sets the current video preferences.
        /// </summary>
        public VideoPreferences videoPreferences
        { get; set; }
    }
}
