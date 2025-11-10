using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Sizes.Commands.CreateSize;

public record CreateSizeCommand(
    string Name,
    string Description
) : ICommand<SizeDto>;
