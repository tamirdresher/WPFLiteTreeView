using System.Collections;

namespace LiteTreeView
{
    class HaveChildrenCollectionWrapper : IHaveChildren
    {
        public HaveChildrenCollectionWrapper(IEnumerable collection)
        {
            Children = collection;
        }
        public IEnumerable Children { get; private set; }
    }
}