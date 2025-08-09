using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Domain.Entities;

public class TodoList : BaseAuditableEntity<int>
{
    [Key]
    public override int Id { get; set; }

    public string? Title { get; set; }

    public Colour Colour { get; set; } = Colour.White;

    public IList<TodoItem> Items { get; private set; } = new List<TodoItem>();
}
