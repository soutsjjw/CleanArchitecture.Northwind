using CleanArchitecture.Northwind.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Northwind.Application.TodoLists.Commands.DeleteTodoList;
using CleanArchitecture.Northwind.Domain.Entities;

using static CleanArchitecture.Northwind.Application.FunctionalTests.Testing;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.TodoLists.Commands;
public class DeleteTodoListTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoListId()
    {
        var command = new DeleteTodoListCommand(99);
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeleteTodoList()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        await SendAsync(new DeleteTodoListCommand(listId));

        var list = await FindAsync<TodoList>(listId);

        list.Should().BeNull();
    }
}
