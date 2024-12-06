using CleanArchitecture.Northwind.Application.Features.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Northwind.Application.Features.TodoLists.Commands.DeleteTodoList;
using CleanArchitecture.Northwind.Application.Features.TodoLists.Commands.UpdateTodoList;
using CleanArchitecture.Northwind.Application.Features.TodoLists.Queries.GetTodos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
public class TodoListsController : ApiController
{
    private readonly ISender _sender;

    public TodoListsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<TodosVm>> GetTodoLists()
    {
        return await _sender.Send(new GetTodosQuery());
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<int>> CreateTodoList([FromBody] CreateTodoListCommand command)
    {
        return await _sender.Send(command);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTodoList(int id, [FromBody] UpdateTodoListCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _sender.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTodoList(int id)
    {
        await _sender.Send(new DeleteTodoListCommand(id));
        return NoContent();
    }
}
