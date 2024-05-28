using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_Askon_Smirkin
{
    public partial class Add_Atributes : Form
    {
        DataBase dataBase = new DataBase();
        public Add_Atributes()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataBase.openConnection();

            var id_obj = textBox1_idobj.Text;
            var name = textBox2_name.Text;
            var value = textBox3_value.Text;

            if (!string.IsNullOrWhiteSpace(id_obj) && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
            {
                // Use parameterized query to avoid SQL injection
                var addQuery = "INSERT INTO Attributes (objectId, name, value) VALUES (@id_obj, @name, @value)";
                using (var command = new SqlCommand(addQuery, dataBase.getConnection()))
                {
                    // Add parameters with values
                    command.Parameters.AddWithValue("@id_obj", id_obj);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@value", value);

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

            dataBase.closeConnection();
        }
    }
}
