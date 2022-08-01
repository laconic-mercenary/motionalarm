namespace motionalarm
{

    using System.Windows;
    using Process = System.Diagnostics.Process;

    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hyperlinkAforge_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }

        private void hyperlinkIcons_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // fill in the label at the top
            this.labelLicense.Content =
                "The following is the license that\n" +
                "governs the use of this software.";
            // fill in with our resourced license
            this.textBoxCompanyLicense.Text = getLicense();
        }

        /// <summary>
        /// This is just crazy enough to work.  It's not that big of a license anyway.
        /// </summary>
        /// <returns></returns>
        private string getLicense()
        {
            return "Frontier Outpost LLC, End User License Agreement v1.0 (2011)\n\n" +

            "This license and the terms contained in it represent a legal agreement between you and Frontier Outpost.  This software is licensed to you if you accept the terms contained in this agreement.  Please read this agreement carefully.\n\n" +

            "IF YOU DO NOT ACCEPT ALL OF THE TERMS CONTAINED IN THIS AGREEMENT, DO NOT USE THE SOFTWARE.\n\n" +

            "In this agreement, software refers to the following (but is not limited to):\n" +
                "1) machine code / executable instructions\n" +
                "2) documentation - user manuals and guides\n" +
                "3) copies (partial or complete) of this software program.\n" +
                "4) upgrades to this software (unless a new license is distributed with the upgrade)\n\n" +
            "This software is property of Frontier Outpost.  This software is licensed, not sold.  You do not own or have any intellectual property rights with this software.\n\n" +

            "I.)  LICENSE\n" +
            "- You may install this software on an unlimited number of machines provided that the license key is not distributed and you ensure a copy of this agreement is contained with it.\n" +
            "- You may use this software if you obtained a license key from Frontier Outpost.  1 license key will allow the use of 1 instance of this software to be used on 1 machine.  \n" +
            "- All users that use the software after installation are also bound to the terms contained in this agreement.\n" +
            "- You may not modify, copy, distribute, reverse engineer (assemble or compile), create any derivative works of, or redistribute the software except as allowed by law or without approval, in writing, by Frontier Outpost.\n" +
            "- You may not use this software in a way that does not fully comply with all laws specified in the jurisdiction where you use the software.\n" +
            "- You may not charge fees for copies of and distributions of the software.\n" +
            "- Use of the software may continue as long as you comply with the terms contained in this agreement.\n" +
            "- Frontier Outpost may terminate this agreement if you fail to adhere to the terms contained in it.\n" +
            "- Upon termination of this agreement, you agree to destroy all copies of the software governed by this agreement.\n\n" +
            "II.) WARRANTY\n" +
            "- Frontier Outpost disclaims any warranty of any kind for the software, to the maximum extent allowed by law.\n" +
            "- The software is provided as is and has no warranty of any kind, either express or implied.  \n" +
            "- Some images and libraries the software uses are licensed under the Lesser GNU Public License v3.0, those libraries are completely independent of this agreement and have no affiliation with Frontier Outpost.  These will be listed in the software, generally under the HELP section.\n\n" +

            "III.) LIABILITY\n" +
            "- Frontier Outpost, its affiliates will not be liable for any damages of any kind that arise from the use of or the inability to use the software.\n\n" +

            "IV.) JURISDICTION\n" +
            "- This agreement will be governed by the laws of the United States of America, and the state of Nebraska.  The location of any judicial proceedings relating to this agreement will be held in the state courts of Lancaster County.\n";
        }
    }
}
