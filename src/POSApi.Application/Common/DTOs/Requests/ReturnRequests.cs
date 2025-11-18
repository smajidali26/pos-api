using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Requests;

public class ProcessReturnRequest
{
    public RefundMethod RefundMethod { get; set; }
    public string Notes { get; set; } = string.Empty;
}
