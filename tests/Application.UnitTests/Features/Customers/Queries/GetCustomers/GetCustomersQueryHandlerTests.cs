using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandlerTests
{
    [Test]
    public async Task HandleShouldMatchTrimmedKeywordAgainstPhone()
    {
        var result = await HandleAsync(new GetCustomersQuery
        {
            Keyword = "  555-0100  ",
            PageNumber = 1,
            PageSize = 10
        });

        result.Succeeded.ShouldBeTrue();
        result.Data.Customers.Items.Select(customer => customer.Id).ShouldBe(["ALFKI"]);
    }

    [Test]
    public async Task HandleShouldLimitCityOptionsToSelectedCountry()
    {
        var result = await HandleAsync(new GetCustomersQuery
        {
            Country = "Germany",
            City = "Berlin",
            PageNumber = 1,
            PageSize = 10
        });

        result.Succeeded.ShouldBeTrue();
        result.Data.Customers.Items.Select(customer => customer.Id).ShouldBe(["ALFKI"]);
        result.Data.Countries.ShouldBe(["France", "Germany", "Mexico"]);
        result.Data.Cities.ShouldBe(["Berlin", "Munich"]);
    }

    [Test]
    public async Task HandleShouldUseIdToStabilizeCompanyNameSort()
    {
        var result = await HandleAsync(new GetCustomersQuery
        {
            SortBy = CustomerSortField.CompanyName,
            PageNumber = 1,
            PageSize = 2
        });

        result.Succeeded.ShouldBeTrue();
        result.Data.Customers.Items.Select(customer => customer.Id).ShouldBe(["ALFKI", "ANATR"]);
    }

    private static Task<CleanArchitecture.Northwind.Application.Common.Models.Result<CustomersDto>> HandleAsync(
        GetCustomersQuery query)
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Customers).Returns(CreateDbSet(new[]
        {
            new Customer
            {
                Id = "ALFKI",
                CompanyName = "Alfreds Futterkiste",
                ContactName = "Maria Anders",
                Country = "Germany",
                City = "Berlin",
                Phone = "555-0100"
            },
            new Customer
            {
                Id = "ANATR",
                CompanyName = "Same Name",
                ContactName = "Ana Trujillo",
                Country = "Mexico",
                City = "Mexico D.F.",
                Phone = "555-0101"
            },
            new Customer
            {
                Id = "ANTON",
                CompanyName = "Same Name",
                ContactName = "Antonio Moreno",
                Country = "Mexico",
                City = "Mexico D.F.",
                Phone = "555-0102"
            },
            new Customer
            {
                Id = "BLAUS",
                CompanyName = "Zebra Delikatessen",
                Country = "Germany",
                City = "Munich"
            },
            new Customer
            {
                Id = "BONAP",
                CompanyName = "Zeta app",
                Country = "France",
                City = "Paris"
            }
        }));

        return new GetCustomersQueryHandler(context.Object).Handle(query, CancellationToken.None);
    }

    private static DbSet<T> CreateDbSet<T>(IEnumerable<T> source)
        where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();

        dbSet.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(x => x.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator())
            .Returns(() => queryable.GetEnumerator());

        return dbSet.Object;
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner)
        : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType)
                .Invoke(inner, [expression]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [executionResult])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(Expression expression)
        : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}