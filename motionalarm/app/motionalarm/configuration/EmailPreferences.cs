
namespace app.motionalarm.configuration
{
    using ArrayList = System.Collections.Generic.List<string>;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    [Serializable]
    public class EmailPreferences
    {
        /// <summary>
        /// Provides default values (no email, no attachments)
        /// </summary>
        public EmailPreferences()
        {
            this.addresses = new ArrayList();
            this.attachmentPaths = new ArrayList();
            this.subjectLine = "INTRUDER DETECTED!";
            this.body = "An intruder was detected where you setup the motion alarm software.";
            this.enableEmail = false;
        }

        public bool save()
        {
            try
            {
                using (Stream stream = new FileStream(Globals.Storage.emailPreferencesFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this);
                }
                return true;
            }
            catch (Exception ex)
            {
                logging.Logger.log(ex.Message);
            }
            return false;
        }

        public bool read()
        {
            if (!File.Exists(Globals.Storage.emailPreferencesFile))
            {
                return false;
            }
            try
            {
                using (Stream stream = new FileStream(Globals.Storage.emailPreferencesFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    IFormatter formatter = new BinaryFormatter();
                    EmailPreferences preferences = (EmailPreferences)formatter.Deserialize(stream);
                    this.addresses = preferences.addresses;
                    this.attachmentPaths = preferences.attachmentPaths;
                    this.body = preferences.body;
                    this.enableEmail = preferences.enableEmail;
                    this.subjectLine = preferences.subjectLine;
                }
                return true;
            }
            catch (Exception ex)
            {
                logging.Logger.log(ex.Message);
            }
            return false;
        }

        public ArrayList addresses { get; set; }
        public ArrayList attachmentPaths { get; set; }
        public string subjectLine { get; set; }
        public string body { get; set; }
        public bool enableEmail { get; set; }
    }

}
