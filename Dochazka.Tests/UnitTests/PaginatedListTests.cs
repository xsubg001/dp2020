using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dochazka.HelperClasses;
using Xunit;

namespace Dochazka.Tests.UnitTests
{    
    public class PaginatedListTests
    {
        public List<int> testList;
        
        public PaginatedListTests()
        {
            testList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        }

        [Fact]
        public void Create_Page1_PageSize4()
        {
            // 1. Arrange        
            int? pageIndex = 1;
            int pageSize = 4;
            int expectedTotalPages = 3;
            var expectedResult = new List<int> { 1, 2, 3, 4 };

            // 2. Act
            var actualResult = PaginatedList<int>.Create(testList.AsQueryable(), pageIndex ?? 1, pageSize);

            // 3. Assert
            Assert.Equal(expectedResult.Count, actualResult.Count);
            Assert.Equal(expectedTotalPages, actualResult.TotalPages);
            Assert.Equal(pageIndex, actualResult.PageIndex);
            Assert.True(actualResult.HasNextPage);
            Assert.False(actualResult.HasPreviousPage);
            for (int i = 0; i < actualResult.Count; i++)                
            {
                Assert.Equal(expectedResult[i], actualResult[i]);
            }            
        }

        [Fact]
        public void Create_Page3_PageSize4()
        {
            // 1. Arrange        
            int? pageNumber = 3;
            int pageSize = 4;
            int expectedTotalPages = 3;
            var expectedResult = new List<int> { 9, 0 };

            // 2. Act
            var actualResult = PaginatedList<int>.Create(testList.AsQueryable(), pageNumber ?? 1, pageSize);

            // 3. Assert
            Assert.Equal(expectedResult.Count, actualResult.Count);
            Assert.Equal(expectedTotalPages, actualResult.TotalPages);
            Assert.Equal(pageNumber, actualResult.PageIndex);
            Assert.False(actualResult.HasNextPage);
            Assert.True(actualResult.HasPreviousPage);
            for (int i = 0; i < actualResult.Count; i++)
            {
                Assert.Equal(expectedResult[i], actualResult[i]);
            }
        }
    }
}
