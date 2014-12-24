﻿using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Catrobat.IDE.Core.Models;
using Catrobat.IDE.Core.Models.Bricks;
using Catrobat.IDE.Core.Models.Scripts;
using Catrobat.IDE.Core.UI.PortableUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;


namespace Catrobat.IDE.WindowsPhone.Controls.ListsViewControls
{
    class CatrobatListView : UserControl
    {
        public CatrobatListViewWorker Clvw
        {
            get;
            private set;
        }

        public INotifyCollectionChanged ItemsSource
        {
            get { return (INotifyCollectionChanged)GetValue(ItemsSourceDP); }
            set { SetValue(ItemsSourceDP, value); }
        }

        public static readonly DependencyProperty ItemsSourceDP = DependencyProperty.Register(
            "ItemsSource", typeof(object), typeof(CatrobatListView),
            new PropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateDP); }
            set { SetValue(ItemTemplateDP, value); }
        }

        public static readonly DependencyProperty ItemTemplateDP = DependencyProperty.Register(
            "ItemTemplate", typeof(object), typeof(CatrobatListView),
            new PropertyMetadata(null, ItemTemplateChanged));

        private static void ItemTemplateChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((CatrobatListView)target).OnItemTemplateChanged(e.NewValue);
        }
        private void OnItemTemplateChanged(object e)
        {
            var templateSelector = e as DataTemplate;
            if (templateSelector == null)
            {
                return;
            }
            Clvw.UpdateItemTemplateSelector(templateSelector);
        }

        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleDP); }
            set { SetValue(ItemContainerStyleDP, value); }
        }

        public static readonly DependencyProperty ItemContainerStyleDP = DependencyProperty.Register(
            "ItemContainerStyle", typeof(object), typeof(CatrobatListView),
            new PropertyMetadata(null, ItemContainerStyleChanged));

        private static void ItemContainerStyleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((CatrobatListView)target).UpdateItemContainerStyle(e.NewValue);
        }

        private void UpdateItemContainerStyle(object e)
        {
            var style = e as Style;
            if (style == null)
            {
                return;
            }
            Clvw.ItemContainerStyle = style;
        }

        private bool _selectionEnabled;

        public bool SelectionEnabled
        {
            get { return _selectionEnabled; }
            set
            {
                _selectionEnabled = value;
                Clvw.SetSelectionMode(_selectionEnabled);
                SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
                ((IList)SelectedItems).Clear();
                SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            }
        }

        public static readonly DependencyProperty ItemWidthPortraitDP =
          DependencyProperty.Register("ItemWidthPortrait", typeof(int), typeof(CatrobatListView), new PropertyMetadata(380));

        public int ItemWidthPortrait
        {
            get { return (int)GetValue(ItemWidthPortraitDP); }
            set { SetValue(ItemWidthPortraitDP, value); }
        }
        public static readonly DependencyProperty ItemWidthLandscapeDP =
          DependencyProperty.Register("ItemWidthLandscape", typeof(int), typeof(CatrobatListView), new PropertyMetadata(450));

        public int ItemWidthLandscape
        {
            get { return (int)GetValue(ItemWidthLandscapeDP); }
            set { SetValue(ItemWidthLandscapeDP, value); }
        }




        public static readonly DependencyProperty VerticalItemMarginDP =
          DependencyProperty.Register("VerticalItemMargin", typeof(int), typeof(CatrobatListView), new PropertyMetadata(0));

        public int VerticalItemMargin
        {
            get { return (int)GetValue(VerticalItemMarginDP); }
            set { SetValue(VerticalItemMarginDP, value); Clvw.UpdateItemMargin(value); }
        }

        public static readonly DependencyProperty ReorderEnabledDP =
          DependencyProperty.Register("ReorderEnabled", typeof(int), typeof(CatrobatListView), new PropertyMetadata(false));

        public bool ReorderEnabled
        {
            get { return (bool)GetValue(ReorderEnabledDP); }
            set { SetValue(ReorderEnabledDP, value); Clvw.SetReorderEnabled(value); }
        }

        public static readonly DependencyProperty GroupingEnabledDP =
          DependencyProperty.Register("GroupingEnabled", typeof(int), typeof(CatrobatListView), new PropertyMetadata(true));

        public bool GroupingEnabled
        {
            get { return (bool)GetValue(GroupingEnabledDP); }
            set { SetValue(GroupingEnabledDP, value); Clvw.SetGroupingEnabled(value); }
        }



        public INotifyCollectionChanged SelectedItems
        {
            get { return (INotifyCollectionChanged)GetValue(SelectedItemsDP); }
            set { SetValue(SelectedItemsDP, value); }
        }

        public static readonly DependencyProperty SelectedItemsDP = DependencyProperty.Register(
            "SelectedItems", typeof(object), typeof(CatrobatListView),
            new PropertyMetadata(default(ObservableCollection<object>), SelectedItemsChanged));

        private static void SelectedItemsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ((CatrobatListView)target).UpdateSelectedItems();
        }

        private void UpdateSelectedItems()
        {
            if (SelectedItems as IList != null)
            {
                (SelectedItems as IList).Clear();
            }
            SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
            this.Clvw.SmartSelectedItems.CollectionChanged -= SmartSelectedItems_CollectionChanged;
            Clvw.UpdateSelectedItems(this.SelectedItems as IList);
            this.Clvw.SmartSelectedItems.CollectionChanged += SmartSelectedItems_CollectionChanged;
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Clvw.SmartSelectedItems.CollectionChanged -= SmartSelectedItems_CollectionChanged;
            Clvw.UpdateSelectedItems(this.SelectedItems as IList);
            this.Clvw.SmartSelectedItems.CollectionChanged += SmartSelectedItems_CollectionChanged;
        }

        public delegate void ItemTappedEventHandler(object sender, CatrobatListViewItemEventArgs e);

        public event ItemTappedEventHandler ItemTapped;

        public CatrobatListView()
        {
            this.Clvw = new CatrobatListViewWorker();
            this.Content = this.Clvw;
            this.Clvw.ItemDragCompletedEvent += clvw_ItemDragCompletedEvent;
            this.Clvw.ItemTapped += Clvw_ItemTapped;
            this.Clvw.SmartSelectedItems.CollectionChanged += SmartSelectedItems_CollectionChanged;
            this.Unloaded += CatrobatListView_Unloaded;
            this.SizeChanged += CatrobatListView_SizeChanged;
            this.Loaded += CatrobatListView_Loaded;
        }

        void SmartSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
            for (int i = 0; i < Clvw.SmartSelectedItems.Count; i++)
            {
                if ((SelectedItems as IList).Contains(Clvw.SmartSelectedItems[i]) == false)
                {
                    (SelectedItems as IList).Add(Clvw.SmartSelectedItems[i]);
                }
            }
            for (int i = (SelectedItems as IList).Count - 1; i >= 0; i--)
            {
                if (Clvw.SmartSelectedItems.Contains((SelectedItems as IList)[i]) == false)
                {
                    (SelectedItems as IList).RemoveAt(i);
                }
            }
            SortSelectedItems();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SortSelectedItems()
        {
            for (int i = 0; i < (SelectedItems as IList).Count - 1; )
            {
                if ((ItemsSource as IList).IndexOf((SelectedItems as IList)[i + 1]) <
                    (ItemsSource as IList).IndexOf((SelectedItems as IList)[i]))
                {
                    var tmp = (SelectedItems as IList)[i];
                    (SelectedItems as IList)[i] = (SelectedItems as IList)[i + 1];
                    (SelectedItems as IList)[i + 1] = tmp;
                    if (i > 0)
                    {
                        i--;
                    }
                }
                else
                {
                    i++;
                }
            }
        }

        void CatrobatListView_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Clvw.ImportItemsSource((IList)ItemsSource);
                ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;
                Clvw.LayoutUpdated += Clvw_LayoutUpdated;
            });
        }

        void Clvw_LayoutUpdated(object sender, object e)
        {
            if (Clvw.SelectionEnabled == false)
            {
                Clvw.CheckIfNewAddedBrick();
            }
            Clvw.LayoutUpdated -= Clvw_LayoutUpdated;
            Clvw.SetProgessRingVisibility(Visibility.Collapsed);
        }

        void Clvw_ItemTapped(object sender, CatrobatListViewItemEventArgs e)
        {
            if (ItemTapped != null)
            {
                ItemTapped(sender, e);
            }
        }

        void CatrobatListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clvw.SetItemWidth(ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Portrait
                ? ItemWidthPortrait
                : ItemWidthLandscape);
        }

        void CatrobatListView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ItemsSource != null)
            {
                this.ItemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
            }
            if (SelectedItems != null)
            {
                this.SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
            }
            this.Clvw.ItemDragCompletedEvent -= clvw_ItemDragCompletedEvent;
            this.Clvw.SmartSelectedItems.CollectionChanged -= SmartSelectedItems_CollectionChanged;
            this.Clvw.ItemTapped -= Clvw_ItemTapped;

            GC.Collect();
        }

        void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                return;
            }
            Clvw.ImportItemsSource(ItemsSource as IList);
        }

        void clvw_ItemDragCompletedEvent(object sender, CatrobatListViewEventArgs e)
        {
            this.ItemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
            DragTransfer(e.GetTmpControl(), e.GetOrignalContent(), e.GetGroupedItems());
            this.ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;
        }

        private void DragTransfer(CatrobatListViewEmptyDummyControl tmpControl, CatrobatListViewDragObject originalContent, IList<object> groupedItems)
        {
            int actSourceIndex = Clvw.Items.IndexOf(tmpControl);
            int actTargetIndex = ((IList)this.ItemsSource).IndexOf(originalContent.Content);

            if (actTargetIndex != -1 && actSourceIndex != actTargetIndex && (groupedItems == null || groupedItems.Count == 0))
            {
                ((IList)this.ItemsSource).RemoveAt(actTargetIndex);
                ((IList)this.ItemsSource).Insert(actSourceIndex, originalContent.Content);
            }
            else if (actTargetIndex != -1 && groupedItems != null)
            {
                ((IList)this.ItemsSource).RemoveAt(actTargetIndex);
                for (int i = 0; i < groupedItems.Count; i++)
                {
                    ((IList)this.ItemsSource).Remove(groupedItems[i]);
                }
                ((IList)this.ItemsSource).Insert(actSourceIndex, originalContent.Content);
                for (int i = 0; i < groupedItems.Count; i++)
                {
                    ((IList)this.ItemsSource).Insert(actSourceIndex + 1, groupedItems[i]);
                    Clvw.Items.Insert(actSourceIndex + 1, groupedItems[i]);
                }
            }

            Clvw.Items[actSourceIndex] = originalContent.Content;

        }
    }


    public class CatrobatListViewWorker : ListViewBase
    {
        private const string DragCanvasName = "DragCanvas";
        private Canvas _dragCanvas;

        private const string ManipulationCanvasName = "ManipulationCanvas";
        private Canvas _manipulationCanvas;

        private const string ScrollViewerName = "ScrollViewer";
        private ScrollViewer _scrollViewer;

        private const string ProgressRingName = "CatrobatListViewProgressRing";
        private ProgressRing _progressRing;

        private int _verticalItemMargin;
        private bool _reorderEnabled;
        public bool GroupingEnabled;

        private CatrobatListViewDragStaus _dragging;
        private CatrobatListViewItem _draggingItem;
        private List<object> _draggingGroupList;

        private const double ImageResizeFactor = 0.95;

        private CatrobatListViewDragObject _originalDragContent;
        private CatrobatListViewEmptyDummyControl _tmpDragContentControl;
        private const double InactiveItemResizeFactor = 0.07;

        private const int AutoScrollMargin = 20;
        private double _autoScrollOldYValue;
        private const double AutoScrollOffsetManual = 20;

        private double _rearrangeOldYValue;
        private const double YDifferenceBeforeRearrange = 10;

        private const int ItemContainerGeneratorError = -1;

        public ObservableCollection<object> SmartSelectedItems;
        public bool SelectionEnabled;

        public delegate void CatrobatListViewEventHandler(object sender, CatrobatListViewEventArgs e);
        public event CatrobatListViewEventHandler ItemDragCompletedEvent;

        public delegate void CatrobatListViewItemEventHandler(object sender, CatrobatListViewItemEventArgs e);

        public event CatrobatListViewItemEventHandler ItemTapped;

        public CatrobatListViewWorker()
        {
            _verticalItemMargin = 0;
            _reorderEnabled = false;
            GroupingEnabled = true;
            _dragging = CatrobatListViewDragStaus.NotDragging;
            _draggingItem = null;
            SmartSelectedItems = new ObservableCollection<object>();
            SmartSelectedItems.CollectionChanged += SmartSelectedItems_CollectionChanged;
            _autoScrollOldYValue = 0;
            _rearrangeOldYValue = 0;
            SelectionEnabled = false;
        }

        void SmartSelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SetItemsSelectedStyle(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                SetItemsSelectedStyle(e.OldItems);
            }
        }

        private void SetItemsSelectedStyle(IList itemsList)
        {
            for (int i = 0; i < itemsList.Count; i++)
            {
                var tmpItem = ContainerFromItem(itemsList[i]) as CatrobatListViewItem;
                if (tmpItem == null)
                {
                    continue;
                }
                if (SmartSelectedItems.Contains(itemsList[i]))
                {
                    tmpItem.SetSelected();
                }
                else
                {
                    tmpItem.SetUnselected();
                }
            }
        }

        private void AddRemoveSelectedItem(object obj, bool contains)
        {
            if (contains)
            {
                SmartSelectedItems.Remove(obj);
            }
            else
            {
                SmartSelectedItems.Add(obj);
            }
        }

        private void InitReorderableEmptyDummyControl()
        {
            if (_tmpDragContentControl != null)
                return;
            _tmpDragContentControl = new CatrobatListViewEmptyDummyControl();
            _tmpDragContentControl.Opacity = 0;
        }

        internal void ImportItemsSource(IList list)
        {
            TransferItemsSource(list);
        }

        private void TransferItemsSource(IList source)
        {
            if (source == null || Items == null)
            {
                return;
            }

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (source.Contains(Items[i]) == false)
                {
                    Items.RemoveAt(i);
                }
            }

            for (int i = 0; i < source.Count; i++)
            {
                int tmpTargetIndex = Items.IndexOf(source[i]);
                if (source[i] != null && tmpTargetIndex != i)
                {
                    if (tmpTargetIndex != -1)
                    {
                        Items.RemoveAt(Items.IndexOf(source[i]));
                    }
                    Items.Insert(i, source[i]);
                }
            }
        }

        public void CheckIfNewAddedBrick()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var tmp = Items[i] as Brick;
                if (tmp != null && tmp.IsNewAdded)
                {
                    tmp.IsNewAdded = false;
                    double viewport = _scrollViewer.VerticalOffset + this.ActualHeight / 2;
                    StartDrag(i, viewport);
                    double tmpHeight = 0;
                    for (int j = 0; j < Items.Count; j++)
                    {
                        tmpHeight += GetActualHeightFromIndex(j);
                        if (tmpHeight > viewport)
                        {
                            MoveItem(j, i);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void TransferSelectedItems(IList source)
        {
            if (source == null || SmartSelectedItems == null)
            {
                return;
            }

            for (int i = SmartSelectedItems.Count - 1; i >= 0; i--)
            {
                if (source.Contains(SmartSelectedItems[i]) == false)
                {
                    SmartSelectedItems.RemoveAt(i);
                }
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (SmartSelectedItems.Contains(source[i]) == false)
                {
                    SmartSelectedItems.Insert(i, source[i]);
                }
            }

        }

        internal void UpdateItemTemplateSelector(DataTemplate itemTemplate)
        {
            this.ItemTemplate = itemTemplate;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new CatrobatListViewItem(_verticalItemMargin, _reorderEnabled, GroupingEnabled, SelectionEnabled, _dragging == CatrobatListViewDragStaus.NotDragging);
            item.Tapped += item_Tapped;
            item.ItemGroupEvent += item_ItemGroupEvent;
            item.ItemSelectedEvent += item_ItemSelectedEvent;
            return item;
        }

        void item_ItemSelectedEvent(object sender, CatrobatListViewEventArgs e)
        {
            var tmpObj = (sender as CatrobatListViewItem).Content;
            bool contains = SmartSelectedItems.Contains(tmpObj);

            AddRemoveSelectedItem(tmpObj, contains);

            if (tmpObj is BlockBeginBrick)
            {
                AddRemoveSelectedItem((tmpObj as BlockBeginBrick).End, contains);
                if (tmpObj is IfBrick)
                {
                    AddRemoveSelectedItem((tmpObj as IfBrick).Else, contains);
                }
                else if (tmpObj is ElseBrick)
                {
                    AddRemoveSelectedItem((tmpObj as ElseBrick).Begin, contains);
                }
            }
            else if (tmpObj is BlockEndBrick)
            {
                AddRemoveSelectedItem((tmpObj as BlockEndBrick).Begin, contains);
                if (tmpObj is EndIfBrick)
                {
                    AddRemoveSelectedItem((tmpObj as EndIfBrick).Else, contains);
                }
            }

        }

        void item_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            _scrollViewer.Focus(FocusState.Pointer);
            ItemTapped(this, new CatrobatListViewItemEventArgs((CatrobatListViewItem)sender));
        }


        void item_ItemGroupEvent(object sender, CatrobatListViewEventArgs e)
        {
            _scrollViewer.Focus(FocusState.Pointer);
            GroupItem(sender as CatrobatListViewItem);
        }

        private void GroupItem(CatrobatListViewItem item)
        {
            int startIndex = Items.IndexOf(item.Content);

            int endIndex = 0;
            if (item.Content is Script)
            {
                endIndex = CalcMaxReorderIndex(startIndex + 1, true);
                endIndex--;

                SmartSelectedItems.Remove(item.Content);
            }
            else
            {
                endIndex = GetEndBrickIndex(item.Content);
            }

            ChangeItemsVisibility(startIndex + 1, endIndex, (item.IsGrouped && SmartSelectedItems.Contains(item.Content)));

            item.IsGrouped = !item.IsGrouped;
        }

        private void ChangeItemsVisibility(int startIndex, int endIndex, bool setSelected)
        {
            Visibility tmpVisibility = Visibility.Visible;
            for (int i = startIndex; i <= endIndex; i++)
            {
                var item = ContainerFromIndex(i) as CatrobatListViewItem;
                if (item == null)
                {
                    continue;
                }
                if (i == startIndex)
                {
                    tmpVisibility = item.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
                if (item.IsGrouped)
                {
                    i = GetEndBrickIndex(item.Content);
                }
                if (setSelected)
                {
                    if (!SmartSelectedItems.Contains(item.Content))
                    {
                        SmartSelectedItems.Add(item.Content);
                    }
                }
                else
                {
                    SmartSelectedItems.Remove(item.Content);
                }

                item.Visibility = tmpVisibility;
            }
        }



        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var itemContainer = (CatrobatListViewItem)element;
            itemContainer.ApplyTemplate();
            if (item is Script)
            {
                itemContainer.SetReorder(false);
            }
            else if (item is Brick && _reorderEnabled && SelectionEnabled == false)
            {
                itemContainer.SetReorder(true);
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CatrobatListViewItem;
        }

        internal void UpdateItemMargin(int value)
        {
            this._verticalItemMargin = value;

            if (this.Items == null)
            {
                return;
            }

            for (int i = 0; i < this.Items.Count; i++)
            {
                if (Items[i] is Brick)
                {
                    var item = this.ContainerFromIndex(i) as CatrobatListViewItem;
                    if (item == null)
                    {
                        continue;
                    }
                    item.SetVerticalMargin(_verticalItemMargin);
                }
            }
        }

        internal void SetReorderEnabled(bool value)
        {
            this._reorderEnabled = value;

            if (this.Items == null || _dragCanvas == null)
            {
                return;
            }

            for (int i = 0; i < this.Items.Count; i++)
            {
                if (!(Items[i] is Script))
                {
                    var item = this.ContainerFromIndex(i) as CatrobatListViewItem;
                    if (item == null)
                    {
                        continue;
                    }
                    item.SetReorder(_reorderEnabled);
                }
            }

            if (this._reorderEnabled)
            {
                _dragCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                _dragCanvas.Visibility = Visibility.Collapsed;
            }
        }

        public void SetProgessRingVisibility(Visibility newVisibility)
        {
            _progressRing.Visibility = newVisibility;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = GetTemplateChild(ScrollViewerName) as ScrollViewer;
            _manipulationCanvas = GetTemplateChild(ManipulationCanvasName) as Canvas;
            _dragCanvas = GetTemplateChild(DragCanvasName) as Canvas;
            _progressRing = GetTemplateChild(ProgressRingName) as ProgressRing;

            if (_dragCanvas == null || _manipulationCanvas == null || _scrollViewer == null || _progressRing == null)
            {
                throw new Exception("Container missing in CatrobatListViewWorker");
            }

            _dragCanvas.Tapped += _dragCanvas_Tapped;
            _dragCanvas.RightTapped += _dragCanvas_RightTapped;


            _dragCanvas.PointerPressed += _dragCanvas_PointerPressed;
            this.ManipulationDelta += CatrobatListViewWorker_ManipulationDelta;
            this.ManipulationCompleted += CatrobatListViewWorker_ManipulationCompleted;
            this.PointerReleased += CatrobatListViewWorker_PointerReleased;



            SetReorderEnabled(this._reorderEnabled);
            SetGroupingEnabled(this.GroupingEnabled);

            InitReorderableEmptyDummyControl();
        }

        private void CatrobatListViewWorker_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            EndDrag();
            e.Handled = true;
        }

        void CatrobatListViewWorker_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
            {
                e.Complete();
            }
            if (_dragging == CatrobatListViewDragStaus.Dragging)
            {
                DeltaDrag(e.Position.Y);
            }
            e.Handled = true;
        }

        void CatrobatListViewWorker_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            EndDrag();
        }

        void _dragCanvas_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        void _dragCanvas_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void DeltaDrag(double yPos)
        {
            if (yPos != _autoScrollOldYValue)
            {
                SetYPositionTo(yPos);
                Autoscroll(yPos);
                CheckRearrangeNecessaryFromDelta(yPos);
            }
        }

        private void CheckRearrangeNecessaryFromDelta(double yVal)
        {
            if (Math.Abs(_rearrangeOldYValue - yVal) >= YDifferenceBeforeRearrange)
            {
                _rearrangeOldYValue = yVal;
                RearrangeRolbi(yVal);
            }
        }

        private void RearrangeRolbi(double setYTo)
        {
            int actIndex = Items.IndexOf(_tmpDragContentControl);
            CatrobatListViewItem tmpItem;
            Rect itemsBounds;
            if (actIndex > 0)
            {
                tmpItem = (this.ContainerFromIndex(actIndex - 1) as CatrobatListViewItem);
                if (tmpItem == null)
                {
                    return;
                }
                if (tmpItem.Visibility == Visibility.Collapsed)
                {
                    if (tmpItem.Content is BlockEndBrick)
                    {
                        tmpItem = ContainerFromItem((tmpItem.Content as BlockEndBrick).Begin) as CatrobatListViewItem;
                    }

                }
                itemsBounds =
                    tmpItem.TransformToVisual(_scrollViewer)
                        .TransformBounds(new Rect(0.0, 0.0, tmpItem.ActualWidth, tmpItem.ActualHeight));

                if (setYTo < (itemsBounds.Top - _verticalItemMargin) + tmpItem.ActualHeight / 2)
                {
                    MoveItem(Items.IndexOf(tmpItem.Content)/*actIndex - 1*/, actIndex);
                    return;
                }
            }

            tmpItem = (this.ContainerFromIndex(actIndex + 1) as CatrobatListViewItem);
            if (tmpItem == null)
            {
                return;
            }

            itemsBounds = tmpItem.TransformToVisual(_scrollViewer).TransformBounds(new Rect(0.0, 0.0, tmpItem.ActualWidth, tmpItem.ActualHeight));
            if (setYTo > (itemsBounds.Bottom - _verticalItemMargin) - tmpItem.ActualHeight / 2)
            {
                if (tmpItem.IsGrouped)
                {
                    tmpItem = ContainerFromItem((tmpItem.Content as BlockBeginBrick).End) as CatrobatListViewItem;
                }
                MoveItem(Items.IndexOf(tmpItem.Content), actIndex);
            }

        }

        private void MoveItem(int to, int from)
        {
            if (to < _draggingItem.MinReorderIndex ||
                to > _draggingItem.MaxReorderIndex ||
                _draggingItem.InvalidReorderIndexes.Contains(to))
            {
                return;
            }
            {
                if (to > from)
                {
                    var tmpItem = ContainerFromIndex(to) as CatrobatListViewItem;

                    if (tmpItem != null && tmpItem.IsGrouped)
                    {
                        for (int i = to + 1; i < Items.Count; i++)
                        {
                            tmpItem = ContainerFromIndex(i) as CatrobatListViewItem;
                            if (tmpItem != null && tmpItem.Visibility == Visibility.Visible)
                            {
                                to = i - 1;
                                break;
                            }
                        }
                    }
                }
                Items.RemoveAt(from);
                Items.Insert(to, _tmpDragContentControl);
            }

            SetTmpDragContentHeight();
        }


        private int GetEndBrickIndex(object obj)
        {
            if (obj is BlockBeginBrick)
            {
                return Items.IndexOf((obj as BlockBeginBrick).End);
            }
            return 0;
        }

        private void SetTmpDragContentHeight()
        {
            var tmp = ContainerFromItem(_tmpDragContentControl) as CatrobatListViewItem;
            tmp.Height = (InactiveItemResizeFactor * _scrollViewer.RenderSize.Height);
        }


        private void Autoscroll(double yVal)
        {
            double actualPositionPercent = yVal / _scrollViewer.RenderSize.Height * 100;
            if (actualPositionPercent < AutoScrollMargin)
            {
                if (_autoScrollOldYValue >= yVal)
                {
                    ScrollToOffset(AutoScrollOffsetManual * -1);
                }
            }
            else if (actualPositionPercent > 100 - AutoScrollMargin)
            {
                if (_autoScrollOldYValue <= yVal)
                {
                    ScrollToOffset(AutoScrollOffsetManual);
                }
            }
            _autoScrollOldYValue = yVal;
        }

        private void ScrollToOffset(double delta)
        {
            double tmp = _scrollViewer.VerticalOffset + delta;
            if (tmp > _scrollViewer.ScrollableHeight)
                tmp = _scrollViewer.ScrollableHeight;
            _scrollViewer.ChangeView(null, tmp, null);
        }

        private void EndDrag()
        {
            if (_dragging == CatrobatListViewDragStaus.NotDragging)
            {
                return;
            }
            _dragging = CatrobatListViewDragStaus.NotDragging;
            _manipulationCanvas.Visibility = Visibility.Collapsed;

            if (Items.IndexOf(_tmpDragContentControl) != -1)
            {
                ItemDragCompletedEvent(this, new CatrobatListViewEventArgs(_tmpDragContentControl, _originalDragContent, null, null, _draggingGroupList));
                if (_draggingGroupList != null && _draggingGroupList.Count > 0)
                {
                    _draggingGroupList.Clear();
                    _draggingItem = null;
                    var tmpItem = ContainerFromItem(_originalDragContent.Content) as CatrobatListViewItem;
                    GroupItem(tmpItem);
                }
            }

        }

        void _dragCanvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PrepareStartDrag(e.GetCurrentPoint(_scrollViewer).Position.Y);
            e.Handled = true;
        }


        private void PrepareStartDrag(double yPos)
        {
            if (_dragging != CatrobatListViewDragStaus.NotDragging)
            {
                return;
            }

            int verticalOffset = (int)Math.Floor(_scrollViewer.VerticalOffset);

            double tmpHeight = 0;
            for (int i = 0; i < Items.Count; i++)
            {

                var tmpItem = ContainerFromIndex(i) as CatrobatListViewItem;
                if (tmpItem == null || tmpItem.Visibility == Visibility.Collapsed)
                {
                    continue;
                }
                tmpHeight += tmpItem.ActualHeight + _verticalItemMargin;
                if (tmpHeight + _verticalItemMargin > verticalOffset + yPos)
                {
                    StartDrag(i, yPos);
                    return;
                }
            }
        }

        private void StartDrag(int index, double yPos)
        {
            var tmpItem = this.ContainerFromIndex(index) as CatrobatListViewItem;
            if (tmpItem == null || tmpItem.ReorderEnabled == false)
            {
                return;
            }
            _dragging = CatrobatListViewDragStaus.PrepareDraggin;
            _draggingItem = tmpItem;

            InitDragContentObject();

            if (_draggingItem.IsGrouped)
            {
                InitDragGroupList();
            }

            MoveValidationCalculation(index);

            AddSnapshotToManipulationCanvas(yPos);
        }

        private void InitDragGroupList()
        {
            int lastGroupIndex = GetEndBrickIndex(_originalDragContent.Content as Brick);
            if (_draggingGroupList != null && _draggingGroupList.Count > 0)
            {
                return;
            }
            _draggingGroupList = new List<object>();

            for (int i = lastGroupIndex; i > Items.IndexOf(_originalDragContent.Content); i--)
            {
                _draggingGroupList.Add(Items[i]);
                Items.RemoveAt(i);
            }
        }

        private void MoveValidationCalculation(int index)
        {
            _draggingItem.MinReorderIndex = 0;
            _draggingItem.MaxReorderIndex = Items.Count;
            if (Items[index] is Brick)
            {
                if (_draggingItem.IsGrouped == false)
                {
                    if (Items[index] is BlockBeginBrick)
                    {
                        if (Items[index] is IfBrick)
                        {
                            _draggingItem.MinReorderIndex = CalcMinReorderIndex(index);
                            _draggingItem.MaxReorderIndex = Items.IndexOf((Items[index] as IfBrick).Else);
                        }
                        else if (Items[index] is ElseBrick)
                        {
                            _draggingItem.MinReorderIndex = Items.IndexOf((Items[index] as ElseBrick).Begin);
                            _draggingItem.MaxReorderIndex = Items.IndexOf((Items[index] as ElseBrick).End);
                        }
                        else
                        {
                            _draggingItem.MinReorderIndex = CalcMinReorderIndex(index);
                            _draggingItem.MaxReorderIndex = Items.IndexOf((Items[index] as BlockBeginBrick).End);
                        }
                    }
                    else if (Items[index] is BlockEndBrick)
                    {
                        if (Items[index] is EndIfBrick)
                        {
                            _draggingItem.MinReorderIndex = Items.IndexOf((Items[index] as EndIfBrick).Else);
                        }
                        else
                        {
                            _draggingItem.MinReorderIndex = Items.IndexOf((Items[index] as BlockEndBrick).Begin);
                        }
                        _draggingItem.MaxReorderIndex = CalcMaxReorderIndex(index);
                    }
                }

                _draggingItem.MinReorderIndex += 1;
                _draggingItem.MaxReorderIndex -= 1;
            }
            CalcInvalidReorderIndexes();
        }

        private void CalcInvalidReorderIndexes()
        {
            _draggingItem.InvalidReorderIndexes = new List<int>();

            for (int i = _draggingItem.MinReorderIndex > 0 ? _draggingItem.MinReorderIndex - 1 : 0;
                i <= _draggingItem.MaxReorderIndex; i++)
            {
                var tmpItem = ContainerFromIndex(i) as CatrobatListViewItem;
                if (tmpItem != null)
                {
                    if (tmpItem.Content is Script && tmpItem.IsGrouped)
                    {
                        _draggingItem.InvalidReorderIndexes.Add(i);
                        for (i++; i <= _draggingItem.MaxReorderIndex; i++)
                        {
                            tmpItem = ContainerFromIndex(i) as CatrobatListViewItem;
                            if (tmpItem != null)
                            {
                                _draggingItem.InvalidReorderIndexes.Add(i);
                                if (tmpItem.Content is Script && tmpItem.IsGrouped == false)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
            }
        }

        private int CalcMinReorderIndex(int index)
        {
            for (int i = index; i >= 0; i--)
            {
                if (Items[i] is Script || Items[i] is BlockEndBrick)
                {
                    return i;
                }
            }
            return 0;
        }

        private int CalcMaxReorderIndex(int index, bool forGrouping = false)
        {
            for (int i = index; i < Items.Count; i++)
            {
                if (Items[i] is Script)
                {
                    return i;
                }
                if (!forGrouping && Items[i] is BlockBeginBrick)
                {
                    var tmpItem = ContainerFromIndex(i) as CatrobatListViewItem;
                    if (tmpItem != null && tmpItem.IsGrouped)
                    {
                        i = Items.IndexOf((Items[i] as BlockBeginBrick).End);
                    }
                    else if (!tmpItem.IsGrouped)
                    {
                        return i;
                    }
                }
            }
            return Items.Count;
        }

        private void InitDragContentObject()
        {
            _originalDragContent = new CatrobatListViewDragObject(_draggingItem.Content);
        }

        private void AddSnapshotToManipulationCanvas(double yPos)
        {
            _manipulationCanvas.Children.Clear();
            CatrobatListViewItem tmpItemClone = GenerateDraggingItemClone();

            if (_dragging == CatrobatListViewDragStaus.PrepareDraggin)
            {
                Items[Items.IndexOf(_originalDragContent.Content)] = _tmpDragContentControl;
                SetTmpDragContentHeight();
                _manipulationCanvas.Children.Add(tmpItemClone);
                Canvas.SetLeft(tmpItemClone, ActualWidth - tmpItemClone.Width);

                SetYPositionTo(yPos);
                _manipulationCanvas.Visibility = Visibility.Visible;
                _dragging = CatrobatListViewDragStaus.Dragging;
            }
        }

        private CatrobatListViewItem GenerateDraggingItemClone()
        {
            CatrobatListViewItem tmpItemClone = new CatrobatListViewItem(_verticalItemMargin, this._reorderEnabled, this.GroupingEnabled, SelectionEnabled);
            tmpItemClone.Content = _draggingItem.Content;

            tmpItemClone.ContentTemplate = this.ItemTemplate;
            tmpItemClone.Style = this.ItemContainerStyle;
            tmpItemClone.Width = ActualWidth * ImageResizeFactor;
            tmpItemClone.IsGrouped = _draggingItem.IsGrouped;
            return tmpItemClone;
        }

        private void SetYPositionTo(double y)
        {
            CatrobatListViewItem tmpDragClone = GetDraggingItemClone();
            if (tmpDragClone != null)
            {
                Canvas.SetTop(tmpDragClone, y - (_draggingItem.OrigHeight * ImageResizeFactor) / 2);
            }
        }

        private CatrobatListViewItem GetDraggingItemClone()
        {
            try
            {
                return (CatrobatListViewItem)_manipulationCanvas.Children[0];
            }
            catch (Exception)
            {
                EndDrag();
                return null;
            }
        }

        private double GetActualHeightFromIndex(int index)
        {
            var item = this.ContainerFromIndex(index) as CatrobatListViewItem;
            if (item == null)
            {
                return -1;
            }
            return item.ActualHeight + _verticalItemMargin;
        }


        internal void UpdateSelectedItems(IList selectedItemsUpdated)
        {
            if (selectedItemsUpdated != null)
            {
                TransferSelectedItems(selectedItemsUpdated);
            }
        }

        internal void SetSelectionMode(bool value)
        {
            this.SelectionEnabled = value;
            if (SelectionEnabled)
            {
                this._dragCanvas.Visibility = Visibility.Collapsed;
            }
            else
            {
                this._dragCanvas.Visibility = Visibility.Visible;
            }
            for (int i = 0; i < this.Items.Count; i++)
            {
                var tmp = ContainerFromIndex(i) as CatrobatListViewItem;
                if (tmp != null)
                {
                    if (SelectionEnabled)
                    {
                        tmp.EnableSelectionMode();
                    }
                    else
                    {
                        tmp.DissableSelectionMode();
                    }
                }
            }
        }

        internal void SetItemWidth(int newWidth)
        {
            this.Width = newWidth;
        }

        internal void SetGroupingEnabled(bool value)
        {
            this.GroupingEnabled = value;

            if (this.Items == null || _dragCanvas == null)
            {
                return;
            }

            for (int i = 0; i < this.Items.Count; i++)
            {
                if (!(Items[i] is Script))
                {
                    var item = this.ContainerFromIndex(i) as CatrobatListViewItem;
                    if (item == null)
                    {
                        continue;
                    }
                    item.SetGrouping(GroupingEnabled);
                }
            }
        }
    }

    public class CatrobatListViewItem : ListViewItem
    {
        private Canvas _dragHandle;
        private const String DragHanldeName = "DragHandle";

        private Canvas _selectionHandleUnselected;
        private const String SelectionHandleUnselectedName = "SelectionHandleUnselected";

        private Canvas _selectionHandleSelected;
        private const String SelectionHandleSelectedName = "SelectionHandleSelected";

        private ContentControl _contentContainer;
        private const String ContentContainerName = "ContentContainer";

        private Grid _groupingGrid;
        private const String GroupingGridName = "GroupingGrid";

        private Canvas _clickPreventerCanvas;
        private const String ClickPreventerCanvasName = "ClickPreventerCanvas";

        private Canvas _groupingMinCanvas;
        private const String GroupingMinCanvasName = "GroupingMinCanvas";

        private Canvas _groupingMaxCanvas;
        private const String GroupingMaxCanvasName = "GroupingMaxCanvas";

        private ProgressRing _progressRing;
        private const String ProgressRingName = "CatrobatListViewItemProgressRing";

        public bool ReorderEnabled { get; private set; }
        public bool GroupingEnabled { get; private set; }
        private int _verticalItemMargin;

        public double OrigHeight { get; set; }

        public int MinReorderIndex { get; set; }
        public int MaxReorderIndex { get; set; }

        public List<int> InvalidReorderIndexes { get; set; }

        private bool _isGrouped;
        private bool _selectionEnabled;
        public bool IsGrouped
        {
            get { return _isGrouped; }
            set
            {
                _isGrouped = value;
                if (this.Content is BlockBeginBrick)
                {
                    (this.Content as BlockBeginBrick).IsGrouped = _isGrouped;
                }
                if (this.Content is Script && _isGrouped && _selectionEnabled)
                {
                    EnableSelectionMode();
                }
                else if (this.Content is Script)
                {
                    _selectionHandleSelected.Visibility = Visibility.Collapsed;
                    _selectionHandleUnselected.Visibility = Visibility.Collapsed;
                    _clickPreventerCanvas.Visibility = Visibility.Collapsed;
                }
                SetGroupingCanvasVisibility();
            }
        }



        private readonly bool _visible;

        public delegate void CatrobatListViewItemEventHandler(object sender, CatrobatListViewEventArgs e);
        public event CatrobatListViewItemEventHandler ItemGroupEvent;

        public event CatrobatListViewItemEventHandler ItemSelectedEvent;


        public CatrobatListViewItem(int verticalItemMargin, bool reorderEnabled, bool groupingEnabled, bool selectionEnabled, bool visible = true)
        {
            OrigHeight = -1;
            ReorderEnabled = reorderEnabled;
            GroupingEnabled = groupingEnabled;
            _verticalItemMargin = verticalItemMargin;
            MinReorderIndex = 0;
            MaxReorderIndex = 0;
            InvalidReorderIndexes = new List<int>();
            _visible = visible;
            _isGrouped = false;
            _selectionEnabled = selectionEnabled;
        }


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragHandle = GetTemplateChild(DragHanldeName) as Canvas;
            _selectionHandleUnselected = GetTemplateChild(SelectionHandleUnselectedName) as Canvas;
            _selectionHandleSelected = GetTemplateChild(SelectionHandleSelectedName) as Canvas;
            _contentContainer = GetTemplateChild(ContentContainerName) as ContentControl;
            _clickPreventerCanvas = GetTemplateChild(ClickPreventerCanvasName) as Canvas;
            _groupingGrid = GetTemplateChild(GroupingGridName) as Grid;
            _groupingMaxCanvas = GetTemplateChild(GroupingMaxCanvasName) as Canvas;
            _groupingMinCanvas = GetTemplateChild(GroupingMinCanvasName) as Canvas;
            _progressRing = GetTemplateChild(ProgressRingName) as ProgressRing;

            if (_dragHandle == null || _selectionHandleSelected == null || _selectionHandleUnselected == null ||
                _contentContainer == null || _clickPreventerCanvas == null || _groupingGrid == null ||
                _groupingMaxCanvas == null || _groupingMinCanvas == null || _progressRing == null)
            {
                throw new Exception("Container missing in CatrobatListViewItem");
            }

            if (Content != null && Content.GetType().Namespace == typeof(Script).Namespace)
            {
                _selectionHandleUnselected.Margin = new Thickness(0, 22, 0, 0);
                _selectionHandleSelected.Margin = new Thickness(0, 22, 0, 0);
                _groupingMaxCanvas.Margin = new Thickness(10, 47, 0, 0);
                _groupingMinCanvas.Margin = new Thickness(13, 47, 0, 0);
            }

            InitGrouping();
            SetGroupingCanvasVisibility();

            SetVerticalMargin(_verticalItemMargin);
            SetReorder(ReorderEnabled);
            if (_selectionEnabled)
            {
                EnableSelectionMode();
            }
            if (_visible == false)
            {
                this.Opacity = 0;
            }
            this.SizeChanged += CatrobatListViewItem_SizeChanged;
            this._clickPreventerCanvas.Tapped += _clickPreventerCanvas_Tapped;
        }

        void _clickPreventerCanvas_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ItemSelectedEvent != null)
            {
                ItemSelectedEvent(this, null);
            }
            e.Handled = true;
        }

        private void SetGroupingCanvasVisibility()
        {
            if (_groupingMaxCanvas != null)
            {

                if (_isGrouped)
                {
                    _groupingMinCanvas.Visibility = Visibility.Visible;
                    _groupingMaxCanvas.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _groupingMinCanvas.Visibility = Visibility.Collapsed;
                    _groupingMaxCanvas.Visibility = Visibility.Visible;
                }
            }
        }

        void _groupingMaxCanvas_LayoutUpdated(object sender, object e)
        {
            _groupingMaxCanvas.LayoutUpdated -= _groupingMaxCanvas_LayoutUpdated;
            _groupingGrid.Tapped += _groupingCanvas_Tapped;
            _progressRing.Visibility = Visibility.Collapsed;
        }

        private void InitGrouping()
        {
            if (GroupingEnabled && (this.Content is Script || (this.Content is BlockBeginBrick && !(this.Content is ElseBrick))))
            {
                _groupingGrid.Tapped += _groupingCanvas_Tapped;
                _groupingGrid.Visibility = Visibility.Visible;
            }
            else
            {
                _groupingGrid.Tapped -= _groupingCanvas_Tapped;
                _groupingGrid.Visibility = Visibility.Collapsed;
            }
        }

        void _groupingCanvas_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ItemGroupEvent != null)
            {
                _progressRing.Visibility = Visibility.Visible;
                _groupingMaxCanvas.Visibility = Visibility.Collapsed;
                _groupingMinCanvas.Visibility = Visibility.Collapsed;
                _groupingGrid.Tapped -= _groupingCanvas_Tapped;

                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    ItemGroupEvent(this, null);
                    _groupingMaxCanvas.LayoutUpdated += _groupingMaxCanvas_LayoutUpdated;
                });
            }
            e.Handled = true;
        }

        void CatrobatListViewItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualHeight > 0 && OrigHeight == -1)
            {
                this.OrigHeight = this.ActualHeight;
            }
        }

        internal void SetVerticalMargin(int verticalMargin)
        {
            _verticalItemMargin = verticalMargin;
            this.Margin = new Thickness(0, _verticalItemMargin, 0, 0);
        }

        internal void SetReorder(bool reorder)
        {
            ReorderEnabled = reorder;
            if (ReorderEnabled == false)
            {
                _dragHandle.Visibility = Visibility.Collapsed;
            }
            else
            {
                _dragHandle.Visibility = Visibility.Visible;
            }
        }

        internal void SetSelected()
        {
            _selectionHandleUnselected.Visibility = Visibility.Collapsed;
            _selectionHandleSelected.Visibility = Visibility.Visible;
            _contentContainer.Opacity = 1;
        }

        internal void SetUnselected()
        {
            _selectionHandleUnselected.Visibility = Visibility.Visible;
            _selectionHandleSelected.Visibility = Visibility.Collapsed;
            _contentContainer.Opacity = 1; //0.6 for transparent
        }

        internal void EnableSelectionMode()
        {
            _selectionEnabled = true;
            _dragHandle.Visibility = Visibility.Collapsed;
            _contentContainer.Opacity = 1; //0.6 for transparent
            if (this.Content is Script && !IsGrouped)
            {
                return;
            }
            _clickPreventerCanvas.Visibility = Visibility.Visible;
            SetUnselected();
        }

        internal void DissableSelectionMode()
        {
            _selectionEnabled = false;
            if (this.ReorderEnabled)
            {
                _dragHandle.Visibility = Visibility.Visible;
            }
            _selectionHandleUnselected.Visibility = Visibility.Collapsed;
            _selectionHandleSelected.Visibility = Visibility.Collapsed;
            _clickPreventerCanvas.Visibility = Visibility.Collapsed;
            _contentContainer.Opacity = 1;
        }

        internal void SetGrouping(bool groupingEnabled)
        {
            GroupingEnabled = groupingEnabled;
            InitGrouping();
        }
    }

    public class CatrobatListViewDragObject
    {
        public CatrobatListViewDragObject(object content)
        {
            Content = content;
        }

        public object Content { get; set; }
    }

    public class CatrobatListViewEmptyDummyControl : Control
    {
        public PortableImage Image { get; set; } // need for ListViewLooks

        // need for SpriteControl
        public int ActionsCount { get; set; }
        public List<object> Sounds { get; set; }
        public List<object> Looks { get; set; }
    }

    public enum CatrobatListViewDragStaus
    {
        NotDragging, PrepareDraggin, Dragging
    }

    public class CatrobatListViewItemEventArgs : EventArgs
    {
        private readonly CatrobatListViewItem _tappedItem;

        public CatrobatListViewItemEventArgs(CatrobatListViewItem tappeItem)
        {
            this._tappedItem = tappeItem;
        }

        public CatrobatListViewItem GetTappedItem()
        {
            return this._tappedItem;
        }
    }
    public class CatrobatListViewEventArgs : EventArgs
    {
        private readonly CatrobatListViewEmptyDummyControl _tmpControl;
        private readonly CatrobatListViewDragObject _orignalControl;
        private readonly IList<object> _addedSelectedItems;
        private readonly IList<object> _removedSelectedItems;
        private readonly IList<object> _groupedItems;
        public CatrobatListViewEventArgs(CatrobatListViewEmptyDummyControl tmpControl, CatrobatListViewDragObject orignalControl,
            IList<object> addedSelectedItems, IList<object> removedSelectedItems, IList<object> groupedItems)
        {
            _tmpControl = tmpControl;
            _orignalControl = orignalControl;
            _addedSelectedItems = addedSelectedItems;
            _removedSelectedItems = removedSelectedItems;
            _groupedItems = groupedItems;
        }

        public CatrobatListViewDragObject GetOrignalContent()
        {
            return _orignalControl;
        }

        public CatrobatListViewEmptyDummyControl GetTmpControl()
        {
            return _tmpControl;
        }

        public IList<object> GetRemovedSelectedItems()
        {
            return _removedSelectedItems;
        }

        public IList<object> GetAddedSelectedItems()
        {
            return _addedSelectedItems;
        }

        public IList<object> GetGroupedItems()
        {
            return _groupedItems;
        }
    }
}
