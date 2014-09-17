
WPFLiteTreeView
===============

WPF Tree View doesn't scale very good, when you expand a tree item with thousands of items the performance is bad. This TreeView implementation uses a ListBox to simulate a tree


Usage
---------------
1. WPFLiteTreeView doesnt support HierarchicalDataTemplate , Instead your model objects needs to implement the IHaveChildren interface
	```csharp
	public interface IHaveChildren
	{
	    IEnumerable Children { get; }  
	}
	```

2. Instead of setting ItemsSource inside your xaml, you need to set MyItemsSource property

	```xaml
	 <liteTreeView:LiteTreeViewControl Grid.Column="1"
	                                   MyItemsSource="{Binding Items}">
	    <liteTreeView:LiteTreeViewControl.ItemTemplate>
	        <DataTemplate>
	            <TextBlock Text="{Binding Name}"></TextBlock>
	        </DataTemplate>
	    </liteTreeView:LiteTreeViewControl.ItemTemplate>            
	</liteTreeView:LiteTreeViewControl>
	```

3. LiteTreeViewControl uses an item container named LiteTreeViewItem.
this how you can change its style

```xaml
<liteTreeView:LiteTreeViewControl Grid.Column="1"
                                  MyItemsSource="{Binding Path=Items}">   
    <liteTreeView:LiteTreeViewControl.ItemContainerStyle>
        <Style TargetType="liteTreeView:LiteTreeViewItem">
            <Setter Property="IsOpen"
                    Value="{Binding IsItOpen,Mode=TwoWay}"></Setter>
            <Setter Property="IsSelected"
                    Value="{Binding IsItSelected,Mode=TwoWay}"></Setter>
        </Style>
    </liteTreeView:LiteTreeViewControl.ItemContainerStyle>
</liteTreeView:LiteTreeViewControl>
```

> Written with [StackEdit](https://stackedit.io/).
