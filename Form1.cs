using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Test_Askon_Smirkin
{
    enum RowState
    {
        Existed,
        New,
        Modify,
        ModifyNew,
        Deleted
    }

    public partial class Form1 : Form
    {
        public class ObjectData
        {
            public string ID { get; set; }
            public string Type { get; set; }
            public string Product { get; set; }
        }

        public class LinksData
        {
            public string ID_parent { get; set; }
            public string ID_child { get; set; }
        }

        public class AttributesData
        {
            public string objectID { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }

        DataBase database = new DataBase();
        int selectedRow;
        private bool isModified = false;

        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            dataGreidViewToolStripMenuItem.Checked = true;
            dataGreidViewToolStripMenuItem.Enabled = false;
            save_btn_Objects.Enabled = false;
            add_atr_btn_tree.Visible = false;
            add_obj_btn_tree.Visible = false;
            del_atr_btn_tree.Visible = false;
            del_obj_btn_tree.Visible = false;
            treeView1.Visible = false;
            textBox1_type_txbx_tree.Visible = false;
            textBox2_product_txbx_tree.Visible = false;
            textBox3_tree.Visible = false;
            textBox2_name_tree.Visible = false;
            label8_type_tree.Visible = false;
            label8_product_tree.Visible = false;
            label8_name_tree.Visible = false;
            label16_tree.Visible = false;

            InitializeTree();
        }

        private void CreateColumns()
        {
            dataGridView1.Columns.Add("id", "ID");
            dataGridView1.Columns.Add("type", "Тип");
            dataGridView1.Columns.Add("product", "Продукт");
            dataGridView1.Columns.Add("IsNew", "Status");

            dataGridView2.Columns.Add("id_parent", "ID_Parent");
            dataGridView2.Columns.Add("id_child", "ID_Child");
            dataGridView2.Columns.Add("IsNew", "Status");

            dataGridView3.Columns.Add("objectId", "ID");
            dataGridView3.Columns.Add("name", "Название");
            dataGridView3.Columns.Add("Value", "Значение");
            dataGridView3.Columns.Add("IsNew", "Status");
        }

        private void ReadSingleView(DataGridView dgw, IDataRecord record)
        {
            switch (dgw.Name)
            {
                case "dataGridView1":
                    dgw.Rows.Add(record.GetInt32(0), record.GetString(1), record.GetString(2), RowState.Existed);
                    break;
                case "dataGridView2":
                    dgw.Rows.Add(record.GetInt32(0), record.GetInt32(1), RowState.Existed);
                    break;
                case "dataGridView3":
                    dgw.Rows.Add(record.GetInt32(0), record.GetString(1), record.GetString(2), RowState.Existed);
                    break;
            }
        }

        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();
            string queryString = ""; // Здесь переменная queryString пустая

            switch (dgw.Name)
            {
                case "dataGridView1":
                    queryString = "SELECT * FROM Objects";
                    break;
                case "dataGridView2":
                    queryString = "SELECT * FROM Links";
                    break;
                case "dataGridView3":
                    queryString = "SELECT * FROM Attributes";
                    break;
            }

            SqlCommand command = new SqlCommand(queryString, database.getConnection());

            database.openConnection();

            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ReadSingleView(dgw, reader);
            }
            reader.Close();
            database.closeConnection();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateColumns();
            RefreshDataGrid(dataGridView1);
            RefreshDataGrid(dataGridView2);
            RefreshDataGrid(dataGridView3);
            HandleDataGridViewEvents(dataGridView1);
            HandleDataGridViewEvents(dataGridView2);
            HandleDataGridViewEvents(dataGridView3);

        }

        private void HandleDataGridViewEvents(DataGridView dataGridView)
        {
            dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            dataGridView.RowsRemoved += DataGridView_RowsRemoved;
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = sender as DataGridView;
            selectedRow = e.RowIndex;

            if (selectedRow >= 0 && grid != null)
            {
                DataGridViewRow row = grid.Rows[selectedRow];

                switch (grid.Name)
                {
                    case "dataGridView1":
                        textBox1_id_Objects.Text = row.Cells[0].Value?.ToString();
                        textBox2_type_Objects.Text = row.Cells[1].Value?.ToString();
                        textBox3_product_Objects.Text = row.Cells[2].Value?.ToString();
                        break;

                    case "dataGridView2":
                        textBox6_idParent_Links.Text = row.Cells[0].Value?.ToString();
                        textBox5_idChild_Links.Text = row.Cells[1].Value?.ToString();
                        break;

                    case "dataGridView3":
                        textBox9_id_Attributes.Text = row.Cells[0].Value?.ToString();
                        textBox8_Name_Attributes.Text = row.Cells[1].Value?.ToString();
                        textBox7_value_Attributes.Text = row.Cells[2].Value?.ToString();
                        break;
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void add_btn_Objects_Click(object sender, EventArgs e)
        {
            Add_Objects add_Objects = new Add_Objects();
            add_Objects.Show();
        }

        private void deleteRows(DataGridView dataGridView, string tableName)
        {
            if (dataGridView.CurrentCell != null)
            {
                int index = dataGridView.CurrentCell.RowIndex;
                if (index >= 0 && index < dataGridView.Rows.Count)
                {
                    var id = Convert.ToInt32(dataGridView.Rows[index].Cells[0].Value);
                    dataGridView.Rows[index].Visible = false;
                    var deleteQuery = "";

                    if (tableName == "Objects")
                    {
                        dataGridView.Rows[index].Cells[3].Value = RowState.Deleted;
                        deleteQuery = $"DELETE FROM {tableName} WHERE id = {id}";
                        Console.WriteLine("Сработал метод для объекта");
                    }
                    else if (tableName == "Attributes")
                    {
                        dataGridView.Rows[index].Cells[3].Value = RowState.Deleted;
                        deleteQuery = $"DELETE FROM {tableName} WHERE objectId= {id}";
                        Console.WriteLine("Сработал метод для Attributes");
                    }

                    var command = new SqlCommand(deleteQuery, database.getConnection());
                    database.openConnection();
                    command.ExecuteNonQuery();
                    database.closeConnection();

                    isModified = true;
                    UpdateSaveButtonState();

                    // Обновление данных в DataGridView после удаления строки из базы данных
                    RefreshDataGrid(dataGridView);

                }
            }
        }

        private void delete_btn_Objects_Click(object sender, EventArgs e)
        {
            deleteRows(dataGridView1, "Objects");
        }

        private void Update(DataGridView dataGridView, string tableName)
        {
            database.openConnection();

            for (int index = 0; index < dataGridView.Rows.Count; index++)
            {
                var rowState = dataGridView.Rows[index].Cells[3].Value;

                if (rowState == null || (RowState)rowState == RowState.Existed)
                {
                    continue;
                }

                var id = Convert.ToInt32(dataGridView.Rows[index].Cells[0].Value);

                if ((RowState)rowState == RowState.Modify)
                {
                    string changeQuery = string.Empty;
                    SqlCommand command = new SqlCommand();

                    if (tableName == "Objects")
                    {
                        var type = dataGridView.Rows[index].Cells[1].Value?.ToString();
                        var product = dataGridView.Rows[index].Cells[2].Value?.ToString();
                        changeQuery = $"UPDATE {tableName} SET type = @type, product = @product WHERE id = @id";
                        command = new SqlCommand(changeQuery, database.getConnection());
                        command.Parameters.AddWithValue("@type", type);
                        command.Parameters.AddWithValue("@product", product);
                    }
                    else if (tableName == "Links")
                    {
                        var parentId = Convert.ToInt32(dataGridView.Rows[index].Cells[1].Value);
                        var childId = Convert.ToInt32(dataGridView.Rows[index].Cells[2].Value);
                        changeQuery = $"UPDATE {tableName} SET parentId = @parentId, childId = @childId WHERE parentId = @id";
                        command = new SqlCommand(changeQuery, database.getConnection());
                        command.Parameters.AddWithValue("@parentId", parentId);
                        command.Parameters.AddWithValue("@childId", childId);
                    }
                    else if (tableName == "Attributes")
                    {
                        var name = dataGridView.Rows[index].Cells[1].Value?.ToString();
                        var value = dataGridView.Rows[index].Cells[2].Value?.ToString();
                        changeQuery = $"UPDATE {tableName} SET name = @name, value = @value WHERE objectId = @id";
                        command = new SqlCommand(changeQuery, database.getConnection());
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@value", value);
                    }

                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }

            database.closeConnection();

            isModified = false;
            UpdateSaveButtonState();
            InitializeTree();
        }

        private void save_btn_Objects_Click(object sender, EventArgs e)
        {
            Update(dataGridView1, "Objects");
        }

        private void edit_btn_Objects_Click(object sender, EventArgs e)
        {
            Change(dataGridView1);
        }

        private void Change(DataGridView dataGridView)
        {
            if (dataGridView.CurrentCell != null)
            {
                var selectedRowIndex = dataGridView.CurrentCell.RowIndex;

                if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView.Rows.Count)
                {
                    var id = textBox1_id_Objects.Text;
                    var type = textBox2_type_Objects.Text;
                    var product = textBox3_product_Objects.Text;

                    if (!string.IsNullOrEmpty(id))
                    {
                        dataGridView.Rows[selectedRowIndex].SetValues(id, type, product);
                        dataGridView.Rows[selectedRowIndex].Cells[3].Value = RowState.Modify;

                        isModified = true;
                        UpdateSaveButtonState();
                    }
                }
            }
        }

        private void сохранитьJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveToJson(saveFileDialog.FileName);
            }
        }

        private void SaveToJson(string fileName)
        {
            int rowCount = Math.Max(dataGridView1.Rows.Count, Math.Max(dataGridView2.Rows.Count, dataGridView3.Rows.Count));

            var objects = new List<ObjectData>();
            var links = new List<LinksData>();
            var attributes = new List<AttributesData>();

            for (int i = 0; i < rowCount; i++)
            {
                var id_1 = i < dataGridView1.Rows.Count ? dataGridView1.Rows[i].Cells[0].Value?.ToString() : null;
                var type = i < dataGridView1.Rows.Count ? dataGridView1.Rows[i].Cells[1].Value?.ToString() : null;
                var product = i < dataGridView1.Rows.Count ? dataGridView1.Rows[i].Cells[2].Value?.ToString() : null;

                var id_parent = i < dataGridView2.Rows.Count ? dataGridView2.Rows[i].Cells[0].Value?.ToString() : null;
                var id_child = i < dataGridView2.Rows.Count ? dataGridView2.Rows[i].Cells[1].Value?.ToString() : null;

                var id_3 = i < dataGridView3.Rows.Count ? dataGridView3.Rows[i].Cells[0].Value?.ToString() : null;
                var name = i < dataGridView3.Rows.Count ? dataGridView3.Rows[i].Cells[1].Value?.ToString() : null;
                var value = i < dataGridView3.Rows.Count ? dataGridView3.Rows[i].Cells[2].Value?.ToString() : null;

                if (id_1 != null && type != null && product != null)
                {
                    var obj = new ObjectData
                    {
                        ID = id_1,
                        Type = type,
                        Product = product
                    };
                    objects.Add(obj);
                }

                if (id_parent != null && id_child != null)
                {
                    var lnks = new LinksData
                    {
                        ID_parent = id_parent,
                        ID_child = id_child
                    };
                    links.Add(lnks);
                }

                if (id_3 != null && name != null && value != null)
                {
                    var attr = new AttributesData
                    {
                        objectID = id_3,
                        name = name,
                        value = value
                    };
                    attributes.Add(attr);
                }
            }

            var json_1 = JsonConvert.SerializeObject(objects, Formatting.Indented);
            File.WriteAllText(fileName, json_1);
            var json_2 = JsonConvert.SerializeObject(links, Formatting.Indented);
            File.WriteAllText(fileName, json_2);
            var json_3 = JsonConvert.SerializeObject(attributes, Formatting.Indented);
            File.WriteAllText(fileName, json_3);

            MessageBox.Show("Data saved to JSON file successfully!", "Save to JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void сохранитьXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ExportToXML(saveFileDialog.FileName);
            }
        }

        private void ExportToXML(string fileName)
        {
            int rowCount = Math.Max(dataGridView1.Rows.Count, Math.Max(dataGridView2.Rows.Count, dataGridView3.Rows.Count));

            var objects = new List<ObjectData>();
            var links = new List<LinksData>();
            var attributes = new List<AttributesData>();

            for (int i = 0; i < rowCount; i++)
            {
                var id_1 = i < dataGridView1.Rows.Count ? dataGridView1.Rows[i].Cells[0].Value?.ToString() : null;
                var type = i < dataGridView1.Rows.Count ? dataGridView1.Rows[i].Cells[1].Value?.ToString() : null;
                var product = i < dataGridView1.Rows.Count ? dataGridView1.Rows[i].Cells[2].Value?.ToString() : null;

                var id_parent = i < dataGridView2.Rows.Count ? dataGridView2.Rows[i].Cells[0].Value?.ToString() : null;
                var id_child = i < dataGridView2.Rows.Count ? dataGridView2.Rows[i].Cells[1].Value?.ToString() : null;

                var id_3 = i < dataGridView3.Rows.Count ? dataGridView3.Rows[i].Cells[0].Value?.ToString() : null;
                var name = i < dataGridView3.Rows.Count ? dataGridView3.Rows[i].Cells[1].Value?.ToString() : null;
                var value = i < dataGridView3.Rows.Count ? dataGridView3.Rows[i].Cells[2].Value?.ToString() : null;

                if (id_1 != null && type != null && product != null)
                {
                    var obj = new ObjectData
                    {
                        ID = id_1,
                        Type = type,
                        Product = product
                    };
                    objects.Add(obj);
                }

                if (id_parent != null && id_child != null)
                {
                    var lnks = new LinksData
                    {
                        ID_parent = id_parent,
                        ID_child = id_child
                    };
                    links.Add(lnks);
                }

                if (id_3 != null && name != null && value != null)
                {
                    var attr = new AttributesData
                    {
                        objectID = id_3,
                        name = name,
                        value = value
                    };
                    attributes.Add(attr);
                }
            }


            var xmlSerializer_1 = new XmlSerializer(typeof(List<ObjectData>));
            var xmlSerializer_2 = new XmlSerializer(typeof(List<LinksData>));
            var xmlSerializer_3 = new XmlSerializer(typeof(List<AttributesData>));

            using (var writer = new StreamWriter(fileName))
            {
                xmlSerializer_1.Serialize(writer, objects);
                xmlSerializer_2.Serialize(writer, links);
                xmlSerializer_3.Serialize(writer, attributes);
            }

            MessageBox.Show("Data exported to XML file successfully!", "Export to XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var dataGridView = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var row = dataGridView.Rows[e.RowIndex];
                if (row.Cells[3].Value == null || (RowState)row.Cells[3].Value == RowState.Existed)
                {
                    row.Cells[3].Value = RowState.Modify;
                }

                isModified = true;
                UpdateSaveButtonState();
            }
        }

        private void DataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            isModified = true;
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            save_btn_Objects.Enabled = isModified;
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView2);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView3);
        }

        private void выходToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void save_btn_Attributes_Click(object sender, EventArgs e)
        {
            Update(dataGridView3, "Attributes");
        }

        private void save_btn_Links_Click(object sender, EventArgs e)
        {
            Update(dataGridView2, "Links");
        }

        private void del_btn_Attributes_Click(object sender, EventArgs e)
        {
            deleteRows(dataGridView3, "Attributes");
        }

        private void edit_btn_Links_Click(object sender, EventArgs e)
        {
            Change(dataGridView2);
        }

        private void edit_btn_Attributes_Click(object sender, EventArgs e)
        {
            Change(dataGridView3);
        }

        private void treeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.Visible = false;
            treeViewToolStripMenuItem.Checked = true;
            treeViewToolStripMenuItem.Enabled = false;
            dataGreidViewToolStripMenuItem.Enabled = !dataGreidViewToolStripMenuItem.Enabled;
            dataGreidViewToolStripMenuItem.Checked = !dataGreidViewToolStripMenuItem.Checked;
            add_atr_btn_tree.Visible = true;
            add_obj_btn_tree.Visible = true;
            del_atr_btn_tree.Visible=true;
            del_obj_btn_tree.Visible = true;
            treeView1.Visible = true;
            textBox1_type_txbx_tree.Visible = true;
            textBox2_product_txbx_tree.Visible = true;
            textBox3_tree.Visible = true;
            textBox2_name_tree.Visible = true;
            label8_type_tree.Visible = true;
            label8_product_tree.Visible = true;
            label8_name_tree.Visible = true;
            label16_tree.Visible = true;

        }

        private void dataGreidViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.Visible = true;
            treeViewToolStripMenuItem.Checked = false;
            treeViewToolStripMenuItem.Enabled = true;
            dataGreidViewToolStripMenuItem.Enabled = !dataGreidViewToolStripMenuItem.Enabled;
            dataGreidViewToolStripMenuItem.Checked = !dataGreidViewToolStripMenuItem.Checked;
            add_atr_btn_tree.Visible = false;
            add_obj_btn_tree.Visible = false;
            del_atr_btn_tree.Visible = false;
            del_obj_btn_tree.Visible = false;
            treeView1.Visible = false;
            textBox1_type_txbx_tree.Visible = false;
            textBox2_product_txbx_tree.Visible = false;
            textBox3_tree.Visible = false;
            textBox2_name_tree.Visible = false;
            label8_type_tree.Visible = false;
            label8_product_tree.Visible=false;
            label8_name_tree.Visible = false;
            label16_tree.Visible = false;
        }

        private void add_btn_Links_Click(object sender, EventArgs e)
        {
            AddLinks addLinks = new AddLinks();
            addLinks.Show();
        }

        private void add_btn_Attributes_Click(object sender, EventArgs e)
        {
            Add_Atributes add_Atributes = new Add_Atributes(); 
            add_Atributes.Show();
        }

        private void add_obj_btn_tree_Click(object sender, EventArgs e)
        {
            database.openConnection();
            var type = textBox1_type_txbx_tree.Text;
            var product = textBox2_product_txbx_tree.Text;

            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(product))
            {
                var addQuery = "INSERT INTO Objects (type, product) VALUES (@type, @product)";
                using (var command = new SqlCommand(addQuery, database.getConnection()))
                {
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
            database.closeConnection();
            InitializeTree();

        }

        private void add_atr_btn_tree_Click(object sender, EventArgs e)
        {
            string attributeName = textBox2_name_tree.Text;
            string attributeValue = textBox3_tree.Text;

            if (!string.IsNullOrEmpty(attributeName))
            {
                if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag != null)
                {
                    int objectId = (int)treeView1.SelectedNode.Tag;
                    database.openConnection();
                    SqlCommand command = new SqlCommand("INSERT INTO Attributes (objectId, name, value) VALUES (@objectId, @name, @value)", database.getConnection());
                    command.Parameters.AddWithValue("@objectId", objectId);
                    command.Parameters.AddWithValue("@name", attributeName);
                    command.Parameters.AddWithValue("@value", attributeValue);
                    command.ExecuteNonQuery();
                    LoadAttributes(objectId);
                    InitializeTree();
                }
                else
                {
                    MessageBox.Show("Сначала выберите объект, к которому хотите добавить атрибут.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Введите имя атрибута.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
  
        }

        private void del_obj_btn_tree_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                
                int objectId = (int)treeView1.SelectedNode.Tag;

                database.openConnection();
                SqlCommand command = new SqlCommand("DELETE FROM Objects WHERE id = @id", database.getConnection());
                command.Parameters.AddWithValue("@id", objectId);
                command.ExecuteNonQuery();
                database.closeConnection();

                InitializeTree();
            }
        }

        private void del_atr_btn_tree_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                // Проверяем, выбран ли узел атрибутов
                if (treeView1.SelectedNode.Tag != null && treeView1.SelectedNode.Tag.ToString() == "Attributes")
                {
                    // Запрашиваем подтверждение удаления атрибута
                    DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить этот атрибут?", "Удаление атрибута", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        // Удаляем атрибут из базы данных
                        int objectId = (int)treeView1.SelectedNode.Parent.Tag; // Получаем идентификатор объекта
                        string attributeName = treeView1.SelectedNode.Nodes[0].Text.Split(':')[0].Trim(); // Получаем имя атрибута
                        RemoveAttributeFromDatabase(objectId, attributeName);

                        // Удаляем только выбранный узел атрибутов
                        treeView1.SelectedNode.Remove();
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите узел атрибутов для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите узел для удаления атрибута.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void RemoveAttributeFromDatabase(int objectId, string attributeName)
        {
            try
            {
                database.openConnection();
                SqlCommand command = new SqlCommand("DELETE FROM Attributes WHERE objectId = @objectId AND name = @attributeName", database.getConnection());
                command.Parameters.AddWithValue("@objectId", objectId);
                command.Parameters.AddWithValue("@attributeName", attributeName);
                command.ExecuteNonQuery();
                database.closeConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении атрибута из базы данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TreeNode FindNodeByTag(TreeNodeCollection nodes, int tagValue)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && (int)node.Tag == tagValue)
                {
                    return node;
                }

                TreeNode foundNode = FindNodeByTag(node.Nodes, tagValue);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }

        private void InitializeTree()
        {
            treeView1.Nodes.Clear();

            database.openConnection();
            SqlCommand command = new SqlCommand("SELECT id, type, product FROM Objects", database.getConnection());
            SqlDataReader reader = command.ExecuteReader();

            Dictionary<int, TreeNode> nodes = new Dictionary<int, TreeNode>();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string type = reader.GetString(1);
                string product = reader.GetString(2);

                TreeNode node = new TreeNode($"{type}: {product}")
                {
                    Tag = id
                };
                nodes.Add(id, node);
            }

            reader.Close(); // Закрыть SqlDataReader здесь

            // Загрузка атрибутов объектов после закрытия SqlDataReader
            foreach (var node in nodes.Values)
            {
                // Получение id объекта
                int objectId = (int)node.Tag;

                // Загрузка атрибутов объекта
                StringBuilder attributesText = LoadAttributes(objectId);
                if (attributesText.Length > 0)
                {
                    TreeNode attributeNode = new TreeNode("Attributes")
                    {
                        Tag = "Attributes",
                        Text = "Attributes",
                        ToolTipText = "Attributes"
                    };
                    attributeNode.Nodes.Add(attributesText.ToString());
                    node.Nodes.Add(attributeNode);
                }
            }

            database.closeConnection();
            // Добавление корневых узлов в TreeView
            foreach (var node in nodes.Values)
            {
                if (node.Parent == null)
                {
                    treeView1.Nodes.Add(node);
                }
            }
        }
        private StringBuilder LoadAttributes(int objectId)
        {
            StringBuilder attributesText = new StringBuilder();
            database.openConnection();
            SqlCommand command = new SqlCommand("SELECT name, value FROM Attributes WHERE objectId = @objectId", database.getConnection());
            command.Parameters.AddWithValue("@objectId", objectId);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    string value = reader.GetString(1);
                    attributesText.AppendLine($"{name}: {value}");
                }
            }
            database.closeConnection();

            return attributesText;
        }
    }
}
