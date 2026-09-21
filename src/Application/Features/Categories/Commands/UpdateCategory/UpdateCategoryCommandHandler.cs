using CleanArchitecture.Northwind.Application.Common.Extensions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure("分類編號無效。", 400);
        }

        var inputError = CategoryCommandSupport.ValidateInput(request.CategoryName);
        if (inputError is not null)
        {
            return Result.Failure(inputError, 400);
        }

        var category = await context.Categories.FindAsync([request.Id], cancellationToken);
        if (category is null || category.IsDelete)
        {
            return Result.Failure("找不到分類。", 404);
        }

        category.CategoryName = request.CategoryName.Trim();
        category.Description = request.Description.TrimToNull();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
