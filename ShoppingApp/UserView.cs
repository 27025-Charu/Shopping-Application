using System;

namespace ShoppingApp
{
    internal class UserView
    {
        private UserService userService;

        public UserView(UserService userService)
        {
            this.userService = userService;
        }

        internal void DisplayMenu()
        {
            Console.WriteLine(@"===========================================
USER AUTHENTICATION
=====================================
[L] LOGIN 
[S] SIGN-UP
(You can click X to exit anytime when the application is in the runnable state.)");

        }
    }
}