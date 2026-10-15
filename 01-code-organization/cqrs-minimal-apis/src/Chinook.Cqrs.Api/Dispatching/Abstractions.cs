namespace Chinook.Cqrs.Api.Dispatching;

/// <summary>A request that changes state. Handled by exactly one <see cref="ICommandHandler{TCommand}"/>.</summary>
public interface ICommand;

/// <summary>A command that hands back a result, typically the id of what it created.</summary>
public interface ICommand<TResult> : ICommand;

/// <summary>A request that reads state and never changes it.</summary>
public interface IQuery<TResult>;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
