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


        public Dictionary<string, bool> FeederDrives { get; set; }

        public Dictionary<string, bool> UnduplicatedDrives { get; set; }

        public Dictionary<string, bool> DuplicatedDrives { get; set; }


        private double _feederFillRatio;

        private double _archiveFillRatio;

        public double ArchiveFillRatio
        {
            get
            {
                return this._archiveFillRatio;
            }
            set
            {
                this._archiveFillRatio = value;
                this.OnChanged();
            }
        }

        public int ArchiveFillRatioSliderValue
        {
            get
            {
                return (int)(this.ArchiveFillRatio * 100);
            }
            set
            {
                this.ArchiveFillRatio = (double)value / 100;
            }
        }

        public string ArchiveFillRatioText
        {
            get
            {
                return string.Format("{0} %", (int)(this.ArchiveFillRatio * 100));
            }
        }

        public IEnumerable<DriveState> Drives
        {
            get
            {
                if (this._balanceStates == null)
                {
                    return new DriveState[0];
                }
                return 
                    (
                        from bs in this._balanceStates
                        select new DriveState(bs, this.FeederDrives, this.UnduplicatedDrives, this.DuplicatedDrives)
                    ).ToArray<DriveState>();
            }
        }

        public double FeederFillRatio
        {
            get
            {
                return this._feederFillRatio;
            }
            set
            {
                this._feederFillRatio = value;
                this.OnChanged();
            }
        }

        public int FeederFillRatioSliderValue
        {
            get
            {
                return (int)(this.FeederFillRatio * 100);
            }
            set
            {
                this.FeederFillRatio = (double)value / 100;
            }
        }

        public string FeederFillRatioText
        {
            get
            {
                return string.Format("{0} %", (int)(this.FeederFillRatio * 100));
            }
        }

        public bool OrderedPlacement
        {
            get
            {
                return this._orderedPlacement;
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
                return this._freePlacement;
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
                    Version = 4,
                    FeederFillRatio = 0.75,
                    ArchiveFillRatio = 0.9,
                    EqualizeProtected = true,
                    EqualizeUnprotected = true,
                    EqualizeByFreeSpace = true,
                    EqualizeByPercent = false,
                    FreePlacement = false,
                    FeederDrives = new Dictionary<string, bool>(),
                    UnduplicatedDrives = new Dictionary<string, bool>(),
                    DuplicatedDrives = new Dictionary<string, bool>()
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
