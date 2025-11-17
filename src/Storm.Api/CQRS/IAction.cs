namespace Storm.Api.CQRS;

public interface IAction<in TParameter, TOutput>
{
	Task<TOutput> Execute(TParameter parameter);
}