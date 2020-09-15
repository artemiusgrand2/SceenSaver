using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WorkWithUsers;
using System.Configuration;
using System.IO;

namespace UserRegTool
{
    public partial class MainForm : Form
    {
        #region Private Static Attributes
        // ------------------------------

        /// <summary>
        /// ��������� ������, ���������������� ��� ������ � ������������������� ������� �������������
        /// </summary>
        private static Authentificator auth = null;

        /// <summary>
        /// ���� = true, �� ��� �������� ������� ����� ����������, �������� ���������� ����������
        /// </summary>
        private static bool exitApplicationOnFormLoad = false;

        /// <summary>
        /// ������ ���� � ����� ������������������ ������ �������������
        /// </summary>
        private static string userAuthentDataFilePath = String.Empty;

        /// <summary>
        /// ����� (� �����), � ������� �������� ��������� ����� ����������� ������� ������������ (� ������, ��� ������).
        /// �� ��������� ������ ��������� � ������� ����
        /// </summary>
        private static int userPasswordDefLifeTime = 1;

        #endregion


        #region Public Methods
        // -------------------

        /// <summary>
        /// ����������� �������� ����� ����������
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();
            this.InitializeUsersList();

            try
            {
                // ��������� �� ����������������� ����� ������ ���� � ����� ������������������ ������ �������������
                // ------------------------------------------------------------------------------------------------
                userAuthentDataFilePath = ConfigurationManager.AppSettings[@"userAuthentDataFilePath"];

                // ��������� �� ����������������� ����� ����� �������� ������ ������������
                // -----------------------------------------------------------------------
                if ((Int32.TryParse(ConfigurationManager.AppSettings[@"userPasswordDefLifeTime"], out userPasswordDefLifeTime)) == false)
                    userPasswordDefLifeTime = 1; // by default
                // The lifetime cannot be less then 0!
                if (userPasswordDefLifeTime < 0)
                    userPasswordDefLifeTime = 0;             

                // �������� ������� ��������� ������, ���������������� ��� ������ � ������������������� ������� �������������
                // ----------------------------------------------------------------------------------------------------------
                auth = new Authentificator(userAuthentDataFilePath, Authentificator.AuthertificatorActions.All);

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // ������������� ���� ������ �������� ����������
                exitApplicationOnFormLoad = true;
                return;

            } // catch

            // ���������� �������� �� ��������� ��� ����� �� �����
            // ---------------------------------------------------
            DateTime _userPasswordDefTimeEnd = DateTime.Now + new TimeSpan(userPasswordDefLifeTime, 0, 0);

            this.dateWhenPasswordEnds.Format = DateTimePickerFormat.Custom; // ���� ����� ���������� ������ ����
            this.dateWhenPasswordEnds.Value = _userPasswordDefTimeEnd; // ��������� ���� ��������� ����� �������� ������ �� ��������� - � ������������ � ����������� ��������� 

            this.timeWhenPasswordEnds.Format = DateTimePickerFormat.Time; // ���� ����� ���������� ������ �����
            this.timeWhenPasswordEnds.Value = _userPasswordDefTimeEnd; // ��������� ������� ��������� ����� �������� ������ �� ��������� - � ������������ � ����������� ��������� 

        } // constructor


        /// <summary>
        /// ��������, ����������� � ������ �������� ������� ����� ����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // ���� ���� ������ �������� ���������� ����������, �� �������� �������� ����������
            // --------------------------------------------------------------------------------
            if (exitApplicationOnFormLoad == true)
            {
                // Exiting application
                Application.Exit();
                return;

            } // if

