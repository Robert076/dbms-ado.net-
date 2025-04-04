using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace dbms
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=ASUS-PC\SQLEXPRESS;Initial Catalog=Supplements;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";
        SqlDataAdapter categoriesAdapter, supplementsAdapter;
        DataSet categoriesDataSet = new DataSet();
        DataSet supplementsDataSet = new DataSet();

        int? selectedCategoryId = null;


        public Form1()
        {
            InitializeComponent();
        }

        public void TestConnectionButtonClick(object sender, EventArgs e)
        {
            TestConnection();
        }

        private void TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
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
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Categories", connection);

                categoriesAdapter = new SqlDataAdapter(cmd);
                categoriesDataSet.Clear();

                categoriesAdapter.Fill(categoriesDataSet, "Categories");
                dataGridCategories.DataSource = categoriesDataSet.Tables["Categories"];
                connection.Close();
            }
        }

        private void LoadSupplementsBySelectedCategoryId()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (this.selectedCategoryId == null || this.selectedCategoryId == -1)
                {
                    MessageBox.Show("Please select a category first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SqlCommand cmd = new SqlCommand("SELECT s.SupplementID, SupplementName, SupplementDescription, SupplierID, SupplementStock, SupplementPrice FROM Supplements s WHERE s.CategoryID = @CategoryID", connection);
                cmd.Parameters.AddWithValue("@CategoryID", this.selectedCategoryId);

                supplementsAdapter = new SqlDataAdapter(cmd);
                supplementsDataSet.Clear();
                supplementsAdapter.Fill(supplementsDataSet, "Supplements");
                dataGridSupplements.DataSource = supplementsDataSet.Tables["Supplements"];
                connection.Close();
            }
        }

        private void dataGridCategoriesSelectionChanged(object sender, EventArgs e)
        {
            if (dataGridCategories.CurrentRow != null)
            {
                this.selectedCategoryId = Convert.ToInt32(dataGridCategories.CurrentRow.Cells["CategoryID"].Value);
                LoadSupplementsBySelectedCategoryId();
            }
        }

        private void AddRecordButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (this.selectedCategoryId == -1 || this.selectedCategoryId == null)
                {
                    MessageBox.Show("Please select a category in order to add a supplement first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string supplementName = supplementNameTextBox.Text;
                string supplementDescription = supplementDescriptionTextBox.Text;

                SqlCommand command = new SqlCommand("INSERT INTO Supplements(SupplementName, SupplementDescription, SupplierId, SupplementStock, SupplementPrice, CategoryID) VALUES(@SupplementName, @SupplementDescription, @SupplierId, @SupplementStock, @SupplementPrice, @CategoryID )", conn);
                if (supplementName == "" || supplementDescription == "")
                {
                    MessageBox.Show("Please fill all the fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                command.Parameters.AddWithValue("@SupplementName", supplementName);
                command.Parameters.AddWithValue("@SupplementDescription", supplementDescription);
                command.Parameters.AddWithValue("@SupplierId", 2);
                command.Parameters.AddWithValue("@SupplementStock", 100);
                command.Parameters.AddWithValue("@SupplementPrice", 24.22);
                command.Parameters.AddWithValue("@CategoryID", this.selectedCategoryId);

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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (dataGridSupplements.SelectedRows.Count > 0)
                {
                    // Ask for confirmation before deleting
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this supplement?",
                        "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        int supplementId = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementID"].Value);
                        SqlCommand cmd = new SqlCommand("DELETE FROM Supplements WHERE SupplementID = @SupplementID", conn);
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
                    else
                    {
                        MessageBox.Show("Please select a supplement to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                LoadSupplementsBySelectedCategoryId();
            }
        }

        private void UpdateButtonClick(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (dataGridSupplements.SelectedRows.Count > 0)
                {
                    int supplementId = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementID"].Value);

                    string supplementName = dataGridSupplements.SelectedRows[0].Cells["SupplementName"].Value.ToString();
                    string supplementDescription = dataGridSupplements.SelectedRows[0].Cells["SupplementDescription"].Value.ToString();
                    Int32 supplementStock = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementStock"].Value);
                    Int32 supplementPrice = Convert.ToInt32(dataGridSupplements.SelectedRows[0].Cells["SupplementPrice"].Value);

                    SqlCommand command = new SqlCommand("UPDATE Supplements SET SupplementName = @SupplementName, SupplementDescription = @SupplementDescription, SupplementStock = @SupplementStock, SupplementPrice = @SupplementPrice WHERE SupplementID = @SupplementID", conn);

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
