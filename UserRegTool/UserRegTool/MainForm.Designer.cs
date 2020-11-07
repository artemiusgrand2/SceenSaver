using System;
using System.Drawing;
using System.Windows.Forms;
using Authentificator;

namespace UserRegTool
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        // ---------------------------- Initializing Users List -------------------------------

        /// <summary>
        /// Sets the initial state of the list view component, that is used for displaying registered users
        /// </summary>
        private void InitializeUsersList()
        {
            // Preventing the Users List View control from drawing (temporarily)
            this.registeredUsersListView.BeginUpdate();

            try
            {
                // Clearing the table where we will display data
                this.registeredUsersListView.Items.Clear();

                // Setting values of control's properties
                this.registeredUsersListView.FullRowSelect = true;
                //this.registeredUsersListView.GridLines = true;
                this.registeredUsersListView.HideSelection = false;
                this.registeredUsersListView.MultiSelect = false;
                // Sorting the records in the users list by user name
                this.registeredUsersListView.Sorting = SortOrder.Ascending;
                this.registeredUsersListView.View = View.Details;

                // Create columns for the items and sub-items
                this.registeredUsersListView.Columns.Add("Логин пользователя", -2, HorizontalAlignment.Left);
                this.registeredUsersListView.Columns.Add("Имя пользователя", -2, HorizontalAlignment.Left);
                this.registeredUsersListView.Columns.Add("Срок действия пароля", -2, HorizontalAlignment.Left);
                this.registeredUsersListView.Columns.Add("Блокировка пароля", Screen.PrimaryScreen.WorkingArea.Width, HorizontalAlignment.Left);

            } // try
            finally
            {
                // Resuming the drawing of the registeredUsersListView control
                this.registeredUsersListView.EndUpdate();

            } // finally

        } // InitializeUsersList


        /// <summary>
        /// Adds an item in the Users List View
        /// </summary>
        /// 
        /// <param name="inp_userInfo">registered user's info</param>
        /// 
        /// <returns>the new item</returns>
        /// 
        /// <exception cref="Exception.SystemException.InvalidOperationException">Error adding a new item in the list view component</exception>
        private ListViewItem AddUsersListViewItem(UserAuthentData inp_userInfo)
        {
            if ((inp_userInfo == null) || (String.IsNullOrEmpty(inp_userInfo.Login) == true))
                return null; // nothing to add

            // Preventing the Users List View control from drawing (temporarily)
            this.registeredUsersListView.BeginUpdate();

            ListViewItem _lwi = null;

            try
            {
                // Creating an item to add
                _lwi = new ListViewItem(inp_userInfo.Login);
                // Adding subitems
                _lwi.SubItems.Add(inp_userInfo.UserName);
                _lwi.SubItems.Add(inp_userInfo.DateTimeWhenPasswordEnds.ToString());
                _lwi.SubItems.Add(inp_userInfo.IsPasswordBlocked.ToString());

                // The name of the item in the users list view must be unique
                _lwi.Name = inp_userInfo.Login;

                // New item is added at the end of the list (by default) - here we do not catch any exception
                this.registeredUsersListView.Items.Add(_lwi);
            } // try
            finally
            {
                // Resuming the drawing of the registeredUsersListView control
                this.registeredUsersListView.EndUpdate();
            } // finally

            return _lwi;

        } // AddUsersListViewItem


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.addUserBtn = new System.Windows.Forms.Button();
            this.editUserPwdBtn = new System.Windows.Forms.Button();
            this.delUserBtn = new System.Windows.Forms.Button();
            this.registeredUsersListView = new System.Windows.Forms.ListView();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.userLoginTextBox = new System.Windows.Forms.TextBox();
            this.passwordLbl = new System.Windows.Forms.Label();
            this.userNameLbl = new System.Windows.Forms.Label();
            this.clearFieldsBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dateWhenPasswordEnds = new System.Windows.Forms.DateTimePicker();
            this.timeWhenPasswordEnds = new System.Windows.Forms.DateTimePicker();
            this.blockUserPwdBtn = new System.Windows.Forms.Button();
            this.unblockUserPwdBtn = new System.Windows.Forms.Button();
            this.editPwdEndDateBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.rfidcodTextBox = new System.Windows.Forms.TextBox();
            this.editUserRfidCod = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.userUserNameTextBox = new System.Windows.Forms.TextBox();
            this.editUserNameBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // addUserBtn
            // 
            this.addUserBtn.Location = new System.Drawing.Point(20, 206);
            this.addUserBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.addUserBtn.Name = "addUserBtn";
            this.addUserBtn.Size = new System.Drawing.Size(208, 28);
            this.addUserBtn.TabIndex = 5;
            this.addUserBtn.Text = "Добавить пользователя";
            this.addUserBtn.UseVisualStyleBackColor = true;
            this.addUserBtn.Click += new System.EventHandler(this.addUserBtn_Click);
            // 
            // editUserPwdBtn
            // 
            this.editUserPwdBtn.Location = new System.Drawing.Point(460, 74);
            this.editUserPwdBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.editUserPwdBtn.Name = "editUserPwdBtn";
            this.editUserPwdBtn.Size = new System.Drawing.Size(95, 28);
            this.editUserPwdBtn.TabIndex = 6;
            this.editUserPwdBtn.Text = "Изменить";
            this.editUserPwdBtn.UseVisualStyleBackColor = true;
            this.editUserPwdBtn.Click += new System.EventHandler(this.editUserPwdBtn_Click);
            // 
            // delUserBtn
            // 
            this.delUserBtn.Location = new System.Drawing.Point(232, 463);
            this.delUserBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.delUserBtn.Name = "delUserBtn";
            this.delUserBtn.Size = new System.Drawing.Size(220, 28);
            this.delUserBtn.TabIndex = 10;
            this.delUserBtn.Text = "Удалить учетную запись";
            this.delUserBtn.UseVisualStyleBackColor = true;
            this.delUserBtn.Click += new System.EventHandler(this.delUserBtn_Click);
            // 
            // registeredUsersListView
            // 
            this.registeredUsersListView.HideSelection = false;
            this.registeredUsersListView.Location = new System.Drawing.Point(20, 241);
            this.registeredUsersListView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.registeredUsersListView.Name = "registeredUsersListView";
            this.registeredUsersListView.Size = new System.Drawing.Size(532, 213);
            this.registeredUsersListView.TabIndex = 7;
            this.registeredUsersListView.UseCompatibleStateImageBehavior = false;
            this.registeredUsersListView.SelectedIndexChanged += new System.EventHandler(this.registeredUsersListView_SelectedIndexChanged);
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(205, 78);
            this.passwordTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '*';
            this.passwordTextBox.Size = new System.Drawing.Size(245, 22);
            this.passwordTextBox.TabIndex = 1;
            // 
            // userLoginTextBox
            // 
            this.userLoginTextBox.Location = new System.Drawing.Point(205, 15);
            this.userLoginTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.userLoginTextBox.Name = "userLoginTextBox";
            this.userLoginTextBox.Size = new System.Drawing.Size(245, 22);
            this.userLoginTextBox.TabIndex = 0;
            // 
            // passwordLbl
            // 
            this.passwordLbl.AutoSize = true;
            this.passwordLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.passwordLbl.Location = new System.Drawing.Point(133, 78);
            this.passwordLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.passwordLbl.Name = "passwordLbl";
            this.passwordLbl.Size = new System.Drawing.Size(61, 17);
            this.passwordLbl.TabIndex = 7;
            this.passwordLbl.Text = "Пароль:";
            // 
            // userNameLbl
            // 
            this.userNameLbl.AutoSize = true;
            this.userNameLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.userNameLbl.Location = new System.Drawing.Point(143, 18);
            this.userNameLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.userNameLbl.Name = "userNameLbl";
            this.userNameLbl.Size = new System.Drawing.Size(51, 17);
            this.userNameLbl.TabIndex = 6;
            this.userNameLbl.Text = "Логин:";
            // 
            // clearFieldsBtn
            // 
            this.clearFieldsBtn.Location = new System.Drawing.Point(372, 176);
            this.clearFieldsBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.clearFieldsBtn.Name = "clearFieldsBtn";
            this.clearFieldsBtn.Size = new System.Drawing.Size(181, 28);
            this.clearFieldsBtn.TabIndex = 4;
            this.clearFieldsBtn.Text = "Очистить поля ввода";
            this.clearFieldsBtn.UseVisualStyleBackColor = true;
            this.clearFieldsBtn.Click += new System.EventHandler(this.clearFieldsBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 144);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Пароль действителен до:";
            // 
            // dateWhenPasswordEnds
            // 
            this.dateWhenPasswordEnds.Location = new System.Drawing.Point(205, 144);
            this.dateWhenPasswordEnds.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dateWhenPasswordEnds.Name = "dateWhenPasswordEnds";
            this.dateWhenPasswordEnds.Size = new System.Drawing.Size(117, 22);
            this.dateWhenPasswordEnds.TabIndex = 2;
            // 
            // timeWhenPasswordEnds
            // 
            this.timeWhenPasswordEnds.Location = new System.Drawing.Point(332, 144);
            this.timeWhenPasswordEnds.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.timeWhenPasswordEnds.Name = "timeWhenPasswordEnds";
            this.timeWhenPasswordEnds.ShowUpDown = true;
            this.timeWhenPasswordEnds.Size = new System.Drawing.Size(119, 22);
            this.timeWhenPasswordEnds.TabIndex = 3;
            // 
            // blockUserPwdBtn
            // 
            this.blockUserPwdBtn.Location = new System.Drawing.Point(16, 463);
            this.blockUserPwdBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.blockUserPwdBtn.Name = "blockUserPwdBtn";
            this.blockUserPwdBtn.Size = new System.Drawing.Size(208, 28);
            this.blockUserPwdBtn.TabIndex = 8;
            this.blockUserPwdBtn.Text = "Блокировать пароль";
            this.blockUserPwdBtn.UseVisualStyleBackColor = true;
            this.blockUserPwdBtn.Click += new System.EventHandler(this.blockUserPwdBtn_Click);
            // 
            // unblockUserPwdBtn
            // 
            this.unblockUserPwdBtn.Location = new System.Drawing.Point(16, 498);
            this.unblockUserPwdBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.unblockUserPwdBtn.Name = "unblockUserPwdBtn";
            this.unblockUserPwdBtn.Size = new System.Drawing.Size(208, 28);
            this.unblockUserPwdBtn.TabIndex = 9;
            this.unblockUserPwdBtn.Text = "Разблокировать пароль";
            this.unblockUserPwdBtn.UseVisualStyleBackColor = true;
            this.unblockUserPwdBtn.Click += new System.EventHandler(this.unblockUserPwdBtn_Click);
            // 
            // editPwdEndDateBtn
            // 
            this.editPwdEndDateBtn.Location = new System.Drawing.Point(460, 140);
            this.editPwdEndDateBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.editPwdEndDateBtn.Name = "editPwdEndDateBtn";
            this.editPwdEndDateBtn.Size = new System.Drawing.Size(95, 28);
            this.editPwdEndDateBtn.TabIndex = 11;
            this.editPwdEndDateBtn.Text = "Изменить";
            this.editPwdEndDateBtn.UseVisualStyleBackColor = true;
            this.editPwdEndDateBtn.Click += new System.EventHandler(this.editPwdEndDateBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(97, 110);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 17);
            this.label2.TabIndex = 12;
            this.label2.Text = "Код карточки";
            // 
            // rfidcodTextBox
            // 
            this.rfidcodTextBox.Location = new System.Drawing.Point(205, 110);
            this.rfidcodTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rfidcodTextBox.Name = "rfidcodTextBox";
            this.rfidcodTextBox.Size = new System.Drawing.Size(245, 22);
            this.rfidcodTextBox.TabIndex = 13;
            // 
            // editUserRfidCod
            // 
            this.editUserRfidCod.Location = new System.Drawing.Point(459, 107);
            this.editUserRfidCod.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.editUserRfidCod.Name = "editUserRfidCod";
            this.editUserRfidCod.Size = new System.Drawing.Size(95, 28);
            this.editUserRfidCod.TabIndex = 14;
            this.editUserRfidCod.Text = "Изменить";
            this.editUserRfidCod.UseVisualStyleBackColor = true;
            this.editUserRfidCod.Click += new System.EventHandler(this.editUserRfidCod_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 49);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 17);
            this.label3.TabIndex = 15;
            this.label3.Text = "Имя пользователя:";
            // 
            // userUserNameTextBox
            // 
            this.userUserNameTextBox.Location = new System.Drawing.Point(205, 46);
            this.userUserNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.userUserNameTextBox.Name = "userUserNameTextBox";
            this.userUserNameTextBox.Size = new System.Drawing.Size(244, 22);
            this.userUserNameTextBox.TabIndex = 16;
            // 
            // editUserNameBtn
            // 
            this.editUserNameBtn.Location = new System.Drawing.Point(460, 43);
            this.editUserNameBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.editUserNameBtn.Name = "editUserNameBtn";
            this.editUserNameBtn.Size = new System.Drawing.Size(95, 28);
            this.editUserNameBtn.TabIndex = 17;
            this.editUserNameBtn.Text = "Изменить";
            this.editUserNameBtn.UseVisualStyleBackColor = true;
            this.editUserNameBtn.Click += new System.EventHandler(this.editUserNameBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MediumAquamarine;
            this.ClientSize = new System.Drawing.Size(576, 608);
            this.Controls.Add(this.editUserNameBtn);
            this.Controls.Add(this.userUserNameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.editUserRfidCod);
            this.Controls.Add(this.rfidcodTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.editPwdEndDateBtn);
            this.Controls.Add(this.unblockUserPwdBtn);
            this.Controls.Add(this.blockUserPwdBtn);
            this.Controls.Add(this.timeWhenPasswordEnds);
            this.Controls.Add(this.dateWhenPasswordEnds);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.clearFieldsBtn);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.userLoginTextBox);
            this.Controls.Add(this.passwordLbl);
            this.Controls.Add(this.userNameLbl);
            this.Controls.Add(this.registeredUsersListView);
            this.Controls.Add(this.delUserBtn);
            this.Controls.Add(this.editUserPwdBtn);
            this.Controls.Add(this.addUserBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Администратор аутентификационных данных";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button addUserBtn;
        private System.Windows.Forms.Button editUserPwdBtn;
        private System.Windows.Forms.Button delUserBtn;
        private System.Windows.Forms.ListView registeredUsersListView;
        public TextBox passwordTextBox;
        public TextBox userLoginTextBox;
        private Label passwordLbl;
        private Label userNameLbl;
        private Button clearFieldsBtn;
        private Label label1;
        private DateTimePicker dateWhenPasswordEnds;
        private DateTimePicker timeWhenPasswordEnds;
        private Button blockUserPwdBtn;
        private Button unblockUserPwdBtn;
        private Button editPwdEndDateBtn;
        private Label label2;
        public TextBox rfidcodTextBox;
        private Button editUserRfidCod;
        private Label label3;
        private TextBox userUserNameTextBox;
        private Button editUserNameBtn;
    }
}

