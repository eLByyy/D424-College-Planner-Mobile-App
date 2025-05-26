using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SQLite;
using src.Models;
using src.Views;

namespace src.Services
{
    public class DatabaseService
    {
        private static SQLiteAsyncConnection _db;
        public static string _dbPath;
        
        public static void Initialize(string databasePath)
        {
            _dbPath = databasePath;
        }


        //Create SQLite Database tables
        static async Task Init()
        {
            if (_db != null)
                return;

            if (string.IsNullOrEmpty(_dbPath))
            {
                throw new InvalidOperationException("DatabaseService.Initialize(path) must be called before use.");
            }

            _db = new SQLiteAsyncConnection(_dbPath);

            await _db.CreateTableAsync<Terms>();
            await _db.CreateTableAsync<Courses>();
            await _db.CreateTableAsync<Assessments>();
            await _db.CreateTableAsync<Users>();
        }

        #region TermsMethods
        
        public static async Task AddTerm(string name, DateTime startDate, DateTime endDate)
        {
            await Init();
            var term = new Terms()
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                UserId = Session.CurrentUser.Id
            };
            await _db.InsertAsync(term);
            var id = term.Id; //Returns the TermId.
        }

        public static async Task RemoveTerm(int id)
        {
            await Init();
            var courses = await _db.Table<Courses>().Where(c => c.TermId == id).ToListAsync();

            foreach (var course in courses)
            {
                var assessments = await _db.Table<Assessments>().Where(a => a.CourseId == course.Id).ToListAsync();

                foreach (var assessment in assessments)
                {
                    await RemoveAssessment(assessment.Id);
                }

                await _db.DeleteAsync(course);
            }
            await _db.DeleteAsync<Terms>(id);
        }

        public static async Task<IEnumerable<Terms>> GetTerms()
        {
            await Init();
            return await _db.Table<Terms>()
                .Where(t => t.UserId == Session.CurrentUser.Id)
                .ToListAsync();
        }

        public static async Task<Terms> GetTermById(int id)
        {
            await Init();
            return await _db.Table<Terms>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public static async Task UpdateTerm(int id, string name, DateTime startDate, DateTime endDate)
        {
            await Init();
            var termQuery = await _db.Table<Terms>()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
            if (termQuery != null)
            {
                termQuery.Name = name;
                termQuery.StartDate = startDate;
                termQuery.EndDate = endDate;
                await _db.UpdateAsync(termQuery);
            }
        }

        #endregion

        #region CoursesMethods

        public static async Task<int> AddCourse(int termId, string name, string status, DateTime startDate, DateTime endDate, 
            string instructorName, string instructorPhone, string instructorEmail, string notes )
        {
            await Init();
            var course = new Courses
            {
                TermId = termId,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                InstructorName = instructorName,
                InstructorPhone = instructorPhone,
                InstructorEmail = instructorEmail,
                Notes = notes,
                UserId = Session.CurrentUser.Id
            };

            await _db.InsertAsync(course);
            return course.Id; //Returns the CourseId.
        }

        public static async Task RemoveCourse(int id)
        {
            await Init();

            //Remove any related assessments first
            var relatedAssessments = await _db.Table<Assessments>().Where(a => a.CourseId == id).ToListAsync();
            foreach (var assessment in relatedAssessments)
            {
                await _db.DeleteAsync(assessment);
            }

            await _db.DeleteAsync<Courses>(id);
        }

        public static async Task<List<Courses>> GetAllCourses()
        {
            await Init();
            return await _db.Table<Courses>()
                .Where(c => c.UserId == Session.CurrentUser.Id)
                .ToListAsync();
        }


        public static async Task<IEnumerable<Courses>> GetCourses(int termId)
        {
            await Init();
            return await _db.Table<Courses>()
                .Where(i => i.TermId == termId && i.UserId == Session.CurrentUser.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Courses>> GetCourses() //Overloaded method for use with notifications.
        {
            await Init();
            return await _db.Table<Courses>()
                .Where(c => c.UserId == Session.CurrentUser.Id)
                .ToListAsync();
        }

        public static async Task<Courses> GetCourseById(int id)
        {
            await Init();
            return await _db.Table<Courses>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public static async Task UpdateCourse(int id, string name, string status, DateTime startDate, DateTime endDate,
            string instructorName, string instructorPhone, string instructorEmail, bool notificationStart, string notes, bool assessmentNotification )
        {
            await Init();

            var coursesQuery = await _db.Table<Courses>()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
            if (coursesQuery != null)
            {
                coursesQuery.Name = name;
                coursesQuery.StartDate = startDate;
                coursesQuery.EndDate = endDate;
                coursesQuery.Status = status;
                coursesQuery.InstructorName = instructorName;
                coursesQuery.InstructorPhone = instructorPhone;
                coursesQuery.InstructorEmail = instructorEmail;                
                coursesQuery.StartNotification = notificationStart;
                coursesQuery.Notes = notes;
                coursesQuery.AssessmentNotification = assessmentNotification;
                await _db.UpdateAsync(coursesQuery);
            }
        }

        public static async Task<int> GetCourseCountForTerm(int termId)
        {
            await Init();
            return await _db.Table<Courses>().Where(c => c.TermId == termId).CountAsync();
        }

        #endregion

        #region AssessmentsMethods

        public static async Task AddAssessment(int courseId, string type, string name, DateTime startDate, DateTime endDate, bool notificationEnabled)
        {
            await Init();
            var assessment = new Assessments
            {
                CourseId = courseId,
                AssessmentType = type,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                NotificationEnabled = notificationEnabled,
                UserId = Session.CurrentUser.Id
            };
            await _db.InsertAsync(assessment);
        }

        public static async Task<IEnumerable<Assessments>> GetAssessmentsForCourse(int courseId)
        {
            await Init();
            return await _db.Table<Assessments>()
                .Where(a => a.CourseId == courseId && a.UserId == Session.CurrentUser.Id)
                .ToListAsync();
        }

        public static async Task RemoveAssessment(int assessmentId)
        {
            await Init();

            var assessmentToRemove = await _db.Table<Assessments>().Where(a => a.Id == assessmentId).FirstOrDefaultAsync();

            if (assessmentToRemove != null)
            {
                await _db.DeleteAsync(assessmentToRemove);
            }
        }

        public static async Task UpdateAssessment(Assessments updatedAssessment)
        {
            await Init();

            var existingAssessment = await _db.Table<Assessments>()
                .Where(a => a.Id == updatedAssessment.Id)
                .FirstOrDefaultAsync();
            if (existingAssessment != null)
            {
                existingAssessment.Name = updatedAssessment.Name;
                existingAssessment.StartDate = updatedAssessment.StartDate;
                existingAssessment.EndDate = updatedAssessment.EndDate;
                existingAssessment.AssessmentType = updatedAssessment.AssessmentType;
                existingAssessment.NotificationEnabled = updatedAssessment.NotificationEnabled;

                await _db.UpdateAsync(existingAssessment);
            }
        }
        

        #endregion

        #region UsersMethods

        public static async Task<bool> CreateUser(string username, string password, string role = "Student")
        {
            await Init();

            var existingUser = await _db.Table<Users>().Where(u => u.Username == username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return false;
            }

            var users = new Users
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role //Scalable for if/when Instructor user type is implemented.
            };

            await _db.InsertAsync(users);
            return true;

        }

        public static async Task<bool> ValidateUser(string username, string password)
        {
            await Init();

            var user = await _db.Table<Users>().Where(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public static async Task<Users?> GetUserByUsername(string username)
        {
            await Init();
            return await _db.Table<Users>().Where(u => u.Username == username).FirstOrDefaultAsync();

        }
       
        #endregion
        
        #region DemoData

        public static async Task<bool> LoadSampleData()
        {
            await Init();

            var sampleUser = await _db.Table<Users>().Where(u => u.Username =="sampleuser").FirstOrDefaultAsync();
            if (sampleUser == null)
            {
                await CreateUser("sampleuser", "password123");
                sampleUser = await _db.Table<Users>().Where(u => u.Username == "sampleuser").FirstOrDefaultAsync();
            }

            Session.CurrentUser = sampleUser;
          
            var term1 = new Terms
            {
                Name = "Term 1",
                StartDate = new DateTime(2025, 3, 1),
                EndDate = new DateTime(2025, 9, 1),
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            var term2 = new Terms
            {
                Name = "Term 2",
                StartDate = new DateTime(2025, 9, 2),
                EndDate = new DateTime(2026, 3, 1),
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            await _db.InsertAsync(term1);
            await _db.InsertAsync(term2);

            var course1 = new Courses
            {
                TermId = term1.Id,
                Name = "Software Engineering",
                StartDate = new DateTime(2025, 3, 1),
                EndDate = new DateTime(2025, 4, 1),
                Status = "Completed",
                InstructorName = "Anika Patel",
                InstructorPhone = "555-123-4567",
                InstructorEmail = "anika.patel@wgu.edu",
                Notes = "Intro to Software Engineering",
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            var course2 = new Courses
            {
                TermId = term1.Id,
                Name = "Mobile App Development",
                StartDate = new DateTime(2025, 4, 2),
                EndDate = new DateTime(2025, 5, 1),
                Status = "In Progress",
                InstructorName = "Anika Patel",
                InstructorPhone = "555-123-4567",
                InstructorEmail = "anika.patel@wgu.edu",
                Notes = "Focus on .NET MAUI development.",
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            var course3 = new Courses
            {
                TermId = term2.Id,
                Name = "Database Design",
                StartDate = new DateTime(2025, 9, 1),
                EndDate = new DateTime(2025, 12, 15),
                Status = "Planned",
                InstructorName = "Ernest Strong",
                InstructorPhone = "555-222-3333",
                InstructorEmail = "estrong@wgu.edu",
                Notes = "",
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            var course4 = new Courses
            {
                TermId = term1.Id,
                Name = "Software Design",
                StartDate = new DateTime(2025, 5, 5),
                EndDate = new DateTime(2025, 6, 3),
                Status = "Planned",
                InstructorName = "Anika Patel",
                InstructorPhone = "555-123-4567",
                InstructorEmail = "anika.patel@wgu.edu",
                Notes = "Emphasis on architectural patterns and UML.",
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            var course5 = new Courses
            {
                TermId = term1.Id,
                Name = "Database Management",
                StartDate = new DateTime(2025, 6, 10),
                EndDate = new DateTime(2025, 7, 9),
                Status = "Not Started",
                InstructorName = "Ernest Strong",
                InstructorPhone = "555-222-3333",
                InstructorEmail = "estrong@wgu.edu",
                Notes = "Learn SQL, ERDs, and relational database concepts.",
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            var course6 = new Courses
            {
                TermId = term1.Id,
                Name = "Web Dev Foundations",
                StartDate = new DateTime(2025, 7, 15),
                EndDate = new DateTime(2025, 8, 13),
                Status = "Not Started",
                InstructorName = "Ethan Johnson",
                InstructorPhone = "555-456-7890",
                InstructorEmail = "ejohnson@wgu.edu",
                Notes = "Covers HTML, CSS, and JavaScript basics.",
                UserId = Session.CurrentUser.Id,
                IsSampleData = true
            };

            await _db.InsertAsync(course1);
            await _db.InsertAsync(course2);
            await _db.InsertAsync(course3);
            await _db.InsertAsync(course4);
            await _db.InsertAsync(course5);
            await _db.InsertAsync(course6);

            return true;
        }

        public static async Task ClearDatabase()
        {
            await _db.DeleteAllAsync<Terms>();
            await _db.DeleteAllAsync<Courses>();
            await _db.DeleteAllAsync<Assessments>();
            await _db.DeleteAllAsync<Users>();
        }

        public static async Task<bool> ClearSampleData()
        {
            await Init();

            var sampleTerms = await _db.Table<Terms>().Where(t => t.IsSampleData).ToListAsync();
            var sampleCourses = await _db.Table<Courses>().Where(c => c.IsSampleData).ToListAsync();

            if (!sampleTerms.Any() && !sampleCourses.Any())
            {
                return false; //No sample data found
            }

            foreach (var term in sampleTerms)
            {
                await _db.DeleteAsync(term);
            }

            foreach(var course in sampleCourses)
            {
                await _db.DeleteAsync(course);
            }

            return true;
                       
        }



        #endregion

        #region TestMethods

        public static async Task UnitTestInitializeDB()
        {
            await Init();
        }

        public static async Task CloseConnection()
        {
            if (_db != null)
            {
                await _db.CloseAsync();
                _db = null;
            }
        }

        #endregion

    }
}
