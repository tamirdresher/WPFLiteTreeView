using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiteTreeView;
using TreeAsListBox.Annotations;

namespace TreeAsListBox
{
    public class DummyObject : IHaveChildren,INotifyPropertyChanged
    {
        private ObservableCollection<DummyObject> _children;
        private bool _isItOpen;

        public DummyObject()
        {
            ChildrenList = new ObservableCollection<DummyObject>();
        }
        public string Name { get; set; }

        public bool IsItOpen
        {
            get { return _isItOpen; }
            set
            {
                if (value.Equals(_isItOpen)) return;
                _isItOpen = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable Children
        {
            get { return ChildrenList; }
        }

        public ObservableCollection<DummyObject> ChildrenList
        {
            get { return _children; }
            set { _children = value; }
        }

        public void Add(DummyObject child)
        {
            ChildrenList.Add(child);
        }

        public void ExpandAll()
        {

            foreach (var child in ChildrenList)
            {
                child.ExpandAll();
            }

            IsItOpen = true;
        }

        public void CollapseAll()
        {

            foreach (var child in ChildrenList)
            {
                child.CollapseAll();
            }

            IsItOpen = false;
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