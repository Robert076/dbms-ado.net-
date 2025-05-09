using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing;

namespace dbms
{
    public partial class Form1 : Form
    {
        private FormConfiguration _config;
        private string _connectionString;
        private SqlDataAdapter _categoriesAdapter, _detailAdapter;
        private DataSet _categoriesDataSet = new DataSet();
        private DataSet _detailDataSet = new DataSet();
        private int? _selectedCategoryId = null;
        private Panel inputPanel;
        private Panel buttonPanel;

        public Form1()
        {
            InitializeComponent();
            try
            {
                _config = ConfigurationManager.GetConfiguration();
                _connectionString = _config.ConnectionString;
                this.Text = _config.FormCaption;
                SetupUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void SetupUI()
        {
            // Create button panel at the top
            buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10)
            };

            // Create and add buttons to the panel
            Button testConnectionButton = new Button
            {
                Text = "Test Connection",
                Location = new Point(10, 10),
                Width = 120
            };
            testConnectionButton.Click += TestConnectionButtonClick;

            Button loadCategoriesButton = new Button
            {
                Text = "Load Categories",
                Location = new Point(140, 10),
                Width = 120
            };
            loadCategoriesButton.Click += LoadCategoriesButtonClick;

            buttonPanel.Controls.Add(testConnectionButton);
            buttonPanel.Controls.Add(loadCategoriesButton);
            this.Controls.Add(buttonPanel);

            // Create input panel
            inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10)
            };
            this.Controls.Add(inputPanel);

            // Setup input fields
            SetupInputFields();

            // Adjust DataGridViews
            dataGridCategories.Dock = DockStyle.Top;
            dataGridCategories.Height = 200;

