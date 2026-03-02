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
            this.buttonWriteToUSB = new System.Windows.Forms.Button();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.buttonUninstallServer = new System.Windows.Forms.Button();
            this.buttonToggleServer = new System.Windows.Forms.Button();
            this.labelServerStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxDBOptions = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxHex = new System.Windows.Forms.CheckBox();
            this.checkBoxBin = new System.Windows.Forms.CheckBox();
            this.checkBoxIso = new System.Windows.Forms.CheckBox();
            this.checkBoxImg = new System.Windows.Forms.CheckBox();
            this.buttonDeleteDB = new System.Windows.Forms.Button();
            this.groupBoxDatabase.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.groupBoxDBOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCreateDataBase
            // 
            this.buttonCreateDataBase.Location = new System.Drawing.Point(151, 12);
            this.buttonCreateDataBase.Name = "buttonCreateDataBase";
            this.buttonCreateDataBase.Size = new System.Drawing.Size(123, 42);
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
            this.buttonSelectDataBase.Location = new System.Drawing.Point(18, 12);
            this.buttonSelectDataBase.Name = "buttonSelectDataBase";
            this.buttonSelectDataBase.Size = new System.Drawing.Size(127, 42);
            this.buttonSelectDataBase.TabIndex = 4;
            this.buttonSelectDataBase.Text = "Выбрать базу данных";
            this.buttonSelectDataBase.UseVisualStyleBackColor = true;
            this.buttonSelectDataBase.Click += new System.EventHandler(this.buttonSelectDatabase_Click);
            // 
            // groupBoxDatabase
            // 
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
            this.buttonWriteToUSB.Location = new System.Drawing.Point(102, 259);
            this.buttonWriteToUSB.Name = "buttonWriteToUSB";
            this.buttonWriteToUSB.Size = new System.Drawing.Size(111, 41);
            this.buttonWriteToUSB.TabIndex = 4;
            this.buttonWriteToUSB.Text = "Записать файл на носитель";
            this.buttonWriteToUSB.UseVisualStyleBackColor = true;
            this.buttonWriteToUSB.Click += new System.EventHandler(this.buttonWriteToUSB_Click);
            // 
            // buttonOptions
            // 
            this.buttonOptions.Image = global::FirmwareManager.Properties.Resources.png_transparent_computer_icons_others_miscellaneous_engineer_electric_blue;
            this.buttonOptions.Location = new System.Drawing.Point(280, 12);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(42, 42);
            this.buttonOptions.TabIndex = 6;
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Controls.Add(this.buttonUninstallServer);
            this.groupBoxOptions.Controls.Add(this.buttonToggleServer);
            this.groupBoxOptions.Controls.Add(this.labelServerStatus);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Location = new System.Drawing.Point(335, 12);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(310, 81);
            this.groupBoxOptions.TabIndex = 8;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Настройки сервера";
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
            // groupBoxDBOptions
            // 
            this.groupBoxDBOptions.Controls.Add(this.buttonDeleteDB);
            this.groupBoxDBOptions.Controls.Add(this.checkBoxImg);
            this.groupBoxDBOptions.Controls.Add(this.checkBoxIso);
            this.groupBoxDBOptions.Controls.Add(this.checkBoxBin);
            this.groupBoxDBOptions.Controls.Add(this.checkBoxHex);
            this.groupBoxDBOptions.Controls.Add(this.label1);
            this.groupBoxDBOptions.Location = new System.Drawing.Point(335, 99);
            this.groupBoxDBOptions.Name = "groupBoxDBOptions";
            this.groupBoxDBOptions.Size = new System.Drawing.Size(310, 268);
            this.groupBoxDBOptions.TabIndex = 9;
            this.groupBoxDBOptions.TabStop = false;
            this.groupBoxDBOptions.Text = "Настройки базы данных";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Расширения файлов:";
            // 
            // checkBoxHex
            // 
            this.checkBoxHex.AutoSize = true;
            this.checkBoxHex.Checked = true;
            this.checkBoxHex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHex.Location = new System.Drawing.Point(9, 55);
            this.checkBoxHex.Name = "checkBoxHex";
            this.checkBoxHex.Size = new System.Drawing.Size(46, 17);
            this.checkBoxHex.TabIndex = 1;
            this.checkBoxHex.Text = ".hex";
            this.checkBoxHex.UseVisualStyleBackColor = true;
            // 
            // checkBoxBin
            // 
            this.checkBoxBin.AutoSize = true;
            this.checkBoxBin.Checked = true;
            this.checkBoxBin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBin.Location = new System.Drawing.Point(9, 78);
            this.checkBoxBin.Name = "checkBoxBin";
            this.checkBoxBin.Size = new System.Drawing.Size(43, 17);
            this.checkBoxBin.TabIndex = 2;
            this.checkBoxBin.Text = ".bin";
            this.checkBoxBin.UseVisualStyleBackColor = true;
            // 
            // checkBoxIso
            // 
            this.checkBoxIso.AutoSize = true;
            this.checkBoxIso.Checked = true;
            this.checkBoxIso.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIso.Location = new System.Drawing.Point(9, 102);
            this.checkBoxIso.Name = "checkBoxIso";
            this.checkBoxIso.Size = new System.Drawing.Size(42, 17);
            this.checkBoxIso.TabIndex = 3;
            this.checkBoxIso.Text = ".iso";
            this.checkBoxIso.UseVisualStyleBackColor = true;
            // 
            // checkBoxImg
            // 
            this.checkBoxImg.AutoSize = true;
            this.checkBoxImg.Checked = true;
            this.checkBoxImg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxImg.Location = new System.Drawing.Point(9, 126);
            this.checkBoxImg.Name = "checkBoxImg";
            this.checkBoxImg.Size = new System.Drawing.Size(45, 17);
            this.checkBoxImg.TabIndex = 4;
            this.checkBoxImg.Text = ".img";
            this.checkBoxImg.UseVisualStyleBackColor = true;
            // 
            // buttonDeleteDB
            // 
            this.buttonDeleteDB.Location = new System.Drawing.Point(203, 220);
            this.buttonDeleteDB.Name = "buttonDeleteDB";
            this.buttonDeleteDB.Size = new System.Drawing.Size(101, 41);
            this.buttonDeleteDB.TabIndex = 7;
            this.buttonDeleteDB.Text = "Удалить базу данных";
            this.buttonDeleteDB.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 376);
            this.Controls.Add(this.groupBoxDBOptions);
            this.Controls.Add(this.buttonOptions);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.groupBoxDatabase);
            this.Controls.Add(this.buttonSelectDataBase);
            this.Controls.Add(this.buttonCreateDataBase);
            this.MaximumSize = new System.Drawing.Size(670, 500);
            this.MinimumSize = new System.Drawing.Size(350, 140);
            this.Name = "MainForm";
            this.Text = "Управление файлами прошивок";
            this.groupBoxDatabase.ResumeLayout(false);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.groupBoxDBOptions.ResumeLayout(false);
            this.groupBoxDBOptions.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBoxDBOptions;
        private System.Windows.Forms.CheckBox checkBoxImg;
        private System.Windows.Forms.CheckBox checkBoxIso;
        private System.Windows.Forms.CheckBox checkBoxBin;
        private System.Windows.Forms.CheckBox checkBoxHex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonDeleteDB;
    }
}

