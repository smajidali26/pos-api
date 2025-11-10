using MediatR;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Sizes.Commands.DeleteSize;

public record DeleteSizeCommand(Guid Id) : ICommand<Unit>;
