using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CalorieTracker
{
    public class Connection
    {
        private string connectionString;

        public Connection(string connectionStr)
        {
            connectionString = connectionStr;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection("Data Source=NITRO5\\SQLEXPRESS01;Initial Catalog=OOP_User;Integrated Security=True");
        }
    }
}
