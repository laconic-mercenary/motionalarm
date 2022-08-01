using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace app.motionalarm.configuration
{
    [Serializable]
    public class ScanningPreferences
    {
        /// <summary>
        /// Creates default values.
        /// </summary>
        public ScanningPreferences()
        {
            // apply default values
            this.bitmapDifferenceTolerancePercentage = 0.20f; // 20%
            this.startScanningWaitTimeSeconds = 10000; // 10 seconds
            this.intrusionConfirmationWaitTime = 4000; // 4 seconds
            this.intrudersName = string.Empty; // no name
            // stick it to them
            this.message = "You are trespassing.  I have notified the appropriate parties of your intrusion.";
        }

        public bool save()
        {
            try
            {   
                using (Stream stream = new FileStream(Globals.Storage.scanningPreferencesFile, FileMode.Create, FileAccess.Write, FileShare.None))
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
            if (File.Exists(Globals.Storage.scanningPreferencesFile))
            {
                try
                {
                    using (Stream stream = new FileStream(Globals.Storage.scanningPreferencesFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        ScanningPreferences preferences = (ScanningPreferences)formatter.Deserialize(stream);

                        this.bitmapDifferenceTolerancePercentage = preferences.bitmapDifferenceTolerancePercentage;
                        this.intrudersName = preferences.intrudersName;
                        this.intrusionConfirmationWaitTime = preferences.intrusionConfirmationWaitTime;
                        this.message = preferences.message;
                        this.startScanningWaitTimeSeconds = preferences.startScanningWaitTimeSeconds;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    logging.Logger.log(ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the tolerance difference allowed.
        /// </summary>
        public float bitmapDifferenceTolerancePercentage { get; set; }
        /// <summary>
        /// Gets or sets the time in seconds to wait before scanning actually begins
        /// when the user starts the scan.  (Because sometimes the detector will max out
        /// when it first begins.
        /// </summary>
        public int startScanningWaitTimeSeconds { get; set; }
        /// <summary>
        /// Gets or sets the time in seconds to wait when an intrusion was detected
        /// to where we switch to an intruder confirmed state.
        /// </summary>
        public int intrusionConfirmationWaitTime { get; set; }
        /// <summary>
        /// Gets or sets the anticipated intruders name.
        /// </summary>
        public string intrudersName { get; set; }
        /// <summary>
        /// Gets or sets the message to tell the intruder upon an intrusion confirmation.
        /// </summary>
        public string message { get; set; }
    }
}
