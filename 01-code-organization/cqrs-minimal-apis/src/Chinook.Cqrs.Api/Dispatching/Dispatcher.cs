namespace Chinook.Cqrs.Api.Dispatching;

/// <summary>
/// A hand-written dispatcher: it finds the one handler registered in DI for a command or query and
/// calls it. That is the whole job. No pipeline behaviors, no notifications, no assembly scanning;
/// add those here when a use case needs them, not before.
/// </summary>
public sealed class Dispatcher(IServiceProvider services)
{
    public Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var wrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(command.GetType());
        var wrapper = (VoidHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        return wrapper.HandleAsync(command, services, cancellationToken);
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        // The caller only knows the result type, so close the wrapper over the command's runtime type once
        // and let it resolve the strongly typed handler.
        var wrapperType = typeof(CommandHandlerWrapper<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var wrapper = (HandlerWrapper<TResult>)Activator.CreateInstance(wrapperType)!;
        return wrapper.HandleAsync(command, services, cancellationToken);
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var wrapperType = typeof(QueryHandlerWrapper<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var wrapper = (HandlerWrapper<TResult>)Activator.CreateInstance(wrapperType)!;
        return wrapper.HandleAsync(query, services, cancellationToken);
    }

    private abstract class VoidHandlerWrapper
    {
        public abstract Task HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken);
    }

    private sealed class CommandHandlerWrapper<TCommand> : VoidHandlerWrapper
        where TCommand : ICommand
    {
        public override Task HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken) =>
            services.GetRequiredService<ICommandHandler<TCommand>>().HandleAsync((TCommand)request, cancellationToken);
    }

    private abstract class HandlerWrapper<TResult>
    {
        public abstract Task<TResult> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken);
    }

    private sealed class CommandHandlerWrapper<TCommand, TResult> : HandlerWrapper<TResult>
        where TCommand : ICommand<TResult>
    {
        public override Task<TResult> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken) =>
            services.GetRequiredService<ICommandHandler<TCommand, TResult>>().HandleAsync((TCommand)request, cancellationToken);
    }

    private sealed class QueryHandlerWrapper<TQuery, TResult> : HandlerWrapper<TResult>
        where TQuery : IQuery<TResult>
    {
        public override Task<TResult> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken) =>
            services.GetRequiredService<IQueryHandler<TQuery, TResult>>().HandleAsync((TQuery)request, cancellationToken);
    }
}
