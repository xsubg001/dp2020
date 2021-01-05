using System;
using System.Collections.Generic;
using System.Linq;
using Dochazka.HelperClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dochazka.Tests.UnitTests
{
    [TestClass]
    public class PaginatedListTests
    {
        public List<int> testList;

        [TestInitialize]
        public void SetupTest()
        {
            testList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        }

        [TestMethod]
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
            Assert.AreEqual(expectedResult.Count, actualResult.Count);
            Assert.AreEqual(expectedTotalPages, actualResult.TotalPages);
            Assert.AreEqual(pageIndex, actualResult.PageIndex);
            Assert.IsTrue(actualResult.HasNextPage);
            Assert.IsFalse(actualResult.HasPreviousPage);
            for (int i = 0; i < actualResult.Count; i++)                
            {
                Assert.AreEqual(expectedResult[i], actualResult[i]);
            }            
        }

        [TestMethod]
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
            Assert.AreEqual(expectedResult.Count, actualResult.Count);
            Assert.AreEqual(expectedTotalPages, actualResult.TotalPages);
            Assert.AreEqual(pageNumber, actualResult.PageIndex);
            Assert.IsFalse(actualResult.HasNextPage);
            Assert.IsTrue(actualResult.HasPreviousPage);
            for (int i = 0; i < actualResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i], actualResult[i]);
            }
        }
    }
}
