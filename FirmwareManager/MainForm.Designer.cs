namespace FrimwareDatabase.UI.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCreateDataBase = new System.Windows.Forms.Button();
            this.buttonOpenBinFile = new System.Windows.Forms.Button();
            this.buttonOpenHexFile = new System.Windows.Forms.Button();
            this.buttonSelectDataBase = new System.Windows.Forms.Button();
            this.groupBoxDatabase = new System.Windows.Forms.GroupBox();
            this.listViewFirmwares = new System.Windows.Forms.ListView();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBoxDatabase.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCreateDataBase
            // 
            this.buttonCreateDataBase.Location = new System.Drawing.Point(172, 12);
            this.buttonCreateDataBase.Name = "buttonCreateDataBase";
            this.buttonCreateDataBase.Size = new System.Drawing.Size(111, 42);
            this.buttonCreateDataBase.TabIndex = 0;
            this.buttonCreateDataBase.Text = "Создать новую базу данных";
            this.buttonCreateDataBase.UseVisualStyleBackColor = true;
            this.buttonCreateDataBase.Click += new System.EventHandler(this.buttonCreateDatabase_Click);
            // 
            // buttonOpenBinFile
            // 
            this.buttonOpenBinFile.Location = new System.Drawing.Point(43, 212);
            this.buttonOpenBinFile.Name = "buttonOpenBinFile";
            this.buttonOpenBinFile.Size = new System.Drawing.Size(111, 41);
            this.buttonOpenBinFile.TabIndex = 2;
            this.buttonOpenBinFile.Text = "Добавить файл";
            this.buttonOpenBinFile.UseVisualStyleBackColor = true;
            this.buttonOpenBinFile.Click += new System.EventHandler(this.buttonAddFile_Click);
            // 
            // buttonOpenHexFile
            // 
            this.buttonOpenHexFile.Location = new System.Drawing.Point(160, 212);
            this.buttonOpenHexFile.Name = "buttonOpenHexFile";
            this.buttonOpenHexFile.Size = new System.Drawing.Size(111, 41);
            this.buttonOpenHexFile.TabIndex = 3;
            this.buttonOpenHexFile.Text = "Удалить файл";
            this.buttonOpenHexFile.UseVisualStyleBackColor = true;
            this.buttonOpenHexFile.Click += new System.EventHandler(this.buttonDeleteSelected_Click);
            // 
            // buttonSelectDataBase
            // 
            this.buttonSelectDataBase.Location = new System.Drawing.Point(55, 12);
            this.buttonSelectDataBase.Name = "buttonSelectDataBase";
            this.buttonSelectDataBase.Size = new System.Drawing.Size(111, 42);
            this.buttonSelectDataBase.TabIndex = 4;
            this.buttonSelectDataBase.Text = "Выбрать базу данных";
            this.buttonSelectDataBase.UseVisualStyleBackColor = true;
            this.buttonSelectDataBase.Click += new System.EventHandler(this.buttonSelectDatabase_Click);
            // 
            // groupBoxDatabase
            // 
            this.groupBoxDatabase.Controls.Add(this.listViewFirmwares);
            this.groupBoxDatabase.Controls.Add(this.button2);
            this.groupBoxDatabase.Controls.Add(this.buttonOpenBinFile);
            this.groupBoxDatabase.Controls.Add(this.buttonOpenHexFile);
            this.groupBoxDatabase.Location = new System.Drawing.Point(12, 60);
            this.groupBoxDatabase.Name = "groupBoxDatabase";
            this.groupBoxDatabase.Size = new System.Drawing.Size(310, 307);
            this.groupBoxDatabase.TabIndex = 7;
            this.groupBoxDatabase.TabStop = false;
            this.groupBoxDatabase.Text = "DataBase.xml";
            this.groupBoxDatabase.Visible = false;
            this.groupBoxDatabase.Enter += new System.EventHandler(this.groupBoxDatabase_Enter);
            // 
            // listViewFirmwares
            // 
            this.listViewFirmwares.GridLines = true;
            this.listViewFirmwares.HideSelection = false;
            this.listViewFirmwares.Location = new System.Drawing.Point(6, 19);
            this.listViewFirmwares.Name = "listViewFirmwares";
            this.listViewFirmwares.Size = new System.Drawing.Size(297, 187);
            this.listViewFirmwares.TabIndex = 5;
            this.listViewFirmwares.UseCompatibleStateImageBehavior = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(98, 259);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(111, 41);
            this.button2.TabIndex = 4;
            this.button2.Text = "Записать файл на носитель";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 376);
            this.Controls.Add(this.groupBoxDatabase);
            this.Controls.Add(this.buttonSelectDataBase);
            this.Controls.Add(this.buttonCreateDataBase);
            this.MaximumSize = new System.Drawing.Size(343, 415);
            this.Name = "MainForm";
            this.Text = "Управление файлами прошивок";
            this.groupBoxDatabase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCreateDataBase;
        private System.Windows.Forms.Button buttonOpenBinFile;
        private System.Windows.Forms.Button buttonOpenHexFile;
        private System.Windows.Forms.Button buttonSelectDataBase;
        private System.Windows.Forms.GroupBox groupBoxDatabase;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView listViewFirmwares;
    }
}

