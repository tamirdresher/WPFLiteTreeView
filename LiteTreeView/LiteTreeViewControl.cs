using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace LiteTreeView
{
    public class LiteTreeViewControl : ListBox
    {
        #region HaveChildrenCollectionWrapper

        class HaveChildrenCollectionWrapper : IHaveChildren
        {
            public HaveChildrenCollectionWrapper(IEnumerable collection)
            {
                Children = collection;
            }

            public IEnumerable Children { get; private set; }
        }

        #endregion

        public LiteTreeViewControl()
        {
            SetResourceReference(StyleProperty, typeof(ListBox));
        }
        static LiteTreeViewControl()
        {
            //var oldMetadata=(FrameworkPropertyMetadata)ListBox.SelectedItemProperty.GetMetadata(typeof (ListBox));
            //)
            
            ListBox.SelectedItemProperty.OverrideMetadata(typeof(LiteTreeViewControl),new FrameworkPropertyMetadata(null,OnSelecteItemChanged ));
        }

        private object _lastSelectedItem;
        private static void OnSelecteItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
                var listBox = (dependencyObject as LiteTreeViewControl);

            if (dependencyPropertyChangedEventArgs.NewValue!=null)
            {
                listBox._lastSelectedItem = dependencyPropertyChangedEventArgs.NewValue;
            }
        }

        public ObservableCollection<LiteTreeViewItemViewModel> InternalCollection { get; set; }

        public IEnumerable MyItemsSource
        {
            get { return (IEnumerable)GetValue(MyItemsSourceProperty); }
            set { SetValue(MyItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyItemsSourceProperty =
            DependencyProperty.Register("MyItemsSource", typeof(IEnumerable), typeof(LiteTreeViewControl), new PropertyMetadata(null, MyItemsSourceChanged));

        private static void MyItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var treeListBox = dependencyObject as LiteTreeViewControl;
            treeListBox.MyItemsSourceChanged(eventArgs);
        }

        void MyItemsSourceChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            var observableCollectionAdv = new ObservableCollection<LiteTreeViewItemViewModel>();
            if (Root != null)
            {
                Root.Dispose();
            }
            if (eventArgs.NewValue != null)
            {
                var inputCollection = eventArgs.NewValue as IEnumerable;
                Root = new LiteTreeViewItemViewModel { IsOpen = true, Level = -1 };
                Root.InnerObject = new HaveChildrenCollectionWrapper(inputCollection);
                Root.FillTreeList(observableCollectionAdv);
            }
            InternalCollection = observableCollectionAdv;
            ItemsSource = InternalCollection;
        }

        LiteTreeViewItemViewModel Root { get; set; }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new LiteTreeViewItem();
        }
        
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is LiteTreeViewItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var ti = element as LiteTreeViewItem;
            var vm = item as LiteTreeViewItemViewModel;
            
            if (ti != null&&vm!=null)
            {
                if (this.ItemContainerStyle != null && ti.Style == null)
                {
                    ti.SetValue(FrameworkElement.StyleProperty, this.ItemContainerStyle);
                }
                if (this.ItemTemplate != null && ti.ContentTemplate == null)
                {
                    ti.SetValue(ContentControl.ContentTemplateProperty, this.ItemTemplate);
                }
                if (this.ItemTemplateSelector != null && ti.ContentTemplateSelector == null)
                {
                    ti.SetValue(ContentControl.ContentTemplateSelectorProperty, this.ItemTemplateSelector);
                }
               ti.Content = vm.InnerObject;
            ti.DataContext = vm.InnerObject;

                ti.VM = vm;
            }
            //base.PrepareContainerForItemOverride(element, item);

            
        }

        internal void RefreshSelectedItem()
        {
            if (SelectedItem!=_lastSelectedItem)
            {
                SelectedItem = _lastSelectedItem;
            }
        }
    }
}