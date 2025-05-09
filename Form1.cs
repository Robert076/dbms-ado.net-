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
            // Set form to start maximized
            this.WindowState = FormWindowState.Maximized;

            // Create a panel for all buttons
            Panel buttonPanel = new Panel
            {
                Location = new Point(10, 10),
                Height = 30,
                Width = this.ClientSize.Width - 20,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(buttonPanel);

            // Add all buttons to the panel
            int buttonX = 0;
            int buttonWidth = 100;
            int buttonHeight = 25;
            int buttonSpacing = 10;

            // Test Connection button
            if (testConnectionButton != null)
            {
                testConnectionButton.Size = new Size(buttonWidth, buttonHeight);
                testConnectionButton.Location = new Point(buttonX, 0);
                testConnectionButton.Click += TestConnectionButtonClick;
                buttonPanel.Controls.Add(testConnectionButton);
                buttonX += buttonWidth + buttonSpacing;
            }

            // Load Categories button
            if (loadCategoriesButton != null)
            {
                loadCategoriesButton.Size = new Size(buttonWidth, buttonHeight);
                loadCategoriesButton.Location = new Point(buttonX, 0);
                loadCategoriesButton.Click += LoadCategoriesButtonClick;
                buttonPanel.Controls.Add(loadCategoriesButton);
                buttonX += buttonWidth + buttonSpacing;
            }

            // Create input panel below the buttons
            inputPanel = new Panel
            {
                Location = new Point(10, buttonPanel.Bottom + 10),
                Width = this.ClientSize.Width - 20,
                Height = 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(inputPanel);

            // Setup input fields
            SetupInputFields();

            // Adjust DataGridViews
            dataGridCategories.Location = new Point(10, inputPanel.Bottom + 10);
            dataGridCategories.Width = this.ClientSize.Width - 20;
            dataGridCategories.Height = (this.ClientSize.Height - inputPanel.Bottom - 20) / 2;
            dataGridCategories.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            dataGridSupplements.Location = new Point(10, dataGridCategories.Bottom + 10);
            dataGridSupplements.Width = this.ClientSize.Width - 20;
            dataGridSupplements.Height = this.ClientSize.Height - dataGridSupplements.Top - 20;
            dataGridSupplements.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        }

        private void SetupInputFields()
        {
            inputPanel.Controls.Clear();

            // Create input fields based on configuration
            int yOffset = 5;
            int xOffset = 10;
            foreach (string column in _config.DetailTable.InputColumns)
            {
                Label label = new Label
                {
                    Text = column,
                    Location = new Point(xOffset, yOffset + 3),
                    AutoSize = true
                };
                inputPanel.Controls.Add(label);

                TextBox textBox = new TextBox
                {
                    Name = column + "TextBox",
                    Location = new Point(xOffset + 120, yOffset),
                    Width = 150,
                    Height = 20
                };
                inputPanel.Controls.Add(textBox);

                xOffset += 300; // Move to next column
                if (xOffset > inputPanel.Width - 300) // If we're running out of space
                {
                    xOffset = 10; // Reset to first column
                    yOffset += 30; // Move to next row
                }
            }

            // Add action buttons in a row
            int buttonY = yOffset + 10;
            Button addButton = new Button
            {
                Text = "Add",
                Location = new Point(10, buttonY),
                Size = new Size(60, 25)
            };
            addButton.Click += AddRecordButtonClick;

            Button updateButton = new Button
            {
                Text = "Update",
                Location = new Point(80, buttonY),
                Size = new Size(60, 25)
            };
            updateButton.Click += UpdateButtonClick;

            Button deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(150, buttonY),
                Size = new Size(60, 25)
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
