using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AllInOnePlugin
{
    /// <summary>
    /// This is the "View" class.
    /// 
    /// It is our one and only settings control that the user will use to interact with our balancer.
    /// </summary>
    public partial class SettingsUI : 
        UserControl,
        DrivePool.Integration.Balancing.UI.ISettingsControl
    {
        public SettingsUI()
        {
            InitializeComponent();

            // Initially, set to the default
            this.DataContext = SettingsState;
        }

        /// <summary>
        /// The balance states will be set by the system after instantiating our control.
        /// 
        /// We can make use of them in the UI. We don't need them here.
        /// </summary>
        public IEnumerable<DrivePool.Integration.Info.Balancing.BalanceStateInfo> BalanceStates { get; set; }

        /// <summary>
        /// Reset settings to defaults and update the UI.
        /// </summary>
        public void ResetSettingsStateToDefault()
        {
            // Force new settings
            _settingsState = null;

            // Link to UI
            this.DataContext = SettingsState;
            this._settingsState.BalanceStates = this.BalanceStates;
        }

        private BalancerSettingsState _settingsState;
        /// <summary>
        /// The system will get our settings and store them for us.
        /// 
        /// After instantiating our control it will set this to our saves settings state.
        /// </summary>
        public object SettingsState
        {
            get
            {
                // If we don't already have settings, or we have an unrecognized Version, create a default.
                if (_settingsState == null || _settingsState.Version != BalancerSettingsState.Default.Version)
                {
                    _settingsState = BalancerSettingsState.Default;
                }

                return _settingsState;
            }
            set
            {
                _settingsState = value as BalancerSettingsState;

                // Link the UI directly to our settings
                this.DataContext = SettingsState;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // We can access our BalanceStates here
            base.DataContext = this.SettingsState;
            this._settingsState.BalanceStates = this.BalanceStates;
        }


    }
}
