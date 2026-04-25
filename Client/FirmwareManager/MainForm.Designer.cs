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
            this.buttonAddFile = new System.Windows.Forms.Button();
            this.buttonDeleteSelected = new System.Windows.Forms.Button();
            this.groupBoxDatabase = new System.Windows.Forms.GroupBox();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.listViewFirmwares = new System.Windows.Forms.ListView();
            this.buttonWriteToUSB = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelDatabaseStatus = new System.Windows.Forms.Label();
            this.buttonToggleServer = new System.Windows.Forms.Button();
            this.labelFlashServiceStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxDatabase.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonAddFile
            // 
            this.buttonAddFile.Location = new System.Drawing.Point(64, 260);
            this.buttonAddFile.Name = "buttonAddFile";
            this.buttonAddFile.Size = new System.Drawing.Size(111, 41);
            this.buttonAddFile.TabIndex = 2;
            this.buttonAddFile.Text = "Добавить файл";
            this.buttonAddFile.UseVisualStyleBackColor = true;
            this.buttonAddFile.Click += new System.EventHandler(this.buttonAddFile_Click);
            // 
            // buttonDeleteSelected
            // 
            this.buttonDeleteSelected.Location = new System.Drawing.Point(181, 260);
            this.buttonDeleteSelected.Name = "buttonDeleteSelected";
            this.buttonDeleteSelected.Size = new System.Drawing.Size(111, 41);
            this.buttonDeleteSelected.TabIndex = 3;
            this.buttonDeleteSelected.Text = "Удалить файл";
            this.buttonDeleteSelected.UseVisualStyleBackColor = true;
            this.buttonDeleteSelected.Click += new System.EventHandler(this.buttonDeleteSelected_Click);
            // 
            // groupBoxDatabase
            // 
            this.groupBoxDatabase.Controls.Add(this.buttonOptions);
            this.groupBoxDatabase.Controls.Add(this.listViewFirmwares);
            this.groupBoxDatabase.Controls.Add(this.buttonWriteToUSB);
            this.groupBoxDatabase.Controls.Add(this.buttonAddFile);
            this.groupBoxDatabase.Controls.Add(this.buttonDeleteSelected);
            this.groupBoxDatabase.Location = new System.Drawing.Point(12, 12);
            this.groupBoxDatabase.Name = "groupBoxDatabase";
            this.groupBoxDatabase.Size = new System.Drawing.Size(357, 355);
            this.groupBoxDatabase.TabIndex = 7;
            this.groupBoxDatabase.TabStop = false;
            this.groupBoxDatabase.Text = "DataBase.xml";
            this.groupBoxDatabase.Visible = false;
            // 
            // buttonOptions
            // 
            this.buttonOptions.Image = global::FirmwareManager.Properties.Resources.png_transparent_computer_icons_others_miscellaneous_engineer_electric_blue;
            this.buttonOptions.Location = new System.Drawing.Point(309, 307);
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Size = new System.Drawing.Size(42, 42);
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
            this.listViewFirmwares.Size = new System.Drawing.Size(345, 235);
            this.listViewFirmwares.TabIndex = 5;
            this.listViewFirmwares.UseCompatibleStateImageBehavior = false;
            // 
            // buttonWriteToUSB
            // 
            this.buttonWriteToUSB.Location = new System.Drawing.Point(124, 308);
            this.buttonWriteToUSB.Name = "buttonWriteToUSB";
            this.buttonWriteToUSB.Size = new System.Drawing.Size(111, 41);
            this.buttonWriteToUSB.TabIndex = 4;
            this.buttonWriteToUSB.Text = "Записать файл на носитель";
            this.buttonWriteToUSB.UseVisualStyleBackColor = true;
            this.buttonWriteToUSB.Click += new System.EventHandler(this.buttonWriteToUSB_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Controls.Add(this.label3);
            this.groupBoxOptions.Controls.Add(this.labelDatabaseStatus);
            this.groupBoxOptions.Controls.Add(this.buttonToggleServer);
            this.groupBoxOptions.Controls.Add(this.labelFlashServiceStatus);
            this.groupBoxOptions.Controls.Add(this.label2);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 373);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(357, 81);
            this.groupBoxOptions.TabIndex = 8;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Настройки сервера";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Статус сервера БД:";
            // 
            // labelDatabaseStatus
            // 
            this.labelDatabaseStatus.AutoSize = true;
            this.labelDatabaseStatus.Location = new System.Drawing.Point(131, 48);
            this.labelDatabaseStatus.Name = "labelDatabaseStatus";
            this.labelDatabaseStatus.Size = new System.Drawing.Size(16, 13);
            this.labelDatabaseStatus.TabIndex = 6;
            this.labelDatabaseStatus.Text = "...";
            // 
            // buttonToggleServer
            // 
            this.buttonToggleServer.Location = new System.Drawing.Point(250, 26);
            this.buttonToggleServer.Name = "buttonToggleServer";
            this.buttonToggleServer.Size = new System.Drawing.Size(101, 35);
            this.buttonToggleServer.TabIndex = 4;
            this.buttonToggleServer.Text = "Установить сервер";
            this.buttonToggleServer.UseVisualStyleBackColor = true;
            this.buttonToggleServer.Click += new System.EventHandler(this.buttonToggleServer_Click);
            // 
            // labelFlashServiceStatus
            // 
            this.labelFlashServiceStatus.AutoSize = true;
            this.labelFlashServiceStatus.Location = new System.Drawing.Point(131, 22);
            this.labelFlashServiceStatus.Name = "labelFlashServiceStatus";
            this.labelFlashServiceStatus.Size = new System.Drawing.Size(16, 13);
            this.labelFlashServiceStatus.TabIndex = 3;
            this.labelFlashServiceStatus.Text = "...";
            this.labelFlashServiceStatus.Click += new System.EventHandler(this.labelFlashServiceStatus_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Статус сервиса записи:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 461);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.groupBoxDatabase);
            this.MaximumSize = new System.Drawing.Size(720, 500);
            this.MinimumSize = new System.Drawing.Size(350, 140);
            this.Name = "MainForm";
            this.Text = "Управление файлами прошивок";
            this.groupBoxDatabase.ResumeLayout(false);
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonAddFile;
        private System.Windows.Forms.Button buttonDeleteSelected;
        private System.Windows.Forms.GroupBox groupBoxDatabase;
        private System.Windows.Forms.Button buttonWriteToUSB;
        private System.Windows.Forms.ListView listViewFirmwares;
        private System.Windows.Forms.Button buttonOptions;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.Label labelFlashServiceStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelDatabaseStatus;
        private System.Windows.Forms.Button buttonToggleServer;
        private System.Windows.Forms.Label label3;
    }
}

