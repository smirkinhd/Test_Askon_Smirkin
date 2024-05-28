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
    public partial class AddLinks : Form
    {
        DataBase dataBase = new DataBase();
        public AddLinks()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataBase.openConnection();

            var id_parent = textBox1.Text;
            var id_child = textBox2.Text;

            if (!string.IsNullOrWhiteSpace(id_parent) && !string.IsNullOrWhiteSpace(id_child))
            {
                var addQuery = "INSERT INTO Links (parentId, childId) VALUES (@id_parent, @id_child)";
                using (var command = new SqlCommand(addQuery, dataBase.getConnection()))
                {
                    // Add parameters with values
                    command.Parameters.AddWithValue("@parentId", id_parent);
                    command.Parameters.AddWithValue("@childId", id_child);

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
