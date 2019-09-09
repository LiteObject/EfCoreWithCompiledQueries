namespace EfCoreWithCompiledQueries
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class DataHelper
    {
        /// <summary>
        /// The get users.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/>.
        /// </returns>
        public static List<User> GetUsers(int count)
        {
            var users = new List<User>();

            for (var i = 1; i <= count; i++)
            {
                users.Add(new User()
                              {
                                  Id = i, Username = $"user_{i}"
                              });
            }

            return users;
        }
    }
}
