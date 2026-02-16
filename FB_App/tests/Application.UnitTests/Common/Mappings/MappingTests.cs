using System.Runtime.CompilerServices;
using AutoMapper;
using FB_App.Application.Comments.Queries;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Movies.Queries.GetMovies;
using FB_App.Domain.Entities;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace FB_App.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private ILoggerFactory? _loggerFactory;
    private MapperConfiguration? _configuration;
    private IMapper? _mapper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Minimal logger factory for tests
        _loggerFactory = LoggerFactory.Create(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

        _configuration = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(IApplicationDbContext).Assembly),
            loggerFactory: _loggerFactory);

        _mapper = _configuration.CreateMapper();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        Assert.That(() => _configuration!.AssertConfigurationIsValid(),
            Throws.Nothing);
    }

    [Test]
    [TestCase(typeof(Movie), typeof(MovieDto))]
    [TestCase(typeof(Comment), typeof(CommentDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = GetInstanceOf(source);

        var result = _mapper!.Map(instance, source, destination);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.GetType(), Is.EqualTo(destination));
    }

    private static object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return RuntimeHelpers.GetUninitializedObject(type);
    }


    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _loggerFactory?.Dispose();
    }
}
