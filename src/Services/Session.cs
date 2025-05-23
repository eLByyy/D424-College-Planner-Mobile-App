using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using src.Models;

namespace src.Services
{
    public static class Session
    {
        public static Users? CurrentUser { get; set; }

        public static void Login(Users user)
        {
            CurrentUser = user;
        }

        public static bool IsLoggedIn => CurrentUser != null;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
} 
