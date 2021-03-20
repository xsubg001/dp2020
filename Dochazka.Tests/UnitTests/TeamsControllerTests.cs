using System.Collections.Generic;
using Dochazka.Models;
using Dochazka.Controllers;
using Moq;
using Dochazka.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Dochazka.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Threading.Tasks;
using System.Linq;

namespace Dochazka.Tests.UnitTests
{

    public class TeamsControllerTests
    {
        public List<int> testList;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly Mock<ILogger<TeamsController>> mockLogger;
        private DbContextOptions<ApplicationDbContext> TestContextOptions { get; set; }
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IAuthorizationService> mockAuthorizationService;
        private readonly Mock<IUserStore<ApplicationUser>> mockUserStore;

        public TeamsControllerTests()
        {
            TestContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("TestDB").Options;
            _dbContext = new ApplicationDbContext(TestContextOptions);
            mockLogger = new Mock<ILogger<TeamsController>>();
            mockAuthorizationService = new Mock<IAuthorizationService>();
            mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var mockUserStoreQuearyable = mockUserStore.As<IQueryableUserStore<ApplicationUser>>();
            mockUserStoreQuearyable.Setup(x => x.Users).Returns(GetUsers().AsQueryable());
            userManager = new UserManager<ApplicationUser>(mockUserStoreQuearyable.Object, null, null, null, null, null, null, null, null);            
            Seed();
        }


        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfTeams()
        {
            // Arrange           
            var controller = new TeamsController(mockLogger.Object, _dbContext, mockAuthorizationService.Object, userManager);

            // Act
            var result = await controller.Index(pageNumber: null);

            // Assert

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TeamModel>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }


        [Fact]
        public async Task Details_ReturnsNotFoundObjectResultForIdNull()
        {
            // Arrange           
            var controller = new TeamsController(mockLogger.Object, _dbContext, mockAuthorizationService.Object, userManager);

            // Act
            var result = await controller.Details(id: null);

            // Assert           
            var notFoundObjectResult = Assert.IsType<NotFoundResult>(result);
            Assert.IsType<NotFoundResult>(notFoundObjectResult);            
        }


        [Fact]
        public async Task Details_ReturnsNotFoundObjectResultForInvalidId()
        {
            // Arrange           
            var controller = new TeamsController(mockLogger.Object, _dbContext, mockAuthorizationService.Object, userManager);

            // Act
            var result = await controller.Details(id: 10);

            // Assert           
            var notFoundObjectResult = Assert.IsType<NotFoundResult>(result);            
        }


        [Fact]
        public async Task Details_ReturnsViewResult_TeamDetails()
        {
            // Arrange           
            var controller = new TeamsController(mockLogger.Object, _dbContext, mockAuthorizationService.Object, userManager);

            // Act
            var result = await controller.Details(id: 1);

            // Assert           
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<TeamModel>(viewResult.ViewData.Model);
            Assert.Equal("TestTeam1", model.TeamName);
            Assert.Equal("testmgr021@testmail.com", model.PrimaryManager.UserName);
            Assert.True(viewResult.ViewData.ContainsKey("teamMembers"));
            var teamMembers = Assert.IsType<List<ApplicationUser>>(viewResult.ViewData["teamMembers"]);            
            Assert.Equal(3, teamMembers.Count);
        }


        /// <summary>
        /// Helper method which seeds the InMemoryDatabase with sample test data, i.e. it creates test entries of TeamModel
        /// </summary>
        private void Seed()
        {
            using (var context = new ApplicationDbContext(TestContextOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var team1 = new TeamModel()
                {
                    TeamName = "TestTeam1",
                    TeamModelId = 1,
                    PrimaryManager = new ApplicationUser()
                    {
                        UserName = "testmgr021@testmail.com",
                        Id = "021"
                    },
                    PrimaryManagerId = "021",
                    TeamMembers = new List<ApplicationUser>
                    {
                        new ApplicationUser()
                        {
                            UserName = "teamMember210@testmail.com",
                            Id = "210"
                        },
                        new ApplicationUser()
                        {
                            UserName = "teamMember211@testmail.com",
                            Id = "211"
                        }
                    }

                };

                var team2 = new TeamModel()
                {
                    TeamName = "TestTeam2",
                    TeamModelId = 2,
                    PrimaryManager = new ApplicationUser()
                    {
                        UserName = "testmgr022@testmail.com",
                        Id = "022"
                    },
                    PrimaryManagerId = "022",
                    TeamMembers = new List<ApplicationUser>
                    {
                        new ApplicationUser()
                        {
                            UserName = "teamMember220@testmail.com",
                            Id = "220"
                        },
                        new ApplicationUser()
                        {
                            UserName = "teamMember221@testmail.com",
                            Id = "221"
                        }
                    }
                };

                context.AddRange(team1, team2);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Helper method, which returns sample test data with list of ApplicationUsers
        /// </summary>
        /// <returns> List<ApplicationUser></returns>
        private List<ApplicationUser> GetUsers()
        {
            var users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    UserName = "teamMember210@testmail.com",
                    Id = "210",
                    Team = new TeamModel()
                    {
                        TeamName = "TestTeam1",
                        TeamModelId = 1
                    }
                },
                new ApplicationUser()
                {
                    UserName = "teamMember211@testmail.com",
                    Id = "211",
                    Team = new TeamModel()
                    {
                        TeamName = "TestTeam1",
                        TeamModelId = 1
                    }
                },
                new ApplicationUser()
                {
                    UserName = "teamMember212@testmail.com",
                    Id = "212",
                    Team = new TeamModel()
                    {
                        TeamName = "TestTeam1",
                        TeamModelId = 1
                    }
                },
                new ApplicationUser()
                {
                    UserName = "teamMember220@testmail.com",
                    Id = "220",
                    Team = new TeamModel()
                    {
                        TeamName = "TestTeam2",
                        TeamModelId = 2
                    }
                }
            };

            return users;
        }
    }
}

