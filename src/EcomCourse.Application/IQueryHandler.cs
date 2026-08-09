
namespace EcomCourse.Application
{
    internal interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TQuery, TResponse>
    {
        Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
