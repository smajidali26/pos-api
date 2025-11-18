using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.PerformanceMetrics.Commands;

public record UpdateCustomerRatingCommand : IRequest<Unit>
{
    public Guid MetricId { get; init; }
    public int RatingCount { get; init; }
    public decimal AverageRating { get; init; }
}

public class UpdateCustomerRatingCommandHandler : IRequestHandler<UpdateCustomerRatingCommand, Unit>
{
    private readonly IPerformanceMetricRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerRatingCommandHandler(
        IPerformanceMetricRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCustomerRatingCommand request, CancellationToken cancellationToken)
    {
        var metric = await _repository.GetByIdAsync(request.MetricId, cancellationToken);
        if (metric == null)
            throw new KeyNotFoundException($"Performance metric with ID {request.MetricId} not found");

        metric.UpdateCustomerSatisfaction(request.RatingCount, request.AverageRating);
        metric.CalculatePerformanceScore(); // Recalculate score with new rating

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
