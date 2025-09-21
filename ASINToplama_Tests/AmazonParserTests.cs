using ASINToplama_BusinessLayer.Concrete;
using ASINToplama_BusinessLayer.Parsers;
using FluentAssertions;

namespace ASINToplama_Tests
{
    public class AmazonParserTests
    {
        [Fact]
        public void ExtractAsins_Returns_Distinct_NonEmpty()
        {
            var html = @"<div data-component-type='s-search-result' data-asin='B000111'></div>
                         <div data-component-type='s-search-result' data-asin=''></div>
                         <div data-component-type='s-search-result' data-asin='B000222'></div>
                         <div data-component-type='s-search-result' data-asin='B000111'></div>";

            var result = AmazonParser.ExtractAsins(html);

            result.Should().BeEquivalentTo(new[] { "B000111", "B000222" });
        }

        [Fact]
        public void HasNextPage_True_When_Next_Not_Disabled()
        {
            var html = @"<a class='s-pagination-next' href='/s?k=a&page=2'>Next</a>";
            AmazonParser.HasNextPage(html).Should().BeTrue();
        }

        [Fact]
        public void HasNextPage_False_When_Disabled()
        {
            var html = @"<a class='s-pagination-next' aria-disabled='true'>Next</a>";
            AmazonParser.HasNextPage(html).Should().BeFalse();
        }
    }
}
