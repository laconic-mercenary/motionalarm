
namespace app.motionalarm
{

    using System.Collections.Generic;
    using AForge.Video.DirectShow;
    using app.motionalarm.configuration;
    using Dir = System.IO.Directory;
    using System.IO;

    internal static class Globals
    {

        /// <summary>
        /// Contains information specific to the application, such as company and version info.
        /// </summary>
        public static class Application
        {

            /// <summary>
            /// Gets the application name.
            /// </summary>
            public static string applicationName
            {
                get
                {
                    return "E-Motion";
                }
            }

            public static string companyName
            {
                get
                {
                    return "Frontier Outpost, LLC";
                }
            }

            /// <summary>
            /// Gets the application version.
            /// </summary>
            public static string applicationVersion
            {
                get
                {
                    return "0.1.0.0";
                }
            }
        } // Application

        /// <summary>
        /// Gets constants that help with data storage.
        /// </summary>
        public static class Storage
        {

            /// <summary>
            /// Creates all the necessary folders the application to use.
            /// </summary>
            /// <returns></returns>
            public static bool createDirectories()
            {
                if (Dir.Exists(appSpecificDataFolder) == false)
                {
                    try
                    {
                        Dir.CreateDirectory(Storage.appSpecificDataFolder);
                        Dir.CreateDirectory(Storage.tempImagesFolder);
                        Dir.CreateDirectory(Storage.reportsFolder);
                        return true;
                    }
                    catch
                    {
                        try
                        {
                            string local = "\\" + Globals.Application.companyName + "\\" + Globals.Application.applicationName;
                            Dir.CreateDirectory(local);
                            Dir.CreateDirectory(local + "\\img");
                            _appSpecificDataFolder = local;
                            _tempImagesFolder = local + "\\img";
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            /// <summary>
            /// Gets the location of the folder where reports will go when created.
            /// </summary>
            public static string reportsFolder
            {
                get
                {
                    if (_reportsFolder == null)
                    {
                        _reportsFolder = Storage.appSpecificDataFolder + "\\reports";
                    }
                    return _reportsFolder;
                }
            }
            private static string _reportsFolder = null;

            /// <summary>
            /// Gets the path to the image folder that is used to store
            /// temporary camera images used later for streaming.
            /// </summary>
            public static string tempImagesFolder
            {
                get
                {
                    if (_tempImagesFolder == null)
                    {
                        _tempImagesFolder = Storage.appSpecificDataFolder + "\\img";
                    }
                    return _tempImagesFolder;
                }
            }
            private static string _tempImagesFolder = null;

            /// <summary>
            /// Gets the common\application data folder that is operating system 
            /// specific.
            /// </summary>
            public static string appDataFolder
            {
                get
                {
                    // cache this, we don't know how expensive it is to 
                    // get these environment paths
                    if (_appDataFolder == null)
                    {
                        _appDataFolder = System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.CommonApplicationData,
                            System.Environment.SpecialFolderOption.None
                        );
                    }
                    return _appDataFolder;
                }
            }
            private static string _appDataFolder = null;

            /// <summary>
            /// Gets the folder where data should be stored by the application.
            /// </summary>
            public static string appSpecificDataFolder
            {
                get
                {
                    if (_appSpecificDataFolder == null)
                    {
                        _appSpecificDataFolder = appDataFolder +
                            "\\" + Globals.Application.companyName +
                            "\\" + Globals.Application.applicationName;
                    }
                    return _appSpecificDataFolder;
                }
            }
            private static string _appSpecificDataFolder = null;

            // NEW:
            public static string scanningPreferencesFile
            {
                get
                {
                    if (_scanningPreferencesFile == null)
                    {
                        string filePath = appSpecificDataFolder + "//streamPreferences.preferences";
                        if (!File.Exists(filePath))
                        {
                            File.Create(filePath).Close();
                        }
                        _scanningPreferencesFile = filePath;
                    }
                    return _scanningPreferencesFile;
                }
            }
            private static string _scanningPreferencesFile = null;

            // NEW:
            public static string emailPreferencesFile
            {
                get
                {
                    if (_emailPreferencesFile == null)
                    {
                        string filePath = appSpecificDataFolder + "//emailPreferences.preferences";
                        if (!File.Exists(filePath))
                        {
                            File.Create(filePath);
                        }
                        _emailPreferencesFile = filePath;
                    }
                    return _emailPreferencesFile;
                }
            }
            private static string _emailPreferencesFile = null;
        }

        /// <summary>
        /// Provides a unique message box.
        /// </summary>
        public static class MessageBox
        {

            /// <summary>
            /// Shows an information message box.
            /// </summary>
            /// <param name="isError"></param>
            /// <param name="message"></param>
            /// <param name="title"></param>
            public static void show(string message, string title, bool isError = false)
            {
                System.Windows.MessageBox.Show(
                    message,
                    title,
                    System.Windows.MessageBoxButton.OK,
                    isError ? System.Windows.MessageBoxImage.Error : System.Windows.MessageBoxImage.Information
                );
            }

            /// <summary>
            /// Shows a yes now dialog, returns true if yes was the result, else no.
            /// </summary>
            /// <param name="message"></param>
            /// <param name="title"></param>
            /// <returns></returns>
            public static bool showYesNo(string message, string title)
            {
                System.Windows.MessageBoxResult result =
                System.Windows.MessageBox.Show(
                    message,
                    title,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
                return (result == System.Windows.MessageBoxResult.Yes);
            }

        }

        /// <summary>
        /// Contains information specific to scanning that will never change
        /// and is not editable by a user.
        /// </summary>
        public static class Scanning
        {

            /// <summary>
            /// This is the time that is used in the Scanner.start() method that gives
            /// the camera some time to focus the image before we unlock the motion
            /// detector and begin searching for motion.  This is necessary because there
            /// were times when if an intrusion was confirmed (the scanner stopped), then 
            /// if start() was called again, the threshold was already tripped, and it would
            /// remain that way for a while until the alarm was raised.  This provides the
            /// necessary leeway.
            /// </summary>
            public static readonly int SCANNER_COOL_DOWN_TIME = 5000;
            
            /// <summary>
            /// Gets a complete list of the device monikers attached to this
            /// machine.  If nothing is attached, it returns null.
            /// </summary>
            /// <returns></returns>
            public static string[] getDevicesMonikers()
            {
                FilterInfoCollection filters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (filters.Count != 0)
                {
                    List<string> mons = new List<string>();
                    for (int i = 0; i < filters.Count; i++)
                    {
                        mons.Add(filters[i].MonikerString);
                    }
                    return mons.ToArray();
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the human readable name of the device.
            /// </summary>
            /// <returns></returns>
            public static string[] getDeviceNames()
            {
                FilterInfoCollection filters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (filters.Count != 0)
                {
                    List<string> mons = new List<string>();
                    for (int i = 0; i < filters.Count; i++)
                    {
                        mons.Add(filters[i].Name);
                    }
                    return mons.ToArray();
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets whether or not there are devices that can be used 
            /// for scanning currently connected to this machine.
            /// </summary>
            /// <returns></returns>
            public static bool areDevicesConnected()
            {
                return (Globals.Scanning.getDevicesMonikers() != null);
            }
        }

        /// <summary>
        /// Contains values for using in streaming camera images.
        /// </summary>
        public static class Streaming
        {
            /// <summary>
            /// Gets the local port to bind to.
            /// </summary>
            public static readonly int DEFAULT_LOCAL_PORT = 34329;
        }
    }
}
