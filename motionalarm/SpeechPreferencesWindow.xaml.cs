using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Windows;
using app.motionalarm;

namespace motionalarm
{
    /// <summary>
    /// Interaction logic for SpeechPreferencesWindow.xaml
    /// </summary>
    public partial class SpeechPreferencesWindow : Window
    {
        public SpeechPreferencesWindow(SpeechSynthesizer synth)
        {
            InitializeComponent();
            this.synthReference = synth;
            loadVoices();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // see if there are items to select, if there are, select the first
            if (this.comboBoxVoices.Items.Count != 0)
            {
                this.comboBoxVoices.SelectedIndex = 0;
            }
            // initially enable speech
            this.checkBoxEnableSpeech.IsChecked = true;
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBoxVoices.SelectedItem != null)
            {
                _voiceInfo = this.comboBoxVoices.SelectedItem as string;
                _enableSpeech = this.checkBoxEnableSpeech.IsChecked == true;
            }
            this.DialogResult = true;
        }

        void loadVoices()
        {
            if (synthReference == null)
            {
                Globals.MessageBox.show("No speech synthesizer detected!", "Speech Synthesis Error", true);
                this.DialogResult = false;
            }
            else
            {
                this.comboBoxVoices.Items.Clear();
                ReadOnlyCollection<InstalledVoice> voices = this.synthReference.GetInstalledVoices();
                if (voices.Count != 0)
                {
                    foreach (InstalledVoice voice in voices)
                    {
                        this.comboBoxVoices.Items.Add(voice.VoiceInfo.Name);
                    }
                }
                else
                {
                    Globals.MessageBox.show("There are no installed voices.", "Speech Synthesis Error", true);
                    this.DialogResult = false;
                }
            }
        }

        private SpeechSynthesizer synthReference = null;

        public string voiceInfo
        {
            get
            {
                return _voiceInfo;
            }
        }
        private string _voiceInfo = null;

        public bool enableSpeech
        {
            get
            {
                return _enableSpeech;
            }
        }
        private bool _enableSpeech = true;

    }
}
