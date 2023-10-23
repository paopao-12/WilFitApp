using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace CalorieTracker
{
    /// <summary>
    /// Interaction logic for DataGridStyle.xaml
    /// </summary>
    public partial class DataGridStyle : Window
    {
        public DataGridStyle()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            // Replace these connection details with your own
            string connectionString = "Data Source=NITRO5\\SQLEXPRESS01;Initial Catalog=OOP_User;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM FoodLogg";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dataGrid.ItemsSource = dataTable.DefaultView;
            }
        }
    }
}