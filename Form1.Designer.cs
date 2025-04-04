namespace dbms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button testConnectionButton;
        private System.Windows.Forms.Button loadCategoriesButton;
        private System.Windows.Forms.Button addRecordInChildTableButton;
        private System.Windows.Forms.Button deleteChildRowButton;
        private System.Windows.Forms.Button updateChildRowButton;
        private System.Windows.Forms.DataGridView dataGridCategories;
        private System.Windows.Forms.DataGridView dataGridSupplements;
        private System.Windows.Forms.TextBox supplementNameTextBox;
        private System.Windows.Forms.TextBox supplementDescriptionTextBox;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.testConnectionButton = new System.Windows.Forms.Button();
            this.loadCategoriesButton = new System.Windows.Forms.Button();
            this.dataGridCategories = new System.Windows.Forms.DataGridView();
            this.dataGridSupplements = new System.Windows.Forms.DataGridView();
            this.addRecordInChildTableButton = new System.Windows.Forms.Button();
            this.supplementNameTextBox = new System.Windows.Forms.TextBox();
            this.deleteChildRowButton = new System.Windows.Forms.Button();
            this.supplementDescriptionTextBox = new System.Windows.Forms.TextBox();

            // testConnectionButton
            this.testConnectionButton.Location = new System.Drawing.Point(12, 12);
            this.testConnectionButton.Name = "testConnectionButton";
            this.testConnectionButton.Size = new System.Drawing.Size(210, 125);
            this.testConnectionButton.TabIndex = 0;
            this.testConnectionButton.Text = "Test Connection";
            this.testConnectionButton.UseVisualStyleBackColor = true;
            this.testConnectionButton.Click += new System.EventHandler(this.TestConnectionButtonClick);

            // loadCategoriesButton
            this.loadCategoriesButton.Location = new System.Drawing.Point(250, 12);
            this.loadCategoriesButton.Name = "loadCategoriesButton";
            this.loadCategoriesButton.Size = new System.Drawing.Size(210, 125);
            this.loadCategoriesButton.TabIndex = 1;
            this.loadCategoriesButton.Text = "Load Categories";
            this.loadCategoriesButton.UseVisualStyleBackColor = true;
            this.loadCategoriesButton.Click += new System.EventHandler(this.LoadCategoriesButtonClick);

            // add button
            this.addRecordInChildTableButton.Location = new System.Drawing.Point(450, 12);
            this.addRecordInChildTableButton.Name = "addRecordInChildTableButton";
            this.addRecordInChildTableButton.Size = new System.Drawing.Size(210, 100);
            this.addRecordInChildTableButton.TabIndex = 3;
            this.addRecordInChildTableButton.Text = "Add record";
            this.addRecordInChildTableButton.UseVisualStyleBackColor = true;
            this.addRecordInChildTableButton.Click += new System.EventHandler(this.AddRecordButtonClick);

            // delete button
            this.deleteChildRowButton.Location = new System.Drawing.Point(700, 12);
            this.deleteChildRowButton.Name = "DeleteChildRowButton";
            this.deleteChildRowButton.Size = new System.Drawing.Size(210, 125);
            this.deleteChildRowButton.TabIndex = 5;
            this.deleteChildRowButton.Text = "Delete Selected Child Row";
            this.deleteChildRowButton.UseVisualStyleBackColor = true;
            this.deleteChildRowButton.Click += new System.EventHandler(this.DeleteChildRowButtonClick);

            this.updateChildRowButton = new System.Windows.Forms.Button();
            this.updateChildRowButton.Location = new System.Drawing.Point(950, 12);
            this.updateChildRowButton.Name = "updateChildRowButton";
            this.updateChildRowButton.Size = new System.Drawing.Size(210, 125);
            this.updateChildRowButton.TabIndex = 6;
            this.updateChildRowButton.Text = "Update Selected Row";
            this.updateChildRowButton.UseVisualStyleBackColor = true;
            this.updateChildRowButton.Click += new System.EventHandler(this.UpdateButtonClick);


            // dataGridCategories
            this.dataGridCategories.Location = new System.Drawing.Point(12, 150);
            this.dataGridCategories.Name = "dataGridCategories";
            this.dataGridCategories.Size = new System.Drawing.Size(500, 300);
            this.dataGridCategories.TabIndex = 2;
            this.dataGridCategories.SelectionChanged += new System.EventHandler(this.dataGridCategoriesSelectionChanged);

            // dataGridSupplements
            this.dataGridSupplements.Location = new System.Drawing.Point(550, 150);  
            this.dataGridSupplements.Name = "dataGridSupplements";
            this.dataGridSupplements.Size = new System.Drawing.Size(500, 300);
            this.dataGridSupplements.TabIndex = 3;

            this.supplementNameTextBox.Location = new System.Drawing.Point(550, 500);
            this.supplementNameTextBox.Name = "supplementName";
            this.supplementNameTextBox.ReadOnly = false;
            this.supplementNameTextBox.Size = new System.Drawing.Size(100, 26);
            this.supplementNameTextBox.TabIndex = 10;
            this.supplementNameTextBox.Text = "Supplement name:";

            this.supplementDescriptionTextBox.Location = new System.Drawing.Point(550, 550);
            this.supplementDescriptionTextBox.Name = "supplementDescription";
            this.supplementDescriptionTextBox.ReadOnly = false;
            this.supplementDescriptionTextBox.Size = new System.Drawing.Size(131, 26);
            this.supplementDescriptionTextBox.TabIndex = 11;
            this.supplementDescriptionTextBox.Text = "Supplement description:";


            // Form1 settings
            this.Controls.Add(this.testConnectionButton);
            this.Controls.Add(this.loadCategoriesButton);
            this.Controls.Add(this.addRecordInChildTableButton);
            this.Controls.Add(this.dataGridCategories);
            this.Controls.Add(this.dataGridSupplements);
            this.Controls.Add(this.supplementNameTextBox);
            this.Controls.Add(this.supplementDescriptionTextBox);
            this.Controls.Add(this.deleteChildRowButton);
            this.Controls.Add(this.updateChildRowButton);

            this.ClientSize = new System.Drawing.Size(1080, 600);
            this.Text = "Supplements Database Management System";
        }
    }
}
