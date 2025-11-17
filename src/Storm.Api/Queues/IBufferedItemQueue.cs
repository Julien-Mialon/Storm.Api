namespace Storm.Api.Queues;

public interface IBufferedItemQueue<TWorkItem> : IItemQueue<TWorkItem, TWorkItem[]>;