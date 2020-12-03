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

        private BalanceStateInfo _balanceState;

        public bool IsArchive
        {
            get
            {
                bool flag = false;
                this._feederDrives.TryGetValue(this._balanceState.Uid, out flag);
                return !flag;
            }
            set
            {
                this._feederDrives[this._balanceState.Uid] = false;
                this.OnChanged();
            }
        }

        public bool IsFeeder
        {
            get
            {
                bool flag = false;
                this._feederDrives.TryGetValue(this._balanceState.Uid, out flag);
                return flag;
            }
            set
            {
                this._feederDrives[this._balanceState.Uid] = true;
                this.OnChanged();
            }
        }

        public bool StoreUnduplicated
        {
            get
            {
                bool flag = true;
                this._unduplicatedDrives.TryGetValue(this._balanceState.Uid, out flag);
                return flag;
            }
            set
            {
                this._unduplicatedDrives[this._balanceState.Uid] = !StoreUnduplicated;
                this.OnChanged();
            }
        }

        public bool StoreDuplicated
        {
            get
            {
                bool flag = true;
                this._duplicatedDrives.TryGetValue(this._balanceState.Uid, out flag);
                return flag;
            }
            set
            {
                this._duplicatedDrives[this._balanceState.Uid] = !StoreDuplicated;
                this.OnChanged();
            }
        }

        public string Name
        {
            get
            {
                return this._balanceState.Name;
            }
        }

        public DriveState(BalanceStateInfo balanceState, Dictionary<string, bool> feederDrives, Dictionary<string, bool> unduplicatedDrives, Dictionary<string, bool> duplicatedDrives)
        {
            this._balanceState = balanceState;
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}