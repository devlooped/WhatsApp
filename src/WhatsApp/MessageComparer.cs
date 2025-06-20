namespace Devlooped.WhatsApp;

enum MessageOrder { Ascending, Descending }

class MessageComparer(MessageOrder order = MessageOrder.Ascending) : Comparer<IMessage>
{
    public static Comparer<IMessage> Ascending { get; } = new MessageComparer(MessageOrder.Ascending);
    public static Comparer<IMessage> Descending { get; } = new MessageComparer(MessageOrder.Descending);

    public override int Compare(IMessage? x, IMessage? y)
    {
        if (x is null && y is null)
            return 0;
        if (x is null)
            return -1;
        if (y is null)
            return 1;

        // Compare by timestamp first
        var result = x.Timestamp.CompareTo(y.Timestamp);
        if (result != 0)
            return order == MessageOrder.Ascending ? result : -result;

        // If timestamps are equal, compare by ID
        return order == MessageOrder.Ascending ? x.Id.CompareTo(y.Id) : y.Id.CompareTo(x.Id);
    }
}
