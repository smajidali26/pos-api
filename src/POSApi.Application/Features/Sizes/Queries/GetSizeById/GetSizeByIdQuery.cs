using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Sizes.Queries.GetSizeById;

public record GetSizeByIdQuery(Guid Id) : IQuery<SizeDto?>;
