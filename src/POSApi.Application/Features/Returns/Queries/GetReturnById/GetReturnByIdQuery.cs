using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Returns.Queries.GetReturnById;

public class GetReturnByIdQuery : IQuery<ReturnDto?>
{
    public Guid Id { get; set; }

    public GetReturnByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetReturnByIdQueryHandler : IQueryHandler<GetReturnByIdQuery, ReturnDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReturnByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReturnDto?> Handle(GetReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var return_ = await _unitOfWork.Returns.GetByIdAsync(request.Id, cancellationToken);
        return return_ == null ? null : _mapper.Map<ReturnDto>(return_);
    }
}