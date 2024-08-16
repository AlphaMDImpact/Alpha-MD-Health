using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AlphaMDHealth.Utility;

public class ObservableCollectionExtension<T> : ObservableCollection<T>
{
    public ObservableCollectionExtension(IEnumerable<T> collection) : base(collection)
    {

    }

    public ObservableCollectionExtension() : base()
    {

    }

    public void InsertRange(IEnumerable<T> items)
    {
        this.CheckReentrancy();
        foreach (var item in items)
        {
            this.Items.Add(item);
        }
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}