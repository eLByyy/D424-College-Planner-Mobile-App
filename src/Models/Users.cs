using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Models
{
    public class Users
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, NotNull]
        public string Username { get; set; }
        [NotNull]
        public string PasswordHash { get; set; }
        public string Role {get; set; }       
    }


    //Ready to scale to include "Instructor" and "Student" type "Users"
    public class Instructor : Users
    {
         public string? Department { get; set; }
        public string? Title {get; set; }
    }

    public class Student : Users
    {
        public string? Major {get; set; }
        public int? Year {get; set; }
        public DateTime? EnrollmentDate { get; set; }
    }


}
