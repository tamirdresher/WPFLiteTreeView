using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TreeAsListBox;
using TreeAsListBox.Annotations;

namespace LiteTreeViewTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        private ObservableCollection<DummyObject> _items;

        public ObservableCollection<DummyObject> Items
        {
            get { return _items; }
            set
            {
                if (Equals(value, _items)) return;
                _items = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DummyObject> DummyObjects
        {
            get { return _dummyObjects; }
            set { _dummyObjects = value; }
        }

        public ObservableCollection<DummyObject> DummyObjects2
        {
            get { return _dummyObjects2; }
            set { _dummyObjects2 = value; }
        }

        private ObservableCollection<DummyObject> _dummyObjects = new ObservableCollection<DummyObject>();
        private ObservableCollection<DummyObject> _dummyObjects2 = new ObservableCollection<DummyObject>(); 
        public MainWindow()
        {
            FillItems(_dummyObjects);
            FillItems(_dummyObjects2);
            Items = _dummyObjects;
            InitializeComponent();
        }

        private void FillItems(ObservableCollection<DummyObject> collection)
        {
            for (int i = 0; i < 15; i++)
            {
                collection.Add(new DummyObject() { Name = "item" + i });
                var parent = new DummyObject()
                {
                    Name = "parent item" + i,
                };
                collection.Add(parent);
                for (int j = 0; j < 2000; j++)
                {
                    var child = new DummyObject() {Name = "child item " + j};
                    parent.Add(child);
                    for (int k = 0; k < 4; k++)
                    {
                        var subChild = new DummyObject() {Name = "sub child item " + k};
                        child.Add(subChild);
                    }
                }
            }
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            var items = Items.ToArray();
                foreach (var item in items)
                {
                    item.ExpandAll();
                    // Dispatcher.Invoke(() => item.ExpandAll(), DispatcherPriority.Background);


                }
            
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            Items[1].Add(new DummyObject() { Name = "newone" });
        }

        private void AddItem2(object sender, RoutedEventArgs e)
        {
            var newdummyObject = new DummyObject() { Name = "newone1" };
            Items.First(x => x.ChildrenList.Any()).ChildrenList.Insert(2, newdummyObject);
            //dummyObject.Add(new DummyObject(){Name = "another new"});
            foreach (DummyObject dummyObject in Items[1].Children)
            {
                //dummyObject.Add(new DummyObject() { Name = "another new" });
                dummyObject.ChildrenList.Insert(0, new DummyObject() { Name = "another new" });
            }
        }

        
        private void RemoveChild(object sender, RoutedEventArgs e)
        {
            Items[1].ChildrenList.Remove(Items[1].ChildrenList[1]);
        }

        //private void AddRange(object sender, RoutedEventArgs e)
        //{
        //    Items[1].ChildrenList.InsertRange(0, new[]
        //    {
        //        new DummyObject() {Name = "item1"},
        //        new DummyObject() {Name = "item2"}
        //    });
        //}

        private void CloseAll(object sender, RoutedEventArgs e)
        {
            var items = Items.ToArray();
            foreach (var item in items)
            {
                item.CollapseAll();
                // Dispatcher.Invoke(() => item.ExpandAll(), DispatcherPriority.Background);


            }
        }

        private void AddToRoot(object sender, RoutedEventArgs e)
        {
            Items.Insert(0,new DummyObject(){Name = "newroot"});
        }

        private void ChangeSource(object sender, RoutedEventArgs e)
        {
            Items = Items==_dummyObjects ? _dummyObjects2 : _dummyObjects;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
