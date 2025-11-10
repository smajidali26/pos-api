using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Sizes.Queries.GetAllSizes;

public record GetAllSizesQuery : IQuery<IEnumerable<SizeDto>>;
