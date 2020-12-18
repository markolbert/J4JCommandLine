using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class ContextTests : BaseTest
    {
        [Theory]
        [MemberData(nameof(TestDataSource.GetSinglePropertyData), MemberType=typeof(TestDataSource))]
        public void SinglePropertyAllocations( TestConfig config )
        {
            Initialize( config );
            
            ValidateAllocations();
        }

        [Theory]
        [MemberData(nameof(TestDataSource.GetSinglePropertyData), MemberType = typeof(TestDataSource))]
        public void SinglePropertyParsing(TestConfig config)
        {
            Initialize(config);

            ValidateConfiguration<BasicTarget>();
        }
    }
}