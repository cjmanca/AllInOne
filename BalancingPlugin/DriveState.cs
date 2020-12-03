using DrivePool.Integration.Info.Balancing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace AllInOnePlugin
{
    public class DriveState : INotifyPropertyChanged
    {
        private Dictionary<string, bool> _feederDrives;
        private Dictionary<string, bool> _unduplicatedDrives;
        private Dictionary<string, bool> _duplicatedDrives;

        internal BalanceStateInfo BalanceState;

        public bool IsArchive
        {
            get
            {
                bool flag = false;
                if (!this._feederDrives.TryGetValue(this.BalanceState.Uid, out flag))
                {
                    flag = false;
                }
                return !flag;
            }
            set
            {
                this._feederDrives[this.BalanceState.Uid] = !value;
                this.OnChanged();
            }
        }

        public bool IsFeeder
        {
            get
            {
                bool flag = false;
                if (!this._feederDrives.TryGetValue(this.BalanceState.Uid, out flag))
                {
                    flag = false;
                }
                return flag;
            }
            set
            {
                this._feederDrives[this.BalanceState.Uid] = value;
                this.OnChanged();
            }
        }
        public bool IsSsd
        {
            get
            {
                return IsFeeder;
            }
            set
            {
                IsFeeder = value;
            }
        }
        

        public bool StoreUnduplicated
        {
            get
            {
                bool flag = true;
                if (!this._unduplicatedDrives.TryGetValue(this.BalanceState.Uid, out flag))
                {
                    flag = true;
                }
                return flag;
            }
            set
            {
                this._unduplicatedDrives[this.BalanceState.Uid] = !StoreUnduplicated;
                this.OnChanged();
            }
        }

        public bool StoreDuplicated
        {
            get
            {
                bool flag = true;
                if (!this._duplicatedDrives.TryGetValue(this.BalanceState.Uid, out flag))
                {
                    flag = true;
                }
                return flag;
            }
            set
            {
                this._duplicatedDrives[this.BalanceState.Uid] = !StoreDuplicated;
                this.OnChanged();
            }
        }

        public string Name
        {
            get
            {
                return this.BalanceState.Name;
            }
        }

        public int DiskId
        {
            get
            {
                return this.BalanceState.DiskId;
            }
        }
        public ulong FreeSpace
        {
            get
            {
                return this.BalanceState.FreeSpace;
            }
        }
        public ulong PooledFiles
        {
            get
            {
                return this.BalanceState.PooledFiles;
            }
        }
        public ulong TotalSize
        {
            get
            {
                return this.BalanceState.TotalSize;
            }
        }
        public ulong UsedSpace
        {
            get
            {
                return this.BalanceState.UsedSpace;
            }
        }

        public DriveState(BalanceStateInfo balanceState, Dictionary<string, bool> feederDrives, Dictionary<string, bool> unduplicatedDrives, Dictionary<string, bool> duplicatedDrives)
        {
            this.BalanceState = balanceState;
            this._feederDrives = feederDrives;
            this._unduplicatedDrives = unduplicatedDrives;
            this._duplicatedDrives = duplicatedDrives;
        }

        private void OnChanged()
        {
            if (this.PropertyChanged != null)
            {
                PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < (int)properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyInfo.Name));
                }
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}