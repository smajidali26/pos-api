using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Domain.Entities;

namespace POSApi.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.Size != null ? src.Size.Name : null))
            .ForMember(dest => dest.PrimaryVendorName, opt => opt.MapFrom(src => src.PrimaryVendor != null ? src.PrimaryVendor.Name : null))
            .ForMember(dest => dest.TotalBaseUnitQuantity, opt => opt.MapFrom(src => src.GetTotalBaseUnitQuantity()))
            .ForMember(dest => dest.StockDisplayString, opt => opt.MapFrom(src => src.GetStockDisplayString()))
            .ForMember(dest => dest.InventoryValue, opt => opt.MapFrom(src => src.GetInventoryValue()))
            .ForMember(dest => dest.WeightUnitSymbol, opt => opt.MapFrom(src => src.Unit != null && src.Unit.WeightUnit != null ? src.Unit.WeightUnit.Symbol : null))
            .ForMember(dest => dest.VolumeUnitSymbol, opt => opt.MapFrom(src => src.Unit != null && src.Unit.VolumeUnit != null ? src.Unit.VolumeUnit.Symbol : null))
            .ForMember(dest => dest.NeedsReorder, opt => opt.MapFrom(src => src.NeedsReorder))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
            .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.IsOutOfStock))
            .ForMember(dest => dest.IsWeightBased, opt => opt.MapFrom(src => src.IsWeightBased))
            .ForMember(dest => dest.IsVolumeBased, opt => opt.MapFrom(src => src.IsVolumeBased))
            .ForMember(dest => dest.IsCountBased, opt => opt.MapFrom(src => src.IsCountBased));

        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Size, opt => opt.Ignore())
            .ForMember(dest => dest.PrimaryVendor, opt => opt.Ignore())
            .ForMember(dest => dest.Unit, opt => opt.Ignore());

        // ProductUnit mappings
        CreateMap<ProductUnit, ProductUnitDto>()
            .ForMember(dest => dest.SellingUnit, opt => opt.MapFrom(src => src.PackagingUnit != null ? src.PackagingUnit : src.BaseUnit))
            .ForMember(dest => dest.QuantityPerSellingUnit, opt => opt.MapFrom(src => src.HasPackaging ? src.PackagingQuantity.Value : src.BaseQuantity))
            .ForMember(dest => dest.HasPhysicalWeight, opt => opt.MapFrom(src => src.HasPhysicalWeight))
            .ForMember(dest => dest.HasPhysicalVolume, opt => opt.MapFrom(src => src.HasPhysicalVolume));

        // UnitOfMeasure mappings
        CreateMap<UnitOfMeasure, UnitOfMeasureDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.UnitType != null ? src.UnitType.Name : string.Empty));

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.SubCategoryCount, opt => opt.MapFrom(src => src.SubCategories.Count))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => p.IsActive)));

        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count))
            .ForMember(dest => dest.TotalSpent, opt => opt.MapFrom(src => src.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount)))
            .ForMember(dest => dest.LastOrderDate, opt => opt.MapFrom(src => src.Orders.OrderByDescending(o => o.OrderDate).FirstOrDefault() != null ? src.Orders.OrderByDescending(o => o.OrderDate).First().OrderDate : (DateTime?)null));

        CreateMap<CustomerDto, Customer>()
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}".Trim() : "Walk-in Customer"))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems.Count))
            .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.FullName : string.Empty))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()));

        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Cashier, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Product != null && src.Product.Unit != null 
                ? (src.Product.Unit.PackagingUnit != null ? src.Product.Unit.PackagingUnit.Symbol 
                   : src.Product.Unit.BaseUnit != null ? src.Product.Unit.BaseUnit.Symbol 
                   : string.Empty) 
                : string.Empty));

        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        // Vendor mappings
        CreateMap<Vendor, VendorDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.PaymentTermsName, opt => opt.MapFrom(src => src.PaymentTerms.ToString()))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count))
            .ForMember(dest => dest.PurchaseOrderCount, opt => opt.MapFrom(src => src.PurchaseOrders.Count));

        CreateMap<VendorDto, Vendor>()
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore());

        // Return mappings
        CreateMap<Return, ReturnDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OriginalOrder != null ? src.OriginalOrder.OrderNumber : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}".Trim() : "Unknown"))
            .ForMember(dest => dest.ProcessedByName, opt => opt.MapFrom(src => src.ProcessedBy != null ? src.ProcessedBy.FullName : string.Empty))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.ReturnItems.Count))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src => src.Reason.ToString()));

        CreateMap<ReturnDto, Return>()
            .ForMember(dest => dest.OriginalOrder, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ReturnItems, opt => opt.Ignore());

        // Promotion mappings
        CreateMap<Promotion, PromotionDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FullName : string.Empty))
            .ForMember(dest => dest.IsCurrentlyActive, opt => opt.MapFrom(src => src.IsValidForUse(DateTime.UtcNow)))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.EndDate < DateTime.UtcNow))
            .ForMember(dest => dest.PercentageUsed, opt => opt.MapFrom(src => src.UsageLimit.HasValue && src.UsageLimit.Value > 0 ? (decimal)src.UsageCount / src.UsageLimit.Value * 100 : 0));
        
        CreateMap<PromotionDto, Promotion>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.PromotionProducts, opt => opt.Ignore())
            .ForMember(dest => dest.PromotionCategories, opt => opt.Ignore())
            .ForMember(dest => dest.PromotionUsages, opt => opt.Ignore());

        // PromotionProduct mappings
        CreateMap<PromotionProduct, PromotionProductDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty));
        
        CreateMap<PromotionProductDto, PromotionProduct>()
            .ForMember(dest => dest.Promotion, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        // PromotionCategory mappings
        CreateMap<PromotionCategory, PromotionCategoryDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
        
        CreateMap<PromotionCategoryDto, PromotionCategory>()
            .ForMember(dest => dest.Promotion, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        // PromotionUsage mappings
        CreateMap<PromotionUsage, PromotionUsageDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null));

        CreateMap<PromotionUsageDto, PromotionUsage>()
            .ForMember(dest => dest.Promotion, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());

        // Inventory mappings
        CreateMap<InventoryMovement, InventoryMovementDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
            .ForMember(dest => dest.MovementTypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.PerformedByName, opt => opt.MapFrom(src => src.MovedBy != null ? src.MovedBy.FullName : string.Empty))
            .ForMember(dest => dest.QuantityChanged, opt => opt.MapFrom(src => src.Quantity));

        // Location mappings
        CreateMap<Location, LocationDto>()
            .ForMember(dest => dest.LocationTypeName, opt => opt.MapFrom(src => src.LocationType.ToString()))
            .ForMember(dest => dest.ParentLocationName, opt => opt.MapFrom(src => src.ParentLocation != null ? src.ParentLocation.Name : null))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.ProductLocations.Count(pl => pl.IsActive)))
            .ForMember(dest => dest.TotalInventoryValue, opt => opt.MapFrom(src => src.ProductLocations.Sum(pl => pl.Quantity * (pl.Product != null ? pl.Product.Cost : 0))));

        // ProductLocation mappings
        CreateMap<ProductLocation, ProductLocationDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty));

        // StockTransfer mappings
        CreateMap<StockTransfer, StockTransferDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.FromLocationName, opt => opt.MapFrom(src => src.FromLocation != null ? src.FromLocation.Name : string.Empty))
            .ForMember(dest => dest.ToLocationName, opt => opt.MapFrom(src => src.ToLocation != null ? src.ToLocation.Name : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy != null ? src.RequestedBy.FullName : string.Empty))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null))
            .ForMember(dest => dest.ShippedByName, opt => opt.MapFrom(src => src.ShippedBy != null ? src.ShippedBy.FullName : null))
            .ForMember(dest => dest.ReceivedByName, opt => opt.MapFrom(src => src.ReceivedBy != null ? src.ReceivedBy.FullName : null));

        // Batch mappings
        CreateMap<Batch, BatchDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.Name : null))
            .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.OrderNumber : null))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RecalledByName, opt => opt.MapFrom(src => src.RecalledBy != null ? src.RecalledBy.FullName : null));

        // SerialNumber mappings
        CreateMap<SerialNumber, SerialNumberDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.BatchNumber : null))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null))
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : null));

        // SerialNumberHistory mappings
        CreateMap<SerialNumberHistory, SerialNumberHistoryDto>()
            .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => src.Action.ToString()))
            .ForMember(dest => dest.PerformedByName, opt => opt.MapFrom(src => src.PerformedBy != null ? src.PerformedBy.FullName : null));

        // StockAlert mappings
        CreateMap<StockAlert, StockAlertDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
            .ForMember(dest => dest.AlertTypeName, opt => opt.MapFrom(src => src.AlertType.ToString()))
            .ForMember(dest => dest.SeverityName, opt => opt.MapFrom(src => src.Severity.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AcknowledgedByName, opt => opt.MapFrom(src => src.AcknowledgedBy != null ? src.AcknowledgedBy.FullName : null))
            .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src => src.ResolvedBy != null ? src.ResolvedBy.FullName : null));

        // InventoryValuation mappings
        CreateMap<InventoryValuation, InventoryValuationDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.MethodName, opt => opt.MapFrom(src => src.Method.ToString()));

        // InventoryValuationLayer mappings
        CreateMap<InventoryValuationLayer, InventoryValuationLayerDto>();
    }
}