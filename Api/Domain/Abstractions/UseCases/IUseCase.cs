using Api.Common;

namespace Api.Domain.Abstractions.UseCases;

public interface IUseCase<TRequest>
{
    Task<Result> Handle(TRequest request);
}

public interface IUseCase<TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request);
}