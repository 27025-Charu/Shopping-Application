using ShoppingApp.Model;
using System;
using System.Collections.Generic;

namespace ShoppingApp
{
    internal class UserRepository
    {

        Dictionary<string, User> users = new();

        internal Guid Check(string userName)
        {
            throw new NotImplementedException();
        }

        internal User GetUser(string username)
        {
            if (users.TryGetValue(username, out User user))
            {
                return user;
            }

            return null;
        }

        internal bool RegisterUser(User newUser)
        {
            users.Add(newUser.UserName, newUser);
            return true;
        }
    }
}