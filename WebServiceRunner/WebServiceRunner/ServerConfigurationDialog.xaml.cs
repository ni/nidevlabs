using NationalInstruments.Core;
using System.Globalization;
using System.Windows;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Dialog which allows the end use to configure web service server preferences.
    /// </summary>
    public partial class ServerConfigurationDialog : Window
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        public ServerConfigurationDialog()
        {
            InitializeComponent();
            InitializeControls();
        }

        /// <summary>
        /// Called to initialize the controls of the page based on the current state of the user preferences
        /// </summary>
        public void InitializeControls()
        {
            _localHostOnlyCtrl.IsChecked = WebServiceServer.LocalHostOnly;
            _allowAnyConnectionsCtrl.IsChecked = !WebServiceServer.LocalHostOnly;
            _useStaticPortCtrl.IsChecked = WebServiceServer.UseStaticPort;
            _useRandomPortCtrl.IsChecked = !WebServiceServer.UseStaticPort;
            _portNumberCtrl.Text = WebServiceServer.StaticPort.ToString(CultureInfo.InvariantCulture);
            _gllLocationTextBox.Text = WebServiceRunner.GllLocation;
        }

        /// <summary>
        /// Called to commit any preference changes
        /// </summary>
        /// <returns>True if a preference changed</returns>
        public bool CommitChanges()
        {
            bool preferenceChanged = false;
            if (_localHostOnlyCtrl.IsChecked.Value != WebServiceServer.LocalHostOnly)
            {
                WebServiceServer.LocalHostOnly = _localHostOnlyCtrl.IsChecked.Value;
                preferenceChanged = true;
            }
            if (_useStaticPortCtrl.IsChecked.Value != WebServiceServer.UseStaticPort)
            {
                WebServiceServer.UseStaticPort = _useStaticPortCtrl.IsChecked.Value;
                preferenceChanged = true;
            }
            int newPort = WebServiceServer.StaticPort;
            if (int.TryParse(_portNumberCtrl.Text, out newPort))
            {
                if (WebServiceServer.StaticPort != newPort)
                {
                    WebServiceServer.StaticPort = newPort;
                    preferenceChanged = true;
                }
            }
            if (_gllLocationTextBox.Text != WebServiceRunner.GllLocation)
            {
                WebServiceRunner.GllLocation = _gllLocationTextBox.Text;
                preferenceChanged = true;
            }
            return preferenceChanged;
        }

        private void HandleRandomPortCtrlChecked(object sender, RoutedEventArgs e)
        {
            _staticPortContent.IsEnabled = false;
        }

        private void HandleStaticPortCtrlChecked(object sender, RoutedEventArgs e)
        {
            _staticPortContent.IsEnabled = true;
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            CommitChanges();
            var result = NIMessageBox.Show("Restart server to apply changes?", "Restart Server", NIMessageBoxButton.YesNo);
            Close();
            if (result == NIMessageBoxResult.Yes)
            {
                WebServiceRunner.RestartServerOnClose = true;
                Application.Current.Shutdown();
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnShutdownServer(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }
    }
}