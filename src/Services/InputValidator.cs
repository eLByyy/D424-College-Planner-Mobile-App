
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using src.Models;

namespace src.Services
{
    public static class InputValidator
    {
        public static string ValidateCourse(string courseName, string status, DateTime startDate, DateTime endDate, string instructorName, string instructorPhone, string instructorEmail)
        {
            if (string.IsNullOrWhiteSpace(courseName))
                return "Course Name cannot be empty.";

            if (string.IsNullOrWhiteSpace(status))
                return "Please select a course status.";

            if (endDate < startDate)
                return "End Date cannot be before Start Date.";

            if (startDate == endDate)
                return "Start and end dates cannot be on the same day.";

            if (string.IsNullOrWhiteSpace(instructorName))
                return "Instructor Name cannot be empty.";

            if (string.IsNullOrWhiteSpace(instructorPhone))
                return "Phone Number cannot be empty.";

            if (!IsValidPhoneNumber(instructorPhone))
                return "Invalid Phone Number format. Please use (XXX) XXX-XXXX or XXX-XXX-XXXX.";

            if (string.IsNullOrWhiteSpace(instructorEmail))
                return "Email cannot be empty.";

            if (!IsValidEmail(instructorEmail))
                return "Invalid Email format.";

            return null;
        }

        public static string ValidateTerm(string termName, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(termName))
                return "Term Name cannot be empty.";

            if (endDate < startDate)
                return "End date cannot be before Start Date.";

            if (startDate == endDate)
                return "Start and end dates cannot be on the same day.";

            return null;
        }

        private static bool IsValidPhoneNumber(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{10}$|^\(\d{3}\)\s?\d{3}-\d{4}$|^\d{3}-\d{3}-\d{4}$");
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static string ValidateAssessments(List<Assessments> existingAssessments, string newAssessment)
        {
            bool hasPerformance = existingAssessments.Any(a => a.AssessmentType == "Performance Assessment");
            bool hasObjective = existingAssessments.Any(a => a.AssessmentType == "Objective Assessment");

            if ((newAssessment == "Performance Assessment" && hasPerformance) || 
                (newAssessment == "Objective Assessment" && hasObjective))
            {
                return "You can only have one Performance Assessment and one Objective Assessment.";
            }

            return null;
        }
    }
}
