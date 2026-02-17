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
            this.buttonOptions = new System.Windows.Forms.Button();
            this.listViewFirmwares = new System.Windows.Forms.ListView();
            this.buttonWriteToUSB = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.buttonToggleServer = new System.Windows.Forms.Button();
            this.labelServerStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonUninstallServer = new System.Windows.Forms.Button();
            this.groupBoxDatabase.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
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
            this.groupBoxDatabase.Controls.Add(this.buttonOptions);
            this.groupBoxDatabase.Controls.Add(this.listViewFirmwares);
            this.groupBoxDatabase.Controls.Add(this.buttonWriteToUSB);
            this.groupBoxDatabase.Controls.Add(this.buttonOpenBinFile);
            this.groupBoxDatabase.Controls.Add(this.buttonOpenHexFile);
            this.groupBoxDatabase.Location = new System.Drawing.Point(12, 60);
            this.groupBoxDatabase.Name = "groupBoxDatabase";
            this.groupBoxDatabase.Size = new System.Drawing.Size(310, 307);
            this.groupBoxDatabase.TabIndex = 7;
            this.groupBoxDatabase.TabStop = false;
            this.groupBoxDatabase.Text = "DataBase.xml";
            this.groupBoxDatabase.Visible = false;
            // 
            // buttonOptions
            // 
            this.buttonOptions.Image = global::FirmwareManager.Properties.Resources.png_transparent_computer_icons_others_miscellaneous_engineer_electric_blue;
            this.buttonOptions.Location = new System.Drawing.Point(264, 260);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(40, 40);
            this.buttonOptions.TabIndex = 6;
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
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
            // buttonWriteToUSB
            // 
            this.buttonWriteToUSB.Location = new System.Drawing.Point(98, 259);
            this.buttonWriteToUSB.Name = "buttonWriteToUSB";
            this.buttonWriteToUSB.Size = new System.Drawing.Size(111, 41);
            this.buttonWriteToUSB.TabIndex = 4;
            this.buttonWriteToUSB.Text = "Записать файл на носитель";
            this.buttonWriteToUSB.UseVisualStyleBackColor = true;
            this.buttonWriteToUSB.Click += new System.EventHandler(this.buttonWriteToUSB_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Controls.Add(this.buttonUninstallServer);
            this.groupBoxOptions.Controls.Add(this.buttonToggleServer);
            this.groupBoxOptions.Controls.Add(this.labelServerStatus);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 373);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(310, 81);
            this.groupBoxOptions.TabIndex = 8;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Настройки сервера";
            // 
            // buttonToggleServer
            // 
            this.buttonToggleServer.Location = new System.Drawing.Point(203, 10);
            this.buttonToggleServer.Name = "buttonToggleServer";
            this.buttonToggleServer.Size = new System.Drawing.Size(101, 35);
            this.buttonToggleServer.TabIndex = 4;
            this.buttonToggleServer.Text = "Запустить сервер";
            this.buttonToggleServer.UseVisualStyleBackColor = true;
            this.buttonToggleServer.Click += new System.EventHandler(this.buttonToggleServer_Click);
            // 
            // labelServerStatus
            // 
            this.labelServerStatus.AutoSize = true;
            this.labelServerStatus.Location = new System.Drawing.Point(95, 34);
            this.labelServerStatus.Name = "labelServerStatus";
            this.labelServerStatus.Size = new System.Drawing.Size(35, 13);
            this.labelServerStatus.TabIndex = 3;
            this.labelServerStatus.Text = "label4";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Статус сервера:";
            // 
            // buttonUninstallServer
            // 
            this.buttonUninstallServer.Location = new System.Drawing.Point(203, 47);
            this.buttonUninstallServer.Name = "buttonUninstallServer";
            this.buttonUninstallServer.Size = new System.Drawing.Size(101, 28);
            this.buttonUninstallServer.TabIndex = 5;
            this.buttonUninstallServer.Text = "Удалить сервер";
            this.buttonUninstallServer.UseVisualStyleBackColor = true;
            this.buttonUninstallServer.Click += new System.EventHandler(this.buttonUninstallServer_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 461);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.groupBoxDatabase);
            this.Controls.Add(this.buttonSelectDataBase);
            this.Controls.Add(this.buttonCreateDataBase);
            this.MaximumSize = new System.Drawing.Size(350, 500);
            this.MinimumSize = new System.Drawing.Size(350, 100);
            this.Name = "MainForm";
            this.Text = "Управление файлами прошивок";
            this.groupBoxDatabase.ResumeLayout(false);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCreateDataBase;
        private System.Windows.Forms.Button buttonOpenBinFile;
        private System.Windows.Forms.Button buttonOpenHexFile;
        private System.Windows.Forms.Button buttonSelectDataBase;
        private System.Windows.Forms.GroupBox groupBoxDatabase;
        private System.Windows.Forms.Button buttonWriteToUSB;
        private System.Windows.Forms.ListView listViewFirmwares;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.Button buttonToggleServer;
        private System.Windows.Forms.Label labelServerStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonUninstallServer;
    }
}

