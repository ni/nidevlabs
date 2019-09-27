using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using NationalInstruments.Core;
using NationalInstruments.WebServiceRunner.Resources;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Manages the system tray icon for the web server
    /// </summary>
    internal class SystemTrayManager : Disposable
    {
        private TaskbarIcon _icon;

        public void InstallTrayIcon()
        {
            _icon = new TaskbarIcon();
            _icon.Visibility = System.Windows.Visibility.Visible;
            _icon.Icon = ServerResources.ApplicationIcon;
            _icon.TrayToolTip = new TextBlock() { Text = "LabVIEW NXG Web Service Server" };
            _icon.MenuActivation = PopupActivationMode.LeftOrRightClick;
            _icon.TrayMouseDoubleClick += OnSystemTrayMouseDoubleClick;
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            _icon?.Dispose();
            _icon = null;
        }

        private void OnSystemTrayMouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            ServerConfigurationDialog dlg = new ServerConfigurationDialog();
            dlg.ShowDialog();
        }
    }
}