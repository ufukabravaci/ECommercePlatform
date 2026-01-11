using ECommercePlatform.MvcAdmin.DTOs;

namespace ECommercePlatform.MvcAdmin.Models;

public class CustomerListViewModel
{
    public List<CustomerDto> Customers { get; set; } = new();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public string Search { get; set; } = string.Empty;
}