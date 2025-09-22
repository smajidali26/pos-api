using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Products.Queries.GetProductByBarcode;

public class GetProductByBarcodeQuery : IQuery<ProductDto?>
{
    public string Barcode { get; }

    public GetProductByBarcodeQuery(string barcode)
    {
        Barcode = barcode;
    }
}