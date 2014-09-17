using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LiteTreeView
{
    public class LiteTreeViewItem : ListBoxItem, INotifyPropertyChanged
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


        
        private LiteTreeViewItemViewModel _vm;

        public LiteTreeViewItem()
        {
        }

        static LiteTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LiteTreeViewItem), new FrameworkPropertyMetadata(typeof(LiteTreeViewItem)));
            DataContextProperty.OverrideMetadata(typeof(LiteTreeViewItem),new FrameworkPropertyMetadata((o,e)=>{},DataContextCoerceValueCallback));
        }

        private static object DataContextCoerceValueCallback(DependencyObject dependencyObject, object baseValue)
        {
            if (baseValue!=null)
            {
                var vm=baseValue as LiteTreeViewItemViewModel;
                if (vm!=null)
                {
                    return vm.InnerObject;
                }
            }
            return baseValue;
        }

        public Thickness LevelMargin
        {
            get { return VM.LevelMargin; }
        }

        

        public int Level
        {
            get { return VM.Level; }
            
        }

        //public IList<LiteTreeViewItem> Children { get; set; }

        public bool HasChildren
        {
            get { return VM!=null&&VM.HasChildren; }
        }

        public Visibility HasChildrenVisibility
        {
            get { return VM != null?  VM.HasChildrenVisibility:Visibility.Hidden; }
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(LiteTreeViewItem), new PropertyMetadata(false, IsOpenChanged));

        public LiteTreeViewItemViewModel VM
        {
            get { return _vm; }
            set
            {
                if (_vm!=null)
                {
                    BindingOperations.ClearBinding(_vm, LiteTreeViewItemViewModel.IsOpenProperty);
                    _vm.PropertyChanged -= VmOnPropertyChanged;
                }
                _vm = value;
                
                if (_vm!=null)
                {
                    Binding myBinding = new Binding("IsOpen") { Mode = BindingMode.TwoWay };
                    myBinding.Source = this;
                    BindingOperations.SetBinding(_vm, LiteTreeViewItemViewModel.IsOpenProperty, myBinding);

                    _vm.PropertyChanged += VmOnPropertyChanged;

                    OnPropertyChanged("HasChildren");
                    OnPropertyChanged("HasChildrenVisibility");
                    OnPropertyChanged("IsOpen");
                    OnPropertyChanged("Level");
                    OnPropertyChanged("LevelMargin");
                }
                
            }
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            OnPropertyChanged(propertyChangedEventArgs.PropertyName);
        }

        private static void IsOpenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var liteTreeViewControl = (ItemsControl.ItemsControlFromItemContainer(dependencyObject) as LiteTreeViewControl);
            if (liteTreeViewControl != null)
            {
                liteTreeViewControl.RefreshSelectedItem();

            }
        }


       
       
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

      
        
    }
}