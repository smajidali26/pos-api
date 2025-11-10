using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Sizes.Commands.UpdateSize;

public record UpdateSizeCommand(
    Guid Id,
    string Name,
    string Description
) : ICommand<SizeDto>;
