using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Test_Askon_Smirkin
{
    
    public partial class Add_Objects : Form
    {
        DataBase database = new DataBase();
        public Add_Objects()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            database.openConnection();

            var type = textBox1_add_type.Text;
            var product = textBox2_add_product.Text;

            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(product))
            {
                // Use parameterized query to avoid SQL injection
                var addQuery = "INSERT INTO Objects (type, product) VALUES (@type, @product)";
                using (var command = new SqlCommand(addQuery, database.getConnection()))
                {
                    // Add parameters with values
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@product", product);

                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Запись создана!");
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Ошибка выполнения запроса: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Не может быть пустых строк!");
            }

            database.closeConnection();
        }
    }
}
