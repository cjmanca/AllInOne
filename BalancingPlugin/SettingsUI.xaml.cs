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
using DrivePool.Integration.Balancing.UI;
using DrivePool.Integration.Info.Balancing;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Markup;

namespace AllInOnePlugin
{
    /// <summary>
    /// This is the "View" class.
    /// 
    /// It is our one and only settings control that the user will use to interact with our balancer.
    /// </summary>
    public partial class SettingsUI : UserControl, ISettingsControl
    {
        private BalancerSettingsState _settingsState;

        public IEnumerable<BalanceStateInfo> BalanceStates
        {
            get;
            set;
        }

        public object SettingsState
        {
            get
            {
                if (this._settingsState == null || this._settingsState.Version != BalancerSettingsState.Default.Version)
                {
                    this._settingsState = BalancerSettingsState.Default;
                }
                return this._settingsState;
            }
            set
            {
                this._settingsState = value as BalancerSettingsState;
                base.DataContext = this.SettingsState;
            }
        }
        
        public SettingsUI()
        {
            this.InitializeComponent();
            base.DataContext = this.SettingsState;
        }
        

        private void ButtonDown_Click_1(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = e.Source as FrameworkElement;
            if (source == null)
            {
                return;
            }
            PoolPartSettingsState dataContext = source.DataContext as PoolPartSettingsState;
            if (dataContext == null)
            {
                return;
            }
            dataContext.MoveDown();
        }

        private void ButtonUp_Click_1(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = e.Source as FrameworkElement;
            if (source == null)
            {
                return;
            }
            PoolPartSettingsState dataContext = source.DataContext as PoolPartSettingsState;
            if (dataContext == null)
            {
                return;
            }
            dataContext.MoveUp();
        }
        
        private void ListBoxOrder_DragOver(object s, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            ListBoxItem listBoxItem = s as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }
            if (e.Data.GetData(typeof(SettingsUI.DragDropState)) as SettingsUI.DragDropState == null || listBoxItem.DataContext as PoolPartSettingsState == null)
            {
                return;
            }
            e.Effects = DragDropEffects.Move;
        }

        private void ListBoxOrder_Drop(object s, DragEventArgs e)
        {
            ListBoxItem listBoxItem = s as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }
            PoolPartSettingsState sourceData = (e.Data.GetData(typeof(SettingsUI.DragDropState)) as SettingsUI.DragDropState).SourceData;
            PoolPartSettingsState dataContext = listBoxItem.DataContext as PoolPartSettingsState;
            if (dataContext == null)
            {
                return;
            }
            if (sourceData == dataContext)
            {
                return;
            }
            sourceData.MoveTo(dataContext);
        }

        private void ListBoxOrder_MouseMove(object s, MouseEventArgs e)
        {
            if (e.OriginalSource is Button || e.OriginalSource is Image || e.OriginalSource is CheckBox)
            {
                return;
            }
            ListBoxItem listBoxItem = s as ListBoxItem;
            if (e.LeftButton != MouseButtonState.Pressed || listBoxItem == null)
            {
                return;
            }
            PoolPartSettingsState dataContext = listBoxItem.DataContext as PoolPartSettingsState;
            if (dataContext == null)
            {
                return;
            }
            SettingsUI.DragDropState dragDropState = new SettingsUI.DragDropState()
            {
                SourceData = dataContext,
                MousePosition = Mouse.GetPosition(this),
                SourceItem = listBoxItem
            };
            listBoxItem.Opacity = 0.75;
            Panel.SetZIndex(listBoxItem, 1);
            listBoxItem.IsHitTestVisible = false;
            try
            {
                DragDrop.DoDragDrop(listBoxItem, dragDropState, DragDropEffects.Move);
            }
            finally
            {
                listBoxItem.IsHitTestVisible = true;
                listBoxItem.Opacity = 1;
                listBoxItem.RenderTransform = null;
                Panel.SetZIndex(listBoxItem, 0);
            }
        }

        private void ListBoxOrder_PreviewDragOver(object s, DragEventArgs e)
        {
            ListBoxItem listBoxItem = s as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }
            SettingsUI.DragDropState data = e.Data.GetData(typeof(SettingsUI.DragDropState)) as SettingsUI.DragDropState;
            if (data == null || listBoxItem.DataContext as PoolPartSettingsState == null)
            {
                return;
            }
            Point position = e.GetPosition(this);
            double y = position.Y - data.MousePosition.Y;
            data.SourceItem.RenderTransform = new TranslateTransform(0, y);
        }

        private void ListBoxOrder_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            MouseWheelEventArgs mouseWheelEventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = e.Source
            };
            this.ListBoxUnduplicatedOrder.RaiseEvent(mouseWheelEventArg);
        }

        public void ResetSettingsStateToDefault()
        {
            this._settingsState = null;
            base.DataContext = this.SettingsState;
            this._settingsState.BalanceStates = this.BalanceStates;
        }
        
        /*
        [DebuggerNonUserCode]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 2:
                    {
                        EventSetter eventSetter = new EventSetter()
                        {
                            Event = UIElement.MouseMoveEvent,
                            Handler = new MouseEventHandler(this.ListBoxOrder_MouseMove)
                        };
                        ((Style)target).Setters.Add(eventSetter);
                        eventSetter = new EventSetter()
                        {
                            Event = UIElement.DropEvent,
                            Handler = new DragEventHandler(this.ListBoxOrder_Drop)
                        };
                        ((Style)target).Setters.Add(eventSetter);
                        eventSetter = new EventSetter()
                        {
                            Event = UIElement.DragOverEvent,
                            Handler = new DragEventHandler(this.ListBoxOrder_DragOver)
                        };
                        ((Style)target).Setters.Add(eventSetter);
                        eventSetter = new EventSetter()
                        {
                            Event = UIElement.PreviewDragOverEvent,
                            Handler = new DragEventHandler(this.ListBoxOrder_PreviewDragOver)
                        };
                        ((Style)target).Setters.Add(eventSetter);
                        return;
                    }
                case 3:
                    {
                        ((Button)target).Click += new RoutedEventHandler(this.ButtonUp_Click_1);
                        return;
                    }
                case 4:
                    {
                        ((Button)target).Click += new RoutedEventHandler(this.ButtonDown_Click_1);
                        return;
                    }
                default:
                    {
                        return;
                    }
            }
        }
        */

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            base.DataContext = this.SettingsState;
            this._settingsState.BalanceStates = this.BalanceStates;
            SetSort(_settingsState.SelectedSort);
        }

        private class DragDropState
        {
            public ListBoxItem SourceItem;

            public PoolPartSettingsState SourceData;

            public Point MousePosition;

            public DragDropState()
            {
            }
        }

        public void SetSort(string newSort, bool updateBox = true)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(DriveView.ItemsSource);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(newSort, ListSortDirection.Ascending);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
            
            if (updateBox)
            {
                for (int i = 0; i < SortComboBox.Items.Count; i++)
                {
                    SortComboBox.Text = SortComboBox.Items[i].ToString();
                    if (((SortComboBox.Items[i] as ComboBoxItem).Content).ToString() == newSort)
                    {
                        SortComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            //SetSort(_settingsState.SelectedSort);

            _settingsState.SelectedSort = (e.AddedItems[0] as ComboBoxItem).Content as string;
            SetSort(_settingsState.SelectedSort, false);
        }
    }
}
