using CleanArchitecture.Northwind.Application.Features.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Northwind.Application.Features.TodoItems.Commands.DeleteTodoItem;
using CleanArchitecture.Northwind.Application.Features.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Northwind.Domain.Entities;

using static CleanArchitecture.Northwind.Application.FunctionalTests.Testing;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.TodoItems.Commands;
public class DeleteTodoItemTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoItemId()
    {
        var command = new DeleteTodoItemCommand(99);

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeleteTodoItem()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var itemId = await SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "New Item"
        });

        await SendAsync(new DeleteTodoItemCommand(itemId));

        var item = await FindAsync<TodoItem>(itemId);

        item.Should().BeNull();
    }
}
