
namespace motionalarm {

    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using app.motionalarm;


    /// <summary>
    /// Interaction logic for SystemInformationWindow.xaml
    /// </summary>
    public partial class SystemInformationWindow : Window {

        /// <summary>
        /// Dialog ctor.
        /// </summary>
        public SystemInformationWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the window first appears.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            buildSystemInformation();
        }

        /// <summary>
        /// Adds information to the window.
        /// </summary>
        private void buildSystemInformation() {
            //
            // operating system
            try {
                addElement("Operating System", Environment.OSVersion.ToString());
                addElement("Architecture", Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit");
                addElement("User", Environment.UserName);
                addElement("Machine Name", Environment.MachineName);
                addElement(".NET Runtime Ver.", Environment.Version.ToString());
                addElement("Processors", Environment.ProcessorCount.ToString());
                getCDriveInfo();
                addElement("Domain", Environment.UserDomainName);                
                addElement("IPAddressing", (System.Net.Sockets.Socket.OSSupportsIPv4 ? "IPv4" : "") + " " + (System.Net.Sockets.Socket.OSSupportsIPv6 ? "IPv6" : ""));
                addElement("Local IPAddress", library.ReliableSocket.getNonLocalIPAddress(true).ToString());
            }
            catch {
                // ah, oh well..., probably don't have security clearance
            }
        }

        /// <summary>
        /// Gets a list of drives on the machine, and shows their size.
        /// This will automatically add the appropriate space in the system info dialog.
        /// </summary>
        /// <returns></returns>
        private void getCDriveInfo() {
            DriveInfo[] drives = DriveInfo.GetDrives();
            string driveInfo = null;
            foreach (DriveInfo drive in drives) {
                if (drive.IsReady == true) {
                    driveInfo = string.Format("{0}\\{1} GB",
                        (drive.TotalSize - drive.AvailableFreeSpace) / 1000000000, drive.TotalSize / 1000000000);
                    // 
                    this.gridRoot.RowDefinitions.Add(new RowDefinition());
                    addElement(drive.Name, driveInfo);
                    this.Height += 35.0;
                }
            }
        }

        /// <summary>
        /// Adds information to the window in a descending, sequential fashion.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="content"></param>
        private void addElement(string label, string content) {
            Label name = new Label();
            name.Content = label;
            name.FontWeight = FontWeights.Bold;
            name.SetValue(Grid.RowProperty, row);
            name.SetValue(Grid.ColumnProperty, 0);
            name.Foreground = Brushes.White;
            
            Label value = new Label();
            value.Content = content;
            value.SetValue(Grid.RowProperty, row++);
            value.SetValue(Grid.ColumnProperty, 1);
            value.Foreground = Brushes.White;

            this.gridRoot.Children.Add(name);
            this.gridRoot.Children.Add(value);
        }

        private int row = 0;

    }
}
