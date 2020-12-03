using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AllInOnePlugin
{
    [Serializable]
    public class FillSettingsState : INotifyPropertyChanged
    {
        private double _fillRatio;

        private bool _isFillBytesChecked;

        internal ulong FillBytes;

        private FillSettingsState.ByteSizeMagnitudes FillBytesMagnitude
        {
            get
            {
                if (this.FillBytes < (long)1073741824)
                {
                    return FillSettingsState.ByteSizeMagnitudes.MB;
                }
                if (this.FillBytes < 1099511627776L)
                {
                    return FillSettingsState.ByteSizeMagnitudes.GB;
                }
                return FillSettingsState.ByteSizeMagnitudes.TB;
            }
        }

        public IEnumerable<string> FillBytesMagnitudes
        {
            get
            {
                return Enum.GetNames(typeof(FillSettingsState.ByteSizeMagnitudes));
            }
        }

        public string FillBytesSelectedMagnitude
        {
            get
            {
                return this.FillBytesMagnitude.ToString();
            }
            set
            {
                FillSettingsState.ByteSizeMagnitudes byteSizeMagnitude = (FillSettingsState.ByteSizeMagnitudes)Enum.Parse(typeof(FillSettingsState.ByteSizeMagnitudes), value);
                if (byteSizeMagnitude == this.FillBytesMagnitude)
                {
                    return;
                }
                switch (byteSizeMagnitude)
                {
                    case FillSettingsState.ByteSizeMagnitudes.MB:
                        {
                            this.FillBytes = (ulong)(this.FillBytesSliderValue * 1048576);
                            break;
                        }
                    case FillSettingsState.ByteSizeMagnitudes.GB:
                        {
                            this.FillBytes = (ulong)(this.FillBytesSliderValue * 1073741824);
                            break;
                        }
                    case FillSettingsState.ByteSizeMagnitudes.TB:
                        {
                            this.FillBytes = (ulong)(this.FillBytesSliderValue * 1099511627776);
                            break;
                        }
                }
                this.OnChanged();
            }
        }

        public double FillBytesSliderValue
        {
            get
            {
                switch (this.FillBytesMagnitude)
                {
                    case FillSettingsState.ByteSizeMagnitudes.MB:
                        {
                            return (double)((float)this.FillBytes) / 1048576;
                        }
                    case FillSettingsState.ByteSizeMagnitudes.GB:
                        {
                            return (double)((float)this.FillBytes) / 1073741824;
                        }
                    case FillSettingsState.ByteSizeMagnitudes.TB:
                        {
                            return (double)((float)this.FillBytes) / 1099511627776;
                        }
                }
                throw new InvalidOperationException("Unknown byte size magnitude.");
            }
            set
            {
                switch (this.FillBytesMagnitude)
                {
                    case FillSettingsState.ByteSizeMagnitudes.MB:
                        {
                            this.FillBytes = (ulong)(value * 1048576);
                            break;
                        }
                    case FillSettingsState.ByteSizeMagnitudes.GB:
                        {
                            this.FillBytes = (ulong)(value * 1073741824);
                            break;
                        }
                    case FillSettingsState.ByteSizeMagnitudes.TB:
                        {
                            this.FillBytes = (ulong)(value * 1099511627776);
                            break;
                        }
                }
                this.OnChanged();
            }
        }

        public string FillBytesText
        {
            get
            {
                switch (this.FillBytesMagnitude)
                {
                    case FillSettingsState.ByteSizeMagnitudes.MB:
                        {
                            return string.Format("{0}", (ulong)((double)((float)this.FillBytes) / 1048576));
                        }
                    case FillSettingsState.ByteSizeMagnitudes.GB:
                        {
                            return string.Format("{0}", (ulong)((double)((float)this.FillBytes) / 1073741824));
                        }
                    case FillSettingsState.ByteSizeMagnitudes.TB:
                        {
                            return string.Format("{0}", (ulong)((double)((float)this.FillBytes) / 1099511627776));
                        }
                }
                return "? free";
            }
        }

        public double FillRatio
        {
            get
            {
                return this._fillRatio;
            }
            set
            {
                this._fillRatio = value;
                this.OnChanged();
            }
        }

        public int FillRatioSliderValue
        {
            get
            {
                return (int)(this.FillRatio * 100);
            }
            set
            {
                this.FillRatio = (double)value / 100;
            }
        }

        public string FillRatioText
        {
            get
            {
                return string.Format("{0} %", (int)(this.FillRatio * 100));
            }
        }

        public bool IsFillBytesChecked
        {
            get
            {
                return this._isFillBytesChecked;
            }
            set
            {
                this._isFillBytesChecked = value;
                if (!this._isFillBytesChecked)
                {
                    this.FillBytes = (ulong)0;
                }
                else
                {
                    this.FillBytes = (ulong)10485760;
                }
                this.OnChanged();
            }
        }

        public FillSettingsState()
        {
        }

        internal void OnChanged()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(""));
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        private enum ByteSizeMagnitudes
        {
            MB,
            GB,
            TB
        }
    }
}