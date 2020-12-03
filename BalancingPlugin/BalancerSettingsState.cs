using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DrivePool.Integration.Info.Balancing;

namespace AllInOnePlugin
{
    /// <summary>
    /// This is the "Model" class.
    /// It exposes any settings that the balancer wishes to show to the user as properties.
    /// 
    /// This class will be automatically serialized for us by the system, so we must mark it [Serializable].
    /// </summary>
    [Serializable()]
    public class BalancerSettingsState
        : INotifyPropertyChanged
    {

        private int _version;
        /// <summary>
        /// The version ensures that we are not looking at an older or newer settings state.
        /// </summary>
        public int Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;

                // We must call OnChanged whenever something changes a property in order to notify the UI of the update.
                // If one property depends on another, this ensures a consistent UI experience.
                OnChanged();
            }
        }

        [NonSerialized]
        private IEnumerable<DriveState> _drives;

        [NonSerialized]
        private IEnumerable<BalanceStateInfo> _balanceStates;
        public IEnumerable<BalanceStateInfo> BalanceStates
        {
            get
            {
                return this._balanceStates;
            }
            set
            {
                this._balanceStates = value;
                this._drives = (
                        from bs in this._balanceStates
                        select new DriveState(bs, this.FeederDrives, this.UnduplicatedDrives, this.DuplicatedDrives)
                    ).ToArray<DriveState>();
                foreach (DriveState drive in this.Drives)
                {
                    drive.PropertyChanged += new PropertyChangedEventHandler(this.drive_PropertyChanged);
                }
                this._orderedPlacementSettings.BalanceStates = value;
                this._orderedPlacementSettings.DriveStates = this._drives;
                this.OnChanged();
            }
        }

        /// <summary>
        /// Our settings properties follow.
        /// </summary>
        #region Settings properties

        private bool _equalizeProtected;

        private bool _equalizeUnprotected;

        private bool _equalizeByPercent;

        private bool _equalizeByFreeSpace;

        private bool _freePlacement;
        private bool _orderedPlacement;
        private string _selectedSort;

        private FillSettingsState _ssdFillSettings;

        private FillSettingsState _archiveFillSettings;

        private OrderedPlacementSettingsState _orderedPlacementSettings;

        public string SelectedSort
        {
            get
            {
                return this._selectedSort;
            }
            set
            {
                this._selectedSort = value;
                this.OnChanged();
            }
        }

        public Dictionary<string, bool> FeederDrives { get; set; }

        public Dictionary<string, bool> UnduplicatedDrives { get; set; }

        public Dictionary<string, bool> DuplicatedDrives { get; set; }

        
        public FillSettingsState ArchiveFillSettings
        {
            get
            {
                return this._archiveFillSettings;
            }
        }
        public IEnumerable<DriveState> Drives
        {
            get
            {
                if (this._drives == null)
                {
                    this._drives = new DriveState[0];
                }
                return this._drives;
            }
        }
        
        public bool OrderedPlacement
        {
            get
            {
                if (this._drives == null)
                {
                    return false;
                }
                return this._orderedPlacement && this._drives.Any<DriveState>((DriveState d) => d.IsFeeder);
            }
            set
            {
                this._orderedPlacement = value;
                this.OnChanged();
            }
        }
        public bool FreePlacement
        {
            get
            {
                return this._freePlacement || (this._orderedPlacement && !OrderedPlacement);
            }
            set
            {
                this._freePlacement = value;
                this.OnChanged();
            }
        }
        public bool EqualizeByFreeSpace
        {
            get
            {
                return this._equalizeByFreeSpace;
            }
            set
            {
                this._equalizeByFreeSpace = value;
                this.OnChanged();
            }
        }

        public bool EqualizeByPercent
        {
            get
            {
                return this._equalizeByPercent;
            }
            set
            {
                this._equalizeByPercent = value;
                this.OnChanged();
            }
        }

        public bool EqualizeProtected
        {
            get
            {
                return this._equalizeProtected;
            }
            set
            {
                this._equalizeProtected = value;
                this.OnChanged();
            }
        }

        public bool EqualizeUnprotected
        {
            get
            {
                return this._equalizeUnprotected;
            }
            set
            {
                this._equalizeUnprotected = value;
                this.OnChanged();
            }
        }
        // TODO: Add more settings properties here...

        public OrderedPlacementSettingsState OrderedPlacementSettings
        {
            get
            {
                return this._orderedPlacementSettings;
            }
        }

        public FillSettingsState SsdFillSettings
        {
            get
            {
                return this._ssdFillSettings;
            }
        }

        #endregion

        /// <summary>
        /// This returns a new default settings instance.
        /// </summary>
        public static BalancerSettingsState Default
        {
            get
            {
                BalancerSettingsState balancerSettingsState = new BalancerSettingsState()
                {
                    Version = 7,
                    EqualizeProtected = true,
                    EqualizeUnprotected = true,
                    EqualizeByFreeSpace = true,
                    EqualizeByPercent = false,
                    FreePlacement = false,
                    FeederDrives = new Dictionary<string, bool>(),
                    UnduplicatedDrives = new Dictionary<string, bool>(),
                    DuplicatedDrives = new Dictionary<string, bool>(),
                    _ssdFillSettings = new FillSettingsState()
                    {
                        FillRatio = 0.75,
                        IsFillBytesChecked = false
                    },
                    _archiveFillSettings = new FillSettingsState()
                    {
                        FillRatio = 0.9,
                        IsFillBytesChecked = true,
                        FillBytes = (ulong)10485760
                    },
                    _orderedPlacementSettings = new OrderedPlacementSettingsState()
                    {
                        DuplicatedOrderSameAsUnduplicated = true,
                        IsMoveExistingChecked = true
                    },
                    SelectedSort = "Name"
                };
                return balancerSettingsState;
            }
        }

        /// <summary>
        /// This event informs the UI of property changes.
        /// It must not be serialized.
        /// </summary>
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        private void drive_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnChanged();
        }

        /// <summary>
        /// Go through all the properties and update the UI.
        /// </summary>
        private void OnChanged()
        {
            if (PropertyChanged != null)
            {
                var props = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var prop in props)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop.Name));
                }
            }
        }

    }
}
