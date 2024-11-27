using Asp.Versioning;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Northwind.Application.TodoItems.Commands.DeleteTodoItem;
using CleanArchitecture.Northwind.Application.TodoItems.Commands.UpdateTodoItem;
using CleanArchitecture.Northwind.Application.TodoItems.Commands.UpdateTodoItemDetail;
using CleanArchitecture.Northwind.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
public class TodoItemsController : ApiController
{
    private readonly ISender _sender;

    public TodoItemsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Route("")]
    [Authorize]
    public async Task<ActionResult<PaginatedList<TodoItemBriefDto>>> GetTodoItemsWithPagination([FromQuery] GetTodoItemsWithPaginationQuery query)
    {
        return await _sender.Send(query);
    }

    [HttpPost]
    [Route("")]
    [Authorize]
    public async Task<ActionResult<int>> CreateTodoItem([FromBody] CreateTodoItemCommand command)
    {
        return await _sender.Send(command);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTodoItem(int id, [FromBody] UpdateTodoItemCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command);
        return NoContent();
    }

    [HttpPut]
    [Route("UpdateDetail/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTodoItemDetail(int id, [FromBody] UpdateTodoItemDetailCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command);
        return NoContent();
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
        await _sender.Send(new DeleteTodoItemCommand(id));
        return NoContent();
    }
}
