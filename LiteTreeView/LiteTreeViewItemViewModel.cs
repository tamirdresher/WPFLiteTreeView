using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LiteTreeView
{
    public class LiteTreeViewItemViewModel : DependencyObject, INotifyPropertyChanged, IDisposable, IWeakEventListener
    {
        static class CollectionExtensions
        {
            public static void RemoveRange<T>(IList<T> source, int index, int count)
            {
                while (count > 0 && source.Count > index)
                {
                    source.RemoveAt(index);
                    count--;
                }
            }

            public static void InsertRange<T>(IList<T> source, int index, IEnumerable<T> collection)
            {
                int i = 0;
                foreach (var item in collection)
                {
                    source.Insert(index + i, item);

                    i++;
                }
            }
        }

        private const int LEVEL_LEFT_MARGIN = 15;

        private int _level;
        private object _innerObject;
        private bool _isDisposed;
        IDictionary<LiteTreeViewItemViewModel, object> _treeItemsToItems = new Dictionary<LiteTreeViewItemViewModel, object>();
        IDictionary<object, LiteTreeViewItemViewModel> _itemsToTreeItems = new Dictionary<object, LiteTreeViewItemViewModel>();

        public LiteTreeViewItemViewModel()
        {
            Children = new List<LiteTreeViewItemViewModel>();
        }

        static LiteTreeViewItemViewModel()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(LiteTreeViewItemViewModel), new FrameworkPropertyMetadata(typeof(LiteTreeViewItemViewModel)));
        }

        public Thickness LevelMargin
        {
            get { return new Thickness(LEVEL_LEFT_MARGIN * Level, 0, 0, 0); }
        }

       
        public int Level
        {
            get { return _level; }
            set
            {
                if (value == _level) return;
                _level = value;
                OnPropertyChanged("Level");
                OnPropertyChanged("LevelMargin");
            }
        }

        public IList<LiteTreeViewItemViewModel> Children { get; set; }

        public bool HasChildren
        {
            get { return Children != null && Children.Count > 0; }
        }

        public Visibility HasChildrenVisibility
        {
            get { return HasChildren ? Visibility.Visible : Visibility.Hidden; }
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(LiteTreeViewItemViewModel), new PropertyMetadata(false, IsOpenChanged));

        private static void IsOpenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var newVal = (bool)dependencyPropertyChangedEventArgs.NewValue;

            var myTreeItem = dependencyObject as LiteTreeViewItemViewModel;
            myTreeItem.IsOpenChanged(newVal);
        }

  
        void IsOpenChanged(bool newValue)
        {
            if (newValue)
            {
                if (IsTopLevel() || MyParent.IsOpen)
                {
                    FillTreeList(ContainerList);
                }

            }
            else
            {
                RemoveFromTreeList();
            }
            var liteTreeViewControl = (ItemsControl.ItemsControlFromItemContainer(this) as LiteTreeViewControl);
            if (liteTreeViewControl!=null)
            {
                liteTreeViewControl.RefreshSelectedItem();
                
            }
        }

        public LiteTreeViewItemViewModel MyParent { get; set; }

        private bool IsTopLevel()
        {
            return MyParent == null;
        }

        public void Add(LiteTreeViewItemViewModel child, int index = -1)
        {
            if (Children == null)
            {
                Children = new List<LiteTreeViewItemViewModel>();
            }
            if (index == -1)
            {
                Children.Add(child);

            }
            else
            {
                Children.Insert(index, child);
            }
            child.MyParent = this;
            child.Level = this.Level + 1;
        }
        public void InvalidateVisibility(bool isRootChange = true)
        {
            OnPropertyChanged("IsVisible");
            OnPropertyChanged("Visibility");
            if (HasChildren)
            {
                if (IsOpen || isRootChange)
                {
                    foreach (var child in Children)
                    {
                        child.InvalidateVisibility(false);
                    }
                }

            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void FillTreeList(ObservableCollection<LiteTreeViewItemViewModel> items)
        {
            int index = -1;
            FillTreeList(items, ref index);
        }

        public void FillTreeList(ObservableCollection<LiteTreeViewItemViewModel> items, ref int myIndex)
        {
            if (items == null)
            {
                return;
            }
            ContainerList = items;
            if (HasChildren)
            {
                FillTreeList(items, Children, ref myIndex);
            }
        }

        void FillTreeList(ObservableCollection<LiteTreeViewItemViewModel> items, IList<LiteTreeViewItemViewModel> children, ref int myIndex)
        {
            if (items == null)
            {
                return;
            }
            ContainerList = items;
            if (children != null && children.Any())
            {
                if (myIndex == -1)
                {
                    myIndex = items.IndexOf(this);
                }
                if (IsOpen)
                {
                    CollectionExtensions.InsertRange(items, myIndex + 1, children);

                    foreach (var child in children)
                    {
                        ++myIndex;
                        //items.Insert(myIndex, child);
                        child.FillTreeList(items, ref myIndex);
                    }
                }
            }
        }

        public ObservableCollection<LiteTreeViewItemViewModel> ContainerList { get; set; }

        public object InnerObject
        {
            get { return _innerObject; }
            set
            {
                Clear();
                DetachFromCollectionChanged(_innerObject);
                _innerObject = value;

                var parent = _innerObject as IHaveChildren;
                if (parent != null)
                {
                    foreach (var child in parent.Children)
                    {
                        AddChild(child);
                    }

                    var notifyCollectionChanged = parent.Children as INotifyCollectionChanged;
                    if (notifyCollectionChanged != null)
                    {
                        CollectionChangedEventManager.AddListener(notifyCollectionChanged, this);
                    }
                }
            }
        }

        private void DetachFromCollectionChanged(object innerObj)
        {
            var oldInnerObject = innerObj as IHaveChildren;
            if (oldInnerObject != null)
            {
                var notifyCollectionChanged = oldInnerObject.Children as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    CollectionChangedEventManager.RemoveListener(notifyCollectionChanged, this);
                }
            }
        }

        private LiteTreeViewItemViewModel AddChild(object child, int index = -1)
        {
            var myTreeItem = new LiteTreeViewItemViewModel();
            Add(myTreeItem, index);
            myTreeItem.InnerObject = child;
            _itemsToTreeItems[child] = myTreeItem;
            _treeItemsToItems[myTreeItem] = child;
            return myTreeItem;
        }

        void RemoveChild(object child)
        {
            var treeItem = _itemsToTreeItems[child];
            treeItem.RemoveFromTreeList(false);
            Children.Remove(treeItem);
            _itemsToTreeItems.Remove(child);
            _treeItemsToItems.Remove(treeItem);
            if (IsOpen)
            {
                ContainerList.Remove(treeItem);
            }
            treeItem.Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            DetachFromCollectionChanged(_innerObject);
            foreach (var child in Children)
            {
                child.Dispose();
            }
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~LiteTreeViewItemViewModel()
        {
            Dispose(false);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            var args = e as NotifyCollectionChangedEventArgs;

            if (args == null) return true;

            CollectionChanged(sender, args);

            return true;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InsertAddedItems(e);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        RemoveChild(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    InnerObject = InnerObject;//simulate change of inner object to reset all
                    if (IsOpen)
                    {
                        IsOpenChanged(true);
                    }
                    break;
            }

            OnPropertyChanged("HasChildren");
            OnPropertyChanged("HasChildrenVisibility");
        }

        private void Clear()
        {
            var myTreeItems = Children.ToArray();
            foreach (var myTreeItem in myTreeItems)
            {
                RemoveChild(myTreeItem.InnerObject);
            }
        }

        private void InsertAddedItems(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                int index = e.NewStartingIndex;
                int nextItemIndex = index;

                if (!HasChildren)
                {
                    //first child, should be in the treeitem place
                    nextItemIndex = -1;
                }
                else if (index == 0)
                {
                    nextItemIndex = -1;
                }
                else if (index == Children.Count)
                {
                    nextItemIndex = ContainerList.IndexOf(this) + CountVisibleDescendatns();
                }
                else
                {
                    var nextItem = Children[index];
                    nextItemIndex = ContainerList.IndexOf(nextItem) - 1;
                }

                var newChildren = new List<LiteTreeViewItemViewModel>();
                foreach (object obj in e.NewItems)
                {
                    newChildren.Add(AddChild(obj, index));
                    index++;
                }
                FillTreeList(ContainerList, newChildren, ref nextItemIndex);
            }
        }

        public void RemoveFromTreeList(bool isRoot = true)
        {
            if (HasChildren)
            {
                if (IsOpen || isRoot)
                {
                    //var myIndex = ContainerList.IndexOf(this);
                    //ContainerList.RemoveRange(myIndex+1,CountVisibleDescendatns());
                    foreach (var child in Children)
                    {

                        ContainerList.Remove(child);
                        child.RemoveFromTreeList(false);
                    }
                }
            }
        }

        int CountVisibleDescendatns()
        {
            int count = 0;
            if (HasChildren && IsOpen)
            {
                count += Children.Count;
                foreach (var child in Children.Where(c => c.HasChildren))
                {
                    count += child.CountVisibleDescendatns();
                }
            }
            return count;
        }

        public void ExpandAll()
        {
            if (HasChildren)
            {
                foreach (var child in Children)
                {
                    child.ExpandAll();
                }
            }
            IsOpen = true;
        }

       

       
    }
}