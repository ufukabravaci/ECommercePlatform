using ECommercePlatform.MvcAdmin.DTOs;

namespace ECommercePlatform.MvcAdmin.Models;

public class CustomerListViewModel
{
    public PageResult<CustomerDto> Customers { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; } // "active", "inactive", or null for all
}