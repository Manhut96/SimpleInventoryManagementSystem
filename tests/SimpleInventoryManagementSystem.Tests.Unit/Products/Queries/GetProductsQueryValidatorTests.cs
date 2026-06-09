using FluentAssertions;
using FluentValidation.TestHelper;
using SimpleInventoryManagementSystem.Application.Products.Queries.GetProducts;

namespace SimpleInventoryManagementSystem.Tests.Unit.Products.Queries;

public sealed class GetProductsQueryValidatorTests
{
    private readonly GetProductsQueryValidator _sut = new();

    [Fact]
    public void Validate_DefaultValues_ShouldPass()
    {
        var query = new GetProductsQuery();

        var result = _sut.TestValidate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MaxPageSize_ShouldPass()
    {
        var query = new GetProductsQuery(PageNumber: 1, PageSize: 100);

        var result = _sut.TestValidate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageNumberLessThanOne_ShouldFail(int pageNumber)
    {
        var query = new GetProductsQuery(PageNumber: pageNumber, PageSize: 20);

        var result = _sut.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageSizeLessThanOne_ShouldFail(int pageSize)
    {
        var query = new GetProductsQuery(PageNumber: 1, PageSize: pageSize);

        var result = _sut.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(1000000)]
    public void Validate_PageSizeExceedsMaximum_ShouldFail(int pageSize)
    {
        var query = new GetProductsQuery(PageNumber: 1, PageSize: pageSize);

        var result = _sut.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }
}