            dataGridSupplements.Dock = DockStyle.Fill;
        }

        private void SetupInputFields()
        {
            inputPanel.Controls.Clear();

            // Create input fields based on configuration
            int yOffset = 10;
            foreach (string column in _config.DetailTable.InputColumns)
            {
                Label label = new Label
                {
                    Text = column,
                    Location = new Point(10, yOffset),
                    AutoSize = true
                };
                inputPanel.Controls.Add(label);

                TextBox textBox = new TextBox
                {
                    Name = column + "TextBox",
                    Location = new Point(150, yOffset),
                    Width = 200
                };
                inputPanel.Controls.Add(textBox);

                yOffset += 30;
            }

            // Add action buttons
            Button addButton = new Button
            {
                Text = "Add",
                Location = new Point(10, yOffset),
                Width = 80
            };
            addButton.Click += AddRecordButtonClick;

            Button updateButton = new Button
            {
                Text = "Update",
                Location = new Point(100, yOffset),
                Width = 80
            };
            updateButton.Click += UpdateButtonClick;

            Button deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(190, yOffset),
                Width = 80
            };
            deleteButton.Click += DeleteChildRowButtonClick;

            inputPanel.Controls.Add(addButton);
            inputPanel.Controls.Add(updateButton);
            inputPanel.Controls.Add(deleteButton);
        }

        public void TestConnectionButtonClick(object sender, EventArgs e)
        {
            TestConnection();
        }

        private void TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Test connection succeeded.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test connection failed: " + ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCategoriesButtonClick(object sender, EventArgs e)
        {
            LoadAllCategories();
        }

        private void LoadAllCategories()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(_config.MasterTable.SelectQuery, connection);

                _categoriesAdapter = new SqlDataAdapter(cmd);
                _categoriesDataSet.Clear();

                _categoriesAdapter.Fill(_categoriesDataSet, _config.MasterTable.TableName);
                dataGridCategories.DataSource = _categoriesDataSet.Tables[_config.MasterTable.TableName];
                connection.Close();
            }
        }

        private void LoadDetailBySelectedCategoryId()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                if (this._selectedCategoryId == null || this._selectedCategoryId == -1)
                {
                    MessageBox.Show("Please select a category first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SqlCommand cmd = new SqlCommand(_config.DetailTable.SelectQuery, connection);
                cmd.Parameters.AddWithValue("@CategoryID", this._selectedCategoryId);

                _detailAdapter = new SqlDataAdapter(cmd);
                _detailDataSet.Clear();
                _detailAdapter.Fill(_detailDataSet, _config.DetailTable.TableName);
                dataGridSupplements.DataSource = _detailDataSet.Tables[_config.DetailTable.TableName];
                connection.Close();
            }
        }

        private void dataGridCategoriesSelectionChanged(object sender, EventArgs e)
        {
            if (dataGridCategories.CurrentRow != null)
            {
                this._selectedCategoryId = Convert.ToInt32(dataGridCategories.CurrentRow.Cells["CategoryID"].Value);
                LoadDetailBySelectedCategoryId();
            }
        }

        private void AddRecordButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (this._selectedCategoryId == -1 || this._selectedCategoryId == null)
                {
                    MessageBox.Show($"Please select a category in order to add a {_config.DetailTable.DisplayName.ToLower()} first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate all required fields
                foreach (string column in _config.DetailTable.InputColumns)
                {
                    TextBox textBox = this.Controls.Find(column + "TextBox", true)[0] as TextBox;
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        MessageBox.Show("Please fill all the fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                SqlCommand command = new SqlCommand(_config.DetailTable.InsertQuery, conn);
                
                // Add parameters based on configuration
                foreach (string column in _config.DetailTable.InputColumns)
                {
                    TextBox textBox = this.Controls.Find(column + "TextBox", true)[0] as TextBox;
                    command.Parameters.AddWithValue("@" + column, textBox.Text);
                }
                command.Parameters.AddWithValue("@CategoryID", this._selectedCategoryId);

                conn.Open();

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    conn.Close();
                }
                MessageBox.Show($"{_config.DetailTable.DisplayName} added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Clear input fields
            foreach (string column in _config.DetailTable.InputColumns)
            {
                TextBox textBox = this.Controls.Find(column + "TextBox", true)[0] as TextBox;
                textBox.Clear();
            }
            LoadDetailBySelectedCategoryId();
        }

        private void DeleteChildRowButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (dataGridSupplements.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show($"Are you sure you want to delete this {_config.DetailTable.DisplayName.ToLower()}?",
                        "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        string idColumn = _config.DetailTable.DisplayColumns[0]; // Assuming first column is ID
                        int detailId = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells[idColumn].Value);
                        SqlCommand cmd = new SqlCommand(_config.DetailTable.DeleteQuery, conn);
                        cmd.Parameters.AddWithValue("@" + idColumn, detailId);
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        finally
                        {
                            conn.Close();
                        }
                        MessageBox.Show($"{_config.DetailTable.DisplayName} deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"Please select a {_config.DetailTable.DisplayName.ToLower()} to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                LoadDetailBySelectedCategoryId();
            }
        }

        private void UpdateButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (dataGridSupplements.SelectedRows.Count > 0)
                {
                    string idColumn = _config.DetailTable.DisplayColumns[0]; // Assuming first column is ID
                    int detailId = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells[idColumn].Value);

                    SqlCommand command = new SqlCommand(_config.DetailTable.UpdateQuery, conn);

                    // Add parameters based on configuration
                    foreach (string column in _config.DetailTable.InputColumns)
                    {
                        string value = dataGridSupplements.SelectedRows[0].Cells[column].Value.ToString();
                        command.Parameters.AddWithValue("@" + column, value);
                    }
                    command.Parameters.AddWithValue("@" + idColumn, detailId);

                    conn.Open();
                    int rowsAffected = 0;

                    try
                    {
                        rowsAffected = command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    finally
                    {
                        conn.Close();
                    }
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{_config.DetailTable.DisplayName} updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No changes were made.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"Please select a {_config.DetailTable.DisplayName.ToLower()} to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
