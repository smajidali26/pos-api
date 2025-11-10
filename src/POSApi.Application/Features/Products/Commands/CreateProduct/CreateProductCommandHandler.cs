using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");
        }

        // Validate size exists (if provided)
        if (request.SizeId.HasValue)
        {
            var size = await _unitOfWork.Sizes.GetByIdAsync(request.SizeId.Value, cancellationToken);
            if (size == null)
            {
                throw new InvalidOperationException($"Size with ID {request.SizeId} not found");
            }
        }

        // Validate SKU is unique
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.SKU, cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists");
        }

        // Validate barcode is unique (if provided)
        if (!string.IsNullOrEmpty(request.Barcode))
        {
            var existingByBarcode = await _unitOfWork.Products.GetByBarcodeAsync(request.Barcode, cancellationToken);
            if (existingByBarcode != null)
            {
                throw new InvalidOperationException($"Product with barcode '{request.Barcode}' already exists");
            }
        }

        // Get base unit
        var baseUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.BaseUnitCode, cancellationToken);
        if (baseUnit == null)
        {
            throw new InvalidOperationException($"Invalid base unit code: {request.BaseUnitCode}");
        }

        // Create product
        var product = new Product(
            request.Name,
            request.Description,
            request.SKU,
            request.Barcode,
            request.Price,
            request.Cost,
            request.StockQuantity,
            request.MinStockLevel,
            request.CategoryId,
            request.SizeId);

        // Set vendor if provided
        if (request.PrimaryVendorId.HasValue)
        {
            var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.PrimaryVendorId.Value, cancellationToken);
            if (vendor != null)
            {
                product.SetPrimaryVendor(request.PrimaryVendorId.Value);
            }
        }

        // Add product first to get the ID
        await _unitOfWork.Products.AddAsync(product, cancellationToken);

        // Create product unit
        ProductUnit productUnit;
        if (!string.IsNullOrEmpty(request.PackagingUnitCode) && request.PackagingQuantity.HasValue)
        {
            var packagingUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.PackagingUnitCode, cancellationToken);
            if (packagingUnit == null)
            {
                throw new InvalidOperationException($"Invalid packaging unit code: {request.PackagingUnitCode}");
            }

            if (baseUnit.UnitTypeId != packagingUnit.UnitTypeId)
            {
                throw new InvalidOperationException("Base unit and packaging unit must be of the same type");
            }

            productUnit = new ProductUnit(
                product.Id, 
                baseUnit.Id, 
                request.BaseQuantity, 
                packagingUnit.Id, 
                request.PackagingQuantity.Value);
        }
        else
        {
            productUnit = new ProductUnit(product.Id, baseUnit.Id, request.BaseQuantity);
        }

        // Set physical properties if provided
        if (request.Weight.HasValue || request.Volume.HasValue)
        {
            UnitOfMeasure? weightUnit = null;
            UnitOfMeasure? volumeUnit = null;

            if (request.Weight.HasValue && !string.IsNullOrEmpty(request.WeightUnitCode))
            {
                weightUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.WeightUnitCode, cancellationToken);
                if (weightUnit == null)
                {
                    throw new InvalidOperationException($"Invalid weight unit code: {request.WeightUnitCode}");
                }
            }

            if (request.Volume.HasValue && !string.IsNullOrEmpty(request.VolumeUnitCode))
            {
                volumeUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.VolumeUnitCode, cancellationToken);
                if (volumeUnit == null)
                {
                    throw new InvalidOperationException($"Invalid volume unit code: {request.VolumeUnitCode}");
                }
            }

            productUnit.UpdatePhysicalProperties(request.Weight, weightUnit?.Id, request.Volume, volumeUnit?.Id);
        }

        await _unitOfWork.ProductUnits.AddAsync(productUnit, cancellationToken);
        product.SetUnit(productUnit);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}