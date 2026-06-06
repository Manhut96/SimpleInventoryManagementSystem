using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;
using SimpleInventoryManagementSystem.Application.Orders.Commands.CreateOrder;

namespace SimpleInventoryManagementSystem.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>Places a new order, deducts stock, and applies discounts and location-based pricing.</summary>
    [HttpPost]
    [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request.CustomerId, request.Items);
        var result = await mediator.Send(command);
        return Created("", result);
    }
}
