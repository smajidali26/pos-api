using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Queries.GetProductBySku;

public class GetProductBySkuQuery : IQuery<ProductDto?>
{
    public string Sku { get; }

    public GetProductBySkuQuery(string sku)
    {
        Sku = sku;
    }
}