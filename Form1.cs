using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;
using System.Configuration;

namespace dbms
{
    public partial class Form1 : Form
    {
        private FormConfiguration _config;
        private string _connectionString;
        private SqlDataAdapter _categoriesAdapter, _supplementsAdapter;
        private DataSet _categoriesDataSet = new DataSet();
        private DataSet _supplementsDataSet = new DataSet();
        private int? _selectedCategoryId = null;

        public Form1()
        {
            InitializeComponent();
            try
            {
                _config = ConfigurationManager.GetConfiguration();
                _connectionString = _config.ConnectionString;
                this.Text = _config.FormCaption;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
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

        private void LoadSupplementsBySelectedCategoryId()
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

                _supplementsAdapter = new SqlDataAdapter(cmd);
                _supplementsDataSet.Clear();
                _supplementsAdapter.Fill(_supplementsDataSet, _config.DetailTable.TableName);
                dataGridSupplements.DataSource = _supplementsDataSet.Tables[_config.DetailTable.TableName];
                connection.Close();
            }
        }

        private void dataGridCategoriesSelectionChanged(object sender, EventArgs e)
        {
            if (dataGridCategories.CurrentRow != null)
            {
                this._selectedCategoryId = Convert.ToInt32(dataGridCategories.CurrentRow.Cells["CategoryID"].Value);
                LoadSupplementsBySelectedCategoryId();
            }
        }

        private void AddRecordButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (this._selectedCategoryId == -1 || this._selectedCategoryId == null)
                {
                    MessageBox.Show("Please select a category in order to add a supplement first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string supplementName = supplementNameTextBox.Text;
                string supplementDescription = supplementDescriptionTextBox.Text;

                if (supplementName == "" || supplementDescription == "")
                {
                    MessageBox.Show("Please fill all the fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SqlCommand command = new SqlCommand(_config.DetailTable.InsertQuery, conn);
                command.Parameters.AddWithValue("@SupplementName", supplementName);
                command.Parameters.AddWithValue("@SupplementDescription", supplementDescription);
                command.Parameters.AddWithValue("@SupplierId", 2);
                command.Parameters.AddWithValue("@SupplementStock", 100);
                command.Parameters.AddWithValue("@SupplementPrice", 24.22);
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
                MessageBox.Show("Supplement added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            supplementNameTextBox.Clear();
            supplementDescriptionTextBox.Clear();
            LoadSupplementsBySelectedCategoryId();
        }

        private void DeleteChildRowButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (dataGridSupplements.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this supplement?",
                        "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        int supplementId = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementID"].Value);
                        SqlCommand cmd = new SqlCommand(_config.DetailTable.DeleteQuery, conn);
                        cmd.Parameters.AddWithValue("@SupplementID", supplementId);
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
                        MessageBox.Show("Supplement deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a supplement to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                LoadSupplementsBySelectedCategoryId();
            }
        }

        private void UpdateButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                if (dataGridSupplements.SelectedRows.Count > 0)
                {
                    int supplementId = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementID"].Value);

                    string supplementName = dataGridSupplements.SelectedRows[0].Cells["SupplementName"].Value.ToString();
                    string supplementDescription = dataGridSupplements.SelectedRows[0].Cells["SupplementDescription"].Value.ToString();
                    Int32 supplementStock = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementStock"].Value);
                    Int32 supplementPrice = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementPrice"].Value);

                    SqlCommand command = new SqlCommand(_config.DetailTable.UpdateQuery, conn);

                    command.Parameters.AddWithValue("@SupplementName", supplementName);
                    command.Parameters.AddWithValue("@SupplementDescription", supplementDescription);
                    command.Parameters.AddWithValue("@SupplementStock", supplementStock);
                    command.Parameters.AddWithValue("@SupplementPrice", supplementPrice);
                    command.Parameters.AddWithValue("@SupplementID", supplementId);

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
                        MessageBox.Show("Supplement updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No changes were made.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a supplement to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