            // ��������� � ���������� ��� ����� ������������� (������������������)
            // -------------------------------------------------------------------
            try
            {
                this.RefreshUsersList();
            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Exiting application
                Application.Exit();
                return;

            } // catch

        } // MainForm_Load


        /// <summary>
        /// Updates a list of registered users
        /// </summary>
        /// 
        /// <exception cref="System.Exception">any exception wjile performing method</exception>
        private void RefreshUsersList()
        {
            // ��������� ���������� ��� ���� ������������������ �������������
            // --------------------------------------------------------------
            List<UserAuthentData> _allRegisteredUsers = new List<UserAuthentData>();

            if (File.Exists(userAuthentDataFilePath) == true)
            {
                try
                {
                    _allRegisteredUsers = auth.GetAllUsersInfo();

                } // try
                catch (Exception _ex)
                {
                    // ������� ��������� �� ������
                    MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // ������������� ���� ������ �������� ����������
                    exitApplicationOnFormLoad = true;

                } // catch

            } // if

            // ���������� ���������� ��� ���� ������������������ �������������
            // ---------------------------------------------------------------
            this.registeredUsersListView.Items.Clear();

            if (_allRegisteredUsers != null)
            {
                foreach (UserAuthentData _userData in _allRegisteredUsers)
                    this.AddUsersListViewItem(_userData);

            } // if

        } // RefreshUsersList


        /// <summary>
        /// ���������� ������ ������������ (����������� ������������)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addUserBtn_Click(object sender, EventArgs e)
        {
            DateTime _dt = new DateTime(this.dateWhenPasswordEnds.Value.Year, this.dateWhenPasswordEnds.Value.Month, this.dateWhenPasswordEnds.Value.Day,
                                        this.timeWhenPasswordEnds.Value.Hour, this.timeWhenPasswordEnds.Value.Minute, this.timeWhenPasswordEnds.Value.Second);
            UserAuthentData _userData = new UserAuthentData(this.userNameTextBox.Text, this.passwordTextBox.Text, false, _dt);

            try
            {
                // ���������� ������ ������������
                auth.SaveUserAuthentData(_userData);

                MessageBox.Show(this, "������������ " + this.userNameTextBox.Text + " ������� ���������������",
                    "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // ���������� ������ �������������
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // addUserBtn_Click


        /// <summary>
        /// �������������� ������ ������������� ������������ (������� �������������������)
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void editUserPwdBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // �������������� ������ � ������������� ������������
                auth.EditPassword(this.userNameTextBox.Text, this.passwordTextBox.Text);

                MessageBox.Show(this, "������ ������� �������", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // ���������� ������ �������������
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // editUserPwdBtn_Click


        /// <summary>
        /// �������������� ����� �������� ������ ������������� ������������ (������� �������������������)
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void editPwdEndDateBtn_Click(object sender, EventArgs e)
        {
            DateTime _dt = new DateTime(this.dateWhenPasswordEnds.Value.Year, this.dateWhenPasswordEnds.Value.Month, this.dateWhenPasswordEnds.Value.Day,
                                        this.timeWhenPasswordEnds.Value.Hour, this.timeWhenPasswordEnds.Value.Minute, this.timeWhenPasswordEnds.Value.Second);

            try
            {
                // �������������� ����� �������� ������ � ������������� ������������
                auth.EditPasswordAction(this.userNameTextBox.Text, _dt);

                MessageBox.Show(this, "���� �������� ������ ������������ " + this.userNameTextBox.Text + " ������� �������",
                    "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // ���������� ������ �������������
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // editPwdEndDateBtn_Click


        /// <summary>
        /// �������� ������������� ������������ (������� �������������������)
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void delUserBtn_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection _selectedItems = this.registeredUsersListView.SelectedItems;

                if (_selectedItems.Count > 0)
                {
                    if (MessageBox.Show(this, "������� ������� ������ ������������ " + _selectedItems[0].Name + "?",
                        "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // �������� ������������� ������������
                        auth.DeleteUser(_selectedItems[0].Name);

                        this.clearFieldsBtn_Click(null, null);

                        // ���������� ������ �������������
                        this.RefreshUsersList();

                    } // if

                } // if

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // delUserBtn_Click


        /// <summary>
        /// ������� ����� ����� ������������������ ������ �������������
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void clearFieldsBtn_Click(object sender, EventArgs e)
        {
            this.userNameTextBox.Text = String.Empty;
            this.passwordTextBox.Text = String.Empty;

            DateTime _userPasswordDefTimeEnd = DateTime.Now + new TimeSpan(userPasswordDefLifeTime, 0, 0);
            this.dateWhenPasswordEnds.Value = _userPasswordDefTimeEnd;
            this.timeWhenPasswordEnds.Value = _userPasswordDefTimeEnd;

            this.blockUserPwdBtn.Enabled = true;
            this.unblockUserPwdBtn.Enabled = true;

        } // clearFieldsBtn_Click


        /// <summary>
        /// ����������� � ��������������� ����� ���������� � ���������� � ������� ������������
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void registeredUsersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection _selectedItems = this.registeredUsersListView.SelectedItems;

                if (_selectedItems.Count > 0)
                {
                    this.userNameTextBox.Text = _selectedItems[0].Name;

                    DateTime _dt = Convert.ToDateTime(_selectedItems[0].SubItems[1].Text);

                    this.dateWhenPasswordEnds.Value = _dt;
                    this.timeWhenPasswordEnds.Value = _dt;

                    this.blockUserPwdBtn.Enabled = !Convert.ToBoolean(_selectedItems[0].SubItems[2].Text); // ����������/������ ���������� ������
                    this.unblockUserPwdBtn.Enabled = !this.blockUserPwdBtn.Enabled; // ����������/������ ������������� ������

                } // if
                else
                {
                    this.blockUserPwdBtn.Enabled = true;
                    this.unblockUserPwdBtn.Enabled = true;

                } // else

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // registeredUsersListView_SelectedIndexChanged


        /// <summary>
        /// ��������� ������ ���������� ������������
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void blockUserPwdBtn_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection _selectedItems = this.registeredUsersListView.SelectedItems;

                if (_selectedItems.Count > 0)
                {
                    auth.EditPasswordStatus(_selectedItems[0].Name, true);

                    MessageBox.Show(this, "������ ������������ " + _selectedItems[0].Name + " ��� ������������",
                        "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.clearFieldsBtn_Click(null, null);

                    // ���������� ������ �������������
                    this.RefreshUsersList();

                } // if

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // blockUserPwdBtn_Click


        /// <summary>
        /// ������� ���������� ������ ���������� ������������
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void unblockUserPwdBtn_Click(object sender, EventArgs e)
        {
            try
            {
                ListView.SelectedListViewItemCollection _selectedItems = this.registeredUsersListView.SelectedItems;

                if (_selectedItems.Count > 0)
                {
                    auth.EditPasswordStatus(_selectedItems[0].Name, false);

                    MessageBox.Show(this, "������ ������������ " + _selectedItems[0].Name + " ��� �������������",
                        "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.clearFieldsBtn_Click(null, null);

                    // ���������� ������ �������������
                    this.RefreshUsersList();

                } // if

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // unblockUserPwdBtn_Click

        #endregion

    } // class MainForm

} // namespace UserRegTool