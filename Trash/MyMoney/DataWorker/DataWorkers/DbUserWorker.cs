using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWorker
{
    public class DbUserWorker
    {
        public static int? GetUserIdByToken(string token)
        {
            using (var context = new Data.DataContext())
            {
                var dbUser = context.Users.SingleOrDefault(x => x.Token == token);
                if (dbUser == null)
                {
                    return null;
                }
                return dbUser.Id;
            }
        }
    }
}
