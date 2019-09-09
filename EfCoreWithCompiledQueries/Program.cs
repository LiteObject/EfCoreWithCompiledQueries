namespace EfCoreWithCompiledQueries
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Based on https://cmatskas.com/improve-ef-core-performance-with-compiled-queries/ 
    /// </summary>
    internal class Program
    {
        /*         
         * It's important to clarify that EF Core already automatically compiles and caches your queries
         * using a hashed representation of the query expression. When your code needs to reuse a previously
         * executed query, EF Core uses the hash to lookup and return the compiled query from the cache.
         * However, you may want to bypass the computation of the hash and the cache lookup using compiled
         * queries directly. This can be achieved by using the newly exposed extension method in the EF
         * static class: EF.CompileQuery(), EF.CompileAsyncQuery()
         */
         
        /// <summary>
        /// The user ids.
        /// </summary>
        private static int[] userIds;

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            /*******************************************************
             * Make sure to apply migration before running this app
             *******************************************************/
            userIds = GetUserIds(1000);
            
            RunTest(
                ids =>
                    {
                        using (var db = new UserDbContext())
                        {
                            foreach (var id in userIds)
                            {
                                var user = db.Users.Single(c => c.Id == id);
                            }
                        }
                    },
                "Regular Query");

            RunTest(
                async ids =>
                    {
                        var query = EF.CompileAsyncQuery((UserDbContext db, int id)
                            => db.Users.Single(c => c.Id == id));

                        using (var db = new UserDbContext())
                        {
                            foreach (var id in userIds)
                            {
                                // var user = db.Users.Single(c => c.Id == id);
                                var user = await query(db, id); // <-- Amazing performance
                            }
                        }
                    },
                "Async Comp Qry");

            RunTest(
                ids =>
                    {
                        var query = EF.CompileQuery((UserDbContext db, int id)
                            => db.Users.Single(c => c.Id == id));

                        using (var db = new UserDbContext())
                        {
                            foreach (var id in userIds)
                            {
                                // var user = db.Users.Single(c => c.Id == id);
                                var user = query(db, id);
                            }
                        }
                    },
                "Compiled Query");
            
            Console.WriteLine("\n\nPress any key to exit.");
            Console.Read();
        }

        /// <summary>
        /// The run test.
        /// </summary>
        /// <param name="test">
        /// The test.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        private static void RunTest(Action<int[]> test, string name)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            test(userIds);

            stopwatch.Stop();

            Console.WriteLine($"{name}:\t\t{stopwatch.ElapsedMilliseconds.ToString().PadLeft(4)} ms");
        }

        /// <summary>
        /// The get user ids.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="int[]"/>.
        /// </returns>
        private static int[] GetUserIds(int count)
        {
            var userIds = new int[count];

            for (var i = 1; i <= count; i++)
            {
                userIds[i - 1] = i;
            }

            return userIds;
        }
    }
}
