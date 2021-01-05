using System;
using System.Collections.Generic;
using System.Data;
using Dochazka.HelperClasses;
using Dochazka.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dochazka.Controllers;
using System.Linq;

namespace Dochazka.Tests.UnitTests
{
    [TestClass]
    public class DataTableExtensionsTests
    {
        List<AttendanceRecord> attendanceRecordsAsList;

        [TestInitialize]
        public void SetupTest()
        {            
            attendanceRecordsAsList = new List<AttendanceRecord>() {
                        new AttendanceRecord { WorkDay = new DateTime(2020,01,01),
                              MorningAttendance = Attendance.Absence,
                              AfternoonAttendance = Attendance.DoctorSickness,
                              ManagerApprovalStatus = ManagerApprovalStatus.Rejected,
                              Employee = new Areas.Identity.Data.ApplicationUser { FirstName = "Karel", LastName = "Vomacka", UserName = "karel.vomacka@email.com" }  
                             },
                        new AttendanceRecord { WorkDay = new DateTime(2020,01,02),
                              MorningAttendance = Attendance.LegalJustification,
                              AfternoonAttendance = Attendance.Sickleave,
                              ManagerApprovalStatus = ManagerApprovalStatus.Approved,
                              Employee = new Areas.Identity.Data.ApplicationUser  { FirstName = "Tomas", LastName = "Vomacka", UserName = "tomas.vomacka@email.com" }
                             }
            };
        }

        [TestMethod]
        public void WriteToCsvString()
        {
            // 1. Arrange              
            string expectedResult = System.IO.File.ReadAllText(@"TestResources\WriteToCsvStringTest.csv");
            DataTable dt = AttendanceRecordsController.GetAttendanceRecordsAsDataTable(attendanceRecordsAsList.AsQueryable());

            // 2. Act
            string actualResult = dt.WriteToCsvString();

            // 3. Assert
            Assert.AreEqual(expectedResult,actualResult);
        }
    }
}
