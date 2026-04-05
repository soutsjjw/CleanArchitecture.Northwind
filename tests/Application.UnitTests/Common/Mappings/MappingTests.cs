using System.Runtime.CompilerServices;
using CleanArchitecture.Northwind.Application.Common.Mappings;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.TodoItems.Queries.GetTodoItemsWithPagination;
using CleanArchitecture.Northwind.Application.Features.TodoLists.Queries.GetTodos;
using CleanArchitecture.Northwind.Domain.Entities;
using Mapster;
using MapsterMapper;
using NUnit.Framework;

namespace CleanArchitecture.Northwind.Application.UnitTests.Common.Mappings;
public class MappingTests
{
    private readonly TypeAdapterConfig _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new TypeAdapterConfig();
        MapsterConfiguration.RegisterMappings(_configuration);
        _configuration.Compile();
        _mapper = new Mapper(_configuration);
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.Compile();
    }

    [Test]
    [TestCase(typeof(TodoList), typeof(TodoListDto))]
    [TestCase(typeof(TodoItem), typeof(TodoItemDto))]
    [TestCase(typeof(TodoList), typeof(LookupDto))]
    [TestCase(typeof(TodoItem), typeof(LookupDto))]
    [TestCase(typeof(TodoItem), typeof(TodoItemBriefDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);
        var mapMethod = typeof(IMapper)
            .GetMethods()
            .Single(method => method.Name == nameof(IMapper.Map)
                && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == 1
                && method.GetParameters().Length == 1
                && method.GetParameters()[0].ParameterType == typeof(object));

        mapMethod.MakeGenericMethod(destination).Invoke(_mapper, new[] { instance });
    }

    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        return RuntimeHelpers.GetUninitializedObject(type);
    }
}
