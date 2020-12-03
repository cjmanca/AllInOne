using DrivePool.Integration.Info;
using DrivePool.Integration.Info.Balancing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace AllInOnePlugin
{
    [Serializable]
    public class OrderedPlacementSettingsState : INotifyPropertyChanged
    {
        [NonSerialized]
        private IEnumerable<BalanceStateInfo> _balanceStates;

        [NonSerialized]
        private IEnumerable<DriveState> _driveStates;

        private bool _unduplicatedOrderEnabled;

        private bool _duplicatedOrderEnabled;

        private bool _duplicatedOrderSameAsUnduplicated;

        private bool _moveExistingFiles;

        internal PoolPartSettingsState[] UnduplicatedOrder;

        internal PoolPartSettingsState[] DuplicatedOrder;

        [NonSerialized]
        private ObservableCollection<PoolPartSettingsState> _unduplicatedOrderList;

        [NonSerialized]
        private ObservableCollection<PoolPartSettingsState> _duplicatedOrderList;

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

        public IEnumerable<DriveState> DriveStates
        {
            get
            {
                return this._driveStates;
            }
            set
            {
                this._driveStates = value;
                this.OnChanged();
            }
        }

        public bool DuplicatedOrderEnabled
        {
            get
            {
                return this._duplicatedOrderEnabled;
            }
            set
            {
                this._duplicatedOrderEnabled = value;
                this.OnChanged();
            }
        }

        public Visibility DuplicatedOrderEnabledVisibility
        {
            get
            {
                if (!this.DuplicatedOrderEnabled)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public ObservableCollection<PoolPartSettingsState> DuplicatedOrderList
        {
            get
            {
                if (this._duplicatedOrderList == null && this.BalanceStates != null && this.DriveStates != null)
                {
                    this.CreateObservableOrder(ref this._duplicatedOrderList, ref this.DuplicatedOrder);
                    this._duplicatedOrderList.CollectionChanged += new NotifyCollectionChangedEventHandler(this._duplicatedOrderList_CollectionChanged);
                }
                return this._duplicatedOrderList;
            }
        }

        public bool DuplicatedOrderSameAsUnduplicated
        {
            get
            {
                if (!this._duplicatedOrderSameAsUnduplicated)
                {
                    return false;
                }
                return this.UnduplicatedOrderEnabled;
            }
            set
            {
                this._duplicatedOrderSameAsUnduplicated = value;
                this.OnChanged();
            }
        }

        public bool DuplicatedOrderSameAsUnduplicatedEnabled
        {
            get
            {
                if (!this.UnduplicatedOrderEnabled)
                {
                    return false;
                }
                return this.DuplicatedOrderEnabled;
            }
        }

        public Visibility DuplicatedOrderVisibility
        {
            get
            {
                if (this.DuplicatedOrderEnabled && !this.DuplicatedOrderSameAsUnduplicated)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public bool IsMoveExistingChecked
        {
            get
            {
                return this._moveExistingFiles;
            }
            set
            {
                if (value)
                {
                    this._moveExistingFiles = true;
                    this.OnChanged();
                }
            }
        }

        public bool IsNotMoveExistingChecked
        {
            get
            {
                return !this._moveExistingFiles;
            }
            set
            {
                if (value)
                {
                    this._moveExistingFiles = false;
                    this.OnChanged();
                }
            }
        }

        public bool OrderEnabled
        {
            get
            {
                if (this.DuplicatedOrderEnabled)
                {
                    return true;
                }
                return this.UnduplicatedOrderEnabled;
            }
        }

        public Visibility OrderEnabledVisibility
        {
            get
            {
                if (!this.OrderEnabled)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public bool UnduplicatedOrderEnabled
        {
            get
            {
                return this._unduplicatedOrderEnabled;
            }
            set
            {
                this._unduplicatedOrderEnabled = value;
                if (value)
                {
                    if (!this._duplicatedOrderEnabled)
                    {
                        this._duplicatedOrderEnabled = true;
                        this._duplicatedOrderSameAsUnduplicated = true;
                    }
                }
                else if (this._duplicatedOrderEnabled && this._duplicatedOrderSameAsUnduplicated)
                {
                    this._duplicatedOrderEnabled = false;
                }
                this.OnChanged();
            }
        }

        public Visibility UnduplicatedOrderEnabledVisibility
        {
            get
            {
                if (!this.UnduplicatedOrderEnabled)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public ObservableCollection<PoolPartSettingsState> UnduplicatedOrderList
        {
            get
            {
                if (this._unduplicatedOrderList == null && this.BalanceStates != null && this.DriveStates != null)
                {
                    this.CreateObservableOrder(ref this._unduplicatedOrderList, ref this.UnduplicatedOrder);
                    this._unduplicatedOrderList.CollectionChanged += new NotifyCollectionChangedEventHandler(this._unduplicatedOrderList_CollectionChanged);
                }
                return this._unduplicatedOrderList;
            }
        }

        public OrderedPlacementSettingsState()
        {
        }

        private void _duplicatedOrderList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.DuplicatedOrder = this._duplicatedOrderList.ToArray<PoolPartSettingsState>();
        }

        private void _unduplicatedOrderList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UnduplicatedOrder = this._unduplicatedOrderList.ToArray<PoolPartSettingsState>();
        }

        private void CreateObservableOrder(ref ObservableCollection<PoolPartSettingsState> orderList, ref PoolPartSettingsState[] order)
        {
            orderList = new ObservableCollection<PoolPartSettingsState>();
            if (order != null)
            {
                PoolPartSettingsState[] poolPartSettingsStateArray = order;
                for (int i = 0; i < (int)poolPartSettingsStateArray.Length; i++)
                {
                    PoolPartSettingsState poolPartSettingsState1 = poolPartSettingsStateArray[i];
                    BalanceStateInfo balanceStateInfo1 = (
                        from bs2 in this.BalanceStates
                        where bs2.Uid == poolPartSettingsState1.Uid
                        select bs2).FirstOrDefault<BalanceStateInfo>();
                    if (balanceStateInfo1 != null)
                    {
                        poolPartSettingsState1.BalanceStateInfo = balanceStateInfo1;
                        poolPartSettingsState1.Items = orderList;
                        poolPartSettingsState1.DriveState = this.DriveStates.SingleOrDefault<DriveState>((DriveState ds) => ds.BalanceState == balanceStateInfo1);
                        orderList.Add(poolPartSettingsState1);
                    }
                }
            }
            if (!this.BalanceStates.Any<BalanceStateInfo>() || !(this.BalanceStates.First<BalanceStateInfo>().PoolParts.First<PoolPartInfo>().GetType().GetProperty("FirstSeen") != null))
            {
                foreach (BalanceStateInfo balanceStateInfo2 in
                    from bs in this.BalanceStates
                    orderby (double)((float)bs.UsedSpace) / (double)((float)bs.TotalSize) descending
                    select bs)
                {
                    if (!orderList.Any<PoolPartSettingsState>((PoolPartSettingsState pp2) => pp2.Uid == balanceStateInfo2.Uid))
                    {
                        PoolPartSettingsState poolPartSettingsState2 = new PoolPartSettingsState(balanceStateInfo2, orderList, this.DriveStates.SingleOrDefault<DriveState>((DriveState ds) => ds.BalanceState == balanceStateInfo2));
                        orderList.Add(poolPartSettingsState2);
                    }
                }
            }
            else
            {
                using (IEnumerator<BalanceStateInfo> enumerator = (from bs in this.BalanceStates
                                                                   orderby bs.UsedSpace / bs.TotalSize descending
                                                                   select bs).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        BalanceStateInfo bs = enumerator.Current;
                        if (!orderList.Any((PoolPartSettingsState pp2) => pp2.Uid == bs.Uid))
                        {
                            PoolPartSettingsState item = new PoolPartSettingsState(bs, orderList, this.DriveStates.SingleOrDefault((DriveState ds) => ds.BalanceState == bs));
                            orderList.Add(item);
                        }
                    }
                }
            }
            order = orderList.ToArray<PoolPartSettingsState>();
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
    }
}