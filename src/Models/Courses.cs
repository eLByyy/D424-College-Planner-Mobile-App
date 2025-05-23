using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace src.Models
{
    public class Courses
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TermId { get; set; } //Foreign key from the Terms Class/Table
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string InstructorName { get; set; }
        public string InstructorEmail { get; set; }
        public string InstructorPhone { get; set; }
        public bool StartNotification { get; set; }
        public int? StartNotificationId { get; set; }
        public int? EndNotificationId { get; set; }
        public string Notes { get; set; }
        public bool AssessmentNotification { get; set; }
        public int UserId { get; set; }

        public bool IsSampleData { get; set; } = false;
    }
}
