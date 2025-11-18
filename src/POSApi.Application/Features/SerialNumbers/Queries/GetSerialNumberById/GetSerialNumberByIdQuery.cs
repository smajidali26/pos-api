using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.SerialNumbers.Queries.GetSerialNumberById;

public class GetSerialNumberByIdQuery : IQuery<SerialNumberDto>
{
    public Guid SerialNumberId { get; set; }
}
