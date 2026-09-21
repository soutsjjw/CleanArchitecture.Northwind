using CleanArchitecture.Northwind.Application.Common.Extensions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var inputError = CategoryCommandSupport.ValidateInput(request.CategoryName);
        if (inputError is not null)
        {
            return Result<int>.Failure(inputError, 400);
        }

        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            Description = request.Description.TrimToNull(),
            IsActive = true
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(category.Id);
    }
}
