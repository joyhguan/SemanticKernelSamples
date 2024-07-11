using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace SemanticKernelSamples.Samples.Planning;

internal class OrderPlugins
{
    [KernelFunction, Description("Get Recent order IDs")]
    public Task<IReadOnlyList<int>> GetRecentOrderIds()
    {
        return Task.FromResult<IReadOnlyList<int>>(new List<int> { 101, 102, 103 });
    }

    [KernelFunction, Description("Get order details by order ID. Returns an object containing the order ID, associated customer ID (which represents a user ID), total amount, and order status.")]
    public Task<object> GetOrderDetails(int id)
    {
        var order = id switch
        {
            101 => new
            {
                Id = 101,
                CustomerID = 7891,
                TotalAmount = 251.88,
                Status = "Shipped",
            },
            102 => new
            {
                Id = 102,
                CustomerID = 7892,
                TotalAmount = 99.99,
                Status = "Delivered",
            },
            103 => new
            {
                Id = 103,
                CustomerID = 7891,
                TotalAmount = 199.99,
                Status = "Pending",
            },
            _ => throw new Exception("Order not found"),
        };

        return Task.FromResult<object>(order);
    }
}