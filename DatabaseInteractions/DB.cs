using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInteractions
{
    public static class DB
    {
        public static T GetValue<T>(string sql)
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                DbRawSqlQuery<T> rows = db.Database.SqlQuery<T>(sql);
                if (rows.Count() > 1) throw new ArgumentOutOfRangeException("More than 1 value returned");
                T value = rows.FirstOrDefault();
                return value;
                //return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public static IList<T> GetList<T>(string sql)
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                DbRawSqlQuery<T> rows = db.Database.SqlQuery<T>(sql);
                return rows.ToArray();
            }
        }

        public static IList<T> GetTable<T>(string sql)
        {
           
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                DbRawSqlQuery<T> rows = db.Database.SqlQuery<T>(sql);
                return rows.ToArray();
            }

        }


    }
}
