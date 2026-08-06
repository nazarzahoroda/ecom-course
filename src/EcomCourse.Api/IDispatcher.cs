using System.Windows.Input;

namespace EcomCourse.Api
{
    //public interface IDispatcher
    //{
    //    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    //    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;
    //    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
    //}

    public interface IDispatcher
    {
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
    }

    public class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            dynamic handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"Handler для {command.GetType().Name} не знайдено");
            return await handler.HandleAsync((dynamic)command, cancellationToken);
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            dynamic handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"Handler для {query.GetType().Name} не знайдено");
            return await handler.HandleAsync((dynamic)query, cancellationToken);
        }
    }
}
