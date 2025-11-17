namespace Storm.Api.Workers;

public interface IWorker<in TWorkItem> where TWorkItem : class
{
	void Queue(TWorkItem item);
}