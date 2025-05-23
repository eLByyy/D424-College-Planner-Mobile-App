using Xunit;
using src.Models;
using src.Services;
using System.Threading.Tasks;

namespace UnitTestProject

{
    public class DBServiceTest
    {
        [Fact]
        public async Task AddCourseAsync_Saves_And_Retrieves_Course()
        {
            //Arrange
            var tempDbPath = Path.GetTempFileName();
            DatabaseService.Initialize(tempDbPath);

            try
            {
                DatabaseService.Initialize(tempDbPath);
                await DatabaseService.UnitTestInitializeDB();

                var user = new Users { Id = 1, Username = "testuser" };
                Session.CurrentUser = user;

                await DatabaseService.CreateUser(user.Username, "pass", "Student");
                await DatabaseService.AddTerm("Test Term", DateTime.Today, DateTime.Today.AddMonths(1));

                //Act
                var courseId = await DatabaseService.AddCourse(
                    termId: 1,
                    name: "Test Course",
                    status: "In Progress",
                    startDate: DateTime.Today,
                    endDate: DateTime.Today.AddMonths(1),
                    instructorName: "Test Instructor",
                    instructorPhone: "222-222-2222",
                    instructorEmail: "test@example.com",
                    notes: "Test notes."
                );

                var result = await DatabaseService.GetCourseById(courseId);

                //Assert
                Assert.NotNull(result);
                Assert.Equal("Test Course", result.Name);
                Assert.Equal(1, result.TermId);
                Assert.Equal(1, result.UserId);
            }
            finally
            {
                await DatabaseService.CloseConnection();

                if (File.Exists(tempDbPath))
                {
                    File.Delete(tempDbPath);
                }
            }
        }
    }
}
