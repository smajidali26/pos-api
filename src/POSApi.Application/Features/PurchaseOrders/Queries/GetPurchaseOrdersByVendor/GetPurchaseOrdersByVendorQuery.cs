using AutoMapper;
using FluentValidation;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrdersByVendor;

public class GetPurchaseOrdersByVendorQuery : IQuery<IEnumerable<PurchaseOrderDto>>
{
    public Guid VendorId { get; set; }

    public GetPurchaseOrdersByVendorQuery(Guid vendorId)
    {
        VendorId = vendorId;
    }
}

public class GetPurchaseOrdersByVendorQueryValidator : AbstractValidator<GetPurchaseOrdersByVendorQuery>
{
    public GetPurchaseOrdersByVendorQueryValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required");
    }
}

public class GetPurchaseOrdersByVendorQueryHandler : IQueryHandler<GetPurchaseOrdersByVendorQuery, IEnumerable<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPurchaseOrdersByVendorQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> Handle(GetPurchaseOrdersByVendorQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrders = await _unitOfWork.PurchaseOrders.GetByVendorIdAsync(request.VendorId, cancellationToken);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
    }
}