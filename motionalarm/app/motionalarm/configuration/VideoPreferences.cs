
namespace app.motionalarm.configuration
{
    public class VideoPreferences
    {
        /// <summary>
        /// Gives default values to the fields.
        /// </summary>
        public VideoPreferences()
        {
            // default to 640x480 resolution
            this.resolution = new System.Drawing.Size(640, 480);
            // get the filder data for the default
            string[] deviceMonikers = Globals.Scanning.getDevicesMonikers();
            if (deviceMonikers != null)
            {
                // get the first in the list for default.
                this.currentDeviceMoniker = deviceMonikers[0];
            }
        }

        /// <summary>
        /// Gets or sets the current resolution of the camera.
        /// </summary>
        public System.Drawing.Size resolution
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current device moniker for the device.
        /// </summary>
        public string currentDeviceMoniker
        {
            get;
            set;
        }
    }
}
