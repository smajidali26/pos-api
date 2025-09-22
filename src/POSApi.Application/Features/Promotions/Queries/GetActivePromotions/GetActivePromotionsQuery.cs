using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Promotions.Queries.GetActivePromotions;

public class GetActivePromotionsQuery : IQuery<IEnumerable<PromotionDto>>
{
}

public class GetActivePromotionsQueryHandler : IQueryHandler<GetActivePromotionsQuery, IEnumerable<PromotionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetActivePromotionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PromotionDto>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
    {
        var activePromotions = await _unitOfWork.Promotions.GetActivePromotionsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<PromotionDto>>(activePromotions);
    }
}