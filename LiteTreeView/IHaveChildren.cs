using System.Collections;

namespace LiteTreeView
{
    public interface IHaveChildren
    {
        IEnumerable Children { get; }  
    }
}