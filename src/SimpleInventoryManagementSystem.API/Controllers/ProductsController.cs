using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimpleInventoryManagementSystem.Application.Contracts.Requests;
using SimpleInventoryManagementSystem.Application.Contracts.Responses;
using SimpleInventoryManagementSystem.Application.Products.Commands.CreateProduct;
using SimpleInventoryManagementSystem.Application.Products.Queries.GetProducts;

namespace SimpleInventoryManagementSystem.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns a paginated list of products in the catalog.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    /// <summary>Creates a new product in the catalog.</summary>
    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.Stock);
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetProducts), result);
    }
}
