using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategoryDetail;

public sealed record GetCategoryDetailQuery(int Id)
    : IRequest<Result<CategoryDetailDto>>;
