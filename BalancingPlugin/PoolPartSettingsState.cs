using DrivePool.Integration.Info.Balancing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace AllInOnePlugin
{
    [Serializable]
    public class PoolPartSettingsState : INotifyPropertyChanged
    {
        [NonSerialized]
        private BalanceStateInfo _balanceStateInfo;

        [NonSerialized]
        private DriveState _driveState;

        [NonSerialized]
        private ObservableCollection<PoolPartSettingsState> _items;

        private string _uid;

        internal BalanceStateInfo BalanceStateInfo
        {
            get
            {
                return this._balanceStateInfo;
            }
            set
            {
                this._balanceStateInfo = value;
                this.OnChanged();
            }
        }

        public string Description
        {
            get
            {
                if (this.BalanceStateInfo == null)
                {
                    return "";
                }
                return string.Format("Total size: {0}, Free space: {1}", Str.ToHumanSize(this.BalanceStateInfo.TotalSize), Str.ToHumanSize(this.BalanceStateInfo.FreeSpace));
            }
        }

        internal DriveState DriveState
        {
            get
            {
                return this._driveState;
            }
            set
            {
                if (this._driveState != null)
                {
                    this._driveState.PropertyChanged -= new PropertyChangedEventHandler(this._driveState_PropertyChanged);
                }
                this._driveState = value;
                if (this._driveState != null)
                {
                    this._driveState.PropertyChanged += new PropertyChangedEventHandler(this._driveState_PropertyChanged);
                }
                this.OnChanged();
            }
        }

        public bool IsDownButtonEnabled
        {
            get
            {
                if (this.Items == null)
                {
                    return false;
                }
                return this.VisibleItems.IndexOf(this) < this.VisibleItems.Count - 1;
            }
        }

        public bool IsUpButtonEnabled
        {
            get
            {
                if (this.Items == null)
                {
                    return false;
                }
                return this.VisibleItems.IndexOf(this) > 0;
            }
        }

        public Visibility IsVisible
        {
            get
            {
                if (this._driveState != null && this._driveState.IsArchive)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        internal ObservableCollection<PoolPartSettingsState> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
                this.OnChanged();
            }
        }

        public string Name
        {
            get
            {
                if (this.BalanceStateInfo == null)
                {
                    return "?";
                }
                return this.BalanceStateInfo.Name;
            }
        }

        public string OrderText
        {
            get
            {
                if (this.Items == null)
                {
                    return "";
                }
                int num = this.VisibleItems.IndexOf(this);
                if (num < 0)
                {
                    return "?";
                }
                return (num + 1).ToString("#,0");
            }
        }

        internal string Uid
        {
            get
            {
                return this._uid;
            }
        }

        public double UsedSpaceRatio
        {
            get
            {
                if (this.BalanceStateInfo == null)
                {
                    return 0;
                }
                return (double)((float)this.BalanceStateInfo.UsedSpace) / (double)((float)this.BalanceStateInfo.TotalSize);
            }
        }

        public string UsedSpaceText
        {
            get
            {
                if (this.BalanceStateInfo == null)
                {
                    return "";
                }
                return string.Format("{0:0.0}% used space ({1})", this.UsedSpaceRatio * 100, Str.ToHumanSize(this.BalanceStateInfo.UsedSpace));
            }
        }

        private IList<PoolPartSettingsState> VisibleItems
        {
            get
            {
                return (
                    from pps in this.Items
                    where pps.IsVisible == Visibility.Visible
                    select pps).ToList<PoolPartSettingsState>();
            }
        }

        internal PoolPartSettingsState(BalanceStateInfo balanceStateInfo, ObservableCollection<PoolPartSettingsState> items, DriveState driveState)
        {
            this._balanceStateInfo = balanceStateInfo;
            this._items = items;
            this.DriveState = driveState;
            this._uid = balanceStateInfo.Uid;
        }

        private void _driveState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnChangedAllInstances();
        }

        public void MoveDown()
        {
            if (this.Items == null)
            {
                return;
            }
            int num = this.VisibleItems.IndexOf(this);
            if (num < 0 || num == this.VisibleItems.Count - 1)
            {
                return;
            }
            int num1 = this.Items.IndexOf(this);
            if (num1 < 0 || num1 == this.Items.Count - 1)
            {
                return;
            }
            int num2 = this.Items.IndexOf(this.VisibleItems[num + 1]);
            this.Items.Move(num1, num2);
            this.OnChangedAllInstances();
        }

        public void MoveTo(PoolPartSettingsState pp)
        {
            if (this.Items == null)
            {
                return;
            }
            int num = this.Items.IndexOf(this);
            if (num < 0 || num == this.Items.Count)
            {
                return;
            }
            int num1 = this.Items.IndexOf(pp);
            if (num1 < 0 || num1 == this.Items.Count)
            {
                return;
            }
            this.Items.Move(num, num1);
            this.OnChangedAllInstances();
        }

        public void MoveUp()
        {
            if (this.Items == null)
            {
                return;
            }
            int num = this.VisibleItems.IndexOf(this);
            if (num < 0 || num == 0)
            {
                return;
            }
            int num1 = this.Items.IndexOf(this);
            if (num1 < 0 || num1 == 0)
            {
                return;
            }
            int num2 = this.Items.IndexOf(this.VisibleItems[num - 1]);
            this.Items.Move(num1, num2);
            this.OnChangedAllInstances();
        }

        internal void OnChanged()
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

        private void OnChangedAllInstances()
        {
            if (this.Items == null)
            {
                return;
            }
            foreach (PoolPartSettingsState item in this.Items)
            {
                item.OnChanged();
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}