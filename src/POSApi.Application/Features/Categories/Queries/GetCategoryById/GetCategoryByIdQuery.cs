using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQuery : IQuery<CategoryDto?>
{
    public Guid Id { get; set; }

    public GetCategoryByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }
}