using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Authentificator;
using System.Configuration;

using Authentificator.Enums;

using UserRegTool.RFIDModul;

namespace UserRegTool
{
    public partial class MainForm : Form
    {
        #region Private Static Attributes
        // ------------------------------

        public delegate void UpdateRfidTextDelegate();

        /// <summary>
        /// ��������� ������, ���������������� ��� ������ � ������������������� ������� �������������
        /// </summary>
        private static Authentificators _auth = null;

        /// <summary>
        /// ���� = true, �� ��� �������� ������� ����� ����������, �������� ���������� ����������
        /// </summary>
        private static bool _exitApplicationOnFormLoad = false;

        /// <summary>
        /// ������ ���� � ����� ������������������ ������ �������������
        /// </summary>
        private static string _userAuthentDataFilePath = String.Empty;

        /// <summary>
        /// ����� (� �����), � ������� �������� ��������� ����� ����������� ������� ������������ (� ������, ��� ������).
        /// �� ��������� ������ ��������� � ������� ����
        /// </summary>
        private static int _userPasswordDefLifeTime = 1;

        private IRFIDScan _rfidScan;

        /// <summary>
        /// ��� ������������� �����������
        /// </summary>
        ViewReader _viewReadCard;

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
            this.FormClosed += MainForm_FormClosed;

            try
            {
                // ��������� �� ����������������� ����� ������ ���� � ����� ������������������ ������ �������������
                // ------------------------------------------------------------------------------------------------
                _userAuthentDataFilePath = ConfigurationManager.AppSettings[@"userAuthentDataFilePath"];

                // ��������� �� ����������������� ����� ����� �������� ������ ������������
                // -----------------------------------------------------------------------
                if ((Int32.TryParse(ConfigurationManager.AppSettings[@"userPasswordDefLifeTime"], out _userPasswordDefLifeTime)) == false)
                    _userPasswordDefLifeTime = 1; // by default
                // The lifetime cannot be less then 0!
                if (_userPasswordDefLifeTime < 0)
                    _userPasswordDefLifeTime = 0;//
                //
                if (ConfigurationManager.AppSettings.AllKeys.Contains("viewReader"))
                    _viewReadCard = Authentificator.ParserCommon.GetViewCard(ConfigurationManager.AppSettings["viewReader"]);

                try
                {
                    if (_viewReadCard != ViewReader.smartCard && ConfigurationManager.AppSettings.AllKeys.Contains("serialPortRead"))
                    {

                        string nameSerial;
                        int baudRate;
                        if (ParserCommon.GetNameAndSpeedComPort(ConfigurationManager.AppSettings["serialPortRead"], out nameSerial, out baudRate))
                        {
                            _rfidScan = new SerialRFIDScan(nameSerial, baudRate, _viewReadCard);
                            _rfidScan.EventCardInserted += UpdateRfidText;
                        }
                        else
                        {
                            MessageBox.Show(this, $"�������� ������ ����������� ComPort - {ConfigurationManager.AppSettings["serialPortRead"]}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }



                    }
                    else if (_viewReadCard == ViewReader.smartCard)
                    {
                        _rfidScan = new PCSCRFIDScan();
                        _rfidScan.EventCardInserted += UpdateRfidText;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // �������� ������� ��������� ������, ���������������� ��� ������ � ������������������� ������� �������������
                // ----------------------------------------------------------------------------------------------------------
                _auth = new Authentificators(_userAuthentDataFilePath, Authentificators.AuthertificatorActions.All, string.Empty, string.Empty);

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // ������������� ���� ������ �������� ����������
                _exitApplicationOnFormLoad = true;
                return;

            } // catch

            // ���������� �������� �� ��������� ��� ����� �� �����
            // ---------------------------------------------------
            DateTime _userPasswordDefTimeEnd = DateTime.Now + new TimeSpan(_userPasswordDefLifeTime, 0, 0);

            this.dateWhenPasswordEnds.Format = DateTimePickerFormat.Custom; // ���� ����� ���������� ������ ����
            this.dateWhenPasswordEnds.Value = _userPasswordDefTimeEnd; // ��������� ���� ��������� ����� �������� ������ �� ��������� - � ������������ � ����������� ��������� 

            this.timeWhenPasswordEnds.Format = DateTimePickerFormat.Time; // ���� ����� ���������� ������ �����
            this.timeWhenPasswordEnds.Value = _userPasswordDefTimeEnd; // ��������� ������� ��������� ����� �������� ������ �� ��������� - � ������������ � ����������� ��������� 

        } // constructor

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_rfidScan != null)
                _rfidScan.Stop();
        }

        private void UpdateRfidText(string dataRfid)
        {
            UpdateRfidTextDelegate action = () =>
            {
                if (dataRfid != rfidcodTextBox.Text)
                    rfidcodTextBox.Text = dataRfid;
            };
            this.Invoke(action);
        }

        /// <summary>
        /// ��������, ����������� � ������ �������� ������� ����� ����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // ���� ���� ������ �������� ���������� ����������, �� �������� �������� ����������
            // --------------------------------------------------------------------------------
            if (_exitApplicationOnFormLoad == true)
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

            if (File.Exists(_userAuthentDataFilePath) == true)
            {
                try
                {
                    _allRegisteredUsers = _auth.GetAllUsersInfo();

                } // try
                catch (Exception _ex)
                {
                    // ������� ��������� �� ������
                    MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // ������������� ���� ������ �������� ����������
                    _exitApplicationOnFormLoad = true;

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
            UserAuthentData _userData = new UserAuthentData(this.userLoginTextBox.Text, this.passwordTextBox.Text, this.userUserNameTextBox.Text, false, _dt, this.rfidcodTextBox.Text);

            try
            {
                // ���������� ������ ������������
                _auth.SaveUserAuthentData(_userData, _viewReadCard);

                MessageBox.Show(this, "������������ " + this.userLoginTextBox.Text + " ������� ���������������",
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
                _auth.EditPassword(this.userLoginTextBox.Text, this.passwordTextBox.Text);

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
                _auth.EditPasswordAction(this.userLoginTextBox.Text, _dt);

                MessageBox.Show(this, "���� �������� ������ ������������ " + this.userLoginTextBox.Text + " ������� �������",
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
                        _auth.DeleteUser(_selectedItems[0].Name);

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
            this.userLoginTextBox.Text = String.Empty;
            this.passwordTextBox.Text = String.Empty;
            this.rfidcodTextBox.Text = String.Empty;

            DateTime _userPasswordDefTimeEnd = DateTime.Now + new TimeSpan(_userPasswordDefLifeTime, 0, 0);
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

                    this.userUserNameTextBox.Text = _selectedItems[0].SubItems[1].Text;
                    this.userLoginTextBox.Text = _selectedItems[0].SubItems[0].Text;

                    DateTime _dt = Convert.ToDateTime(_selectedItems[0].SubItems[2].Text);

                    this.dateWhenPasswordEnds.Value = _dt;
                    this.timeWhenPasswordEnds.Value = _dt;

                    this.blockUserPwdBtn.Enabled = !Convert.ToBoolean(_selectedItems[0].SubItems[3].Text); // ����������/������ ���������� ������
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
                    _auth.EditPasswordStatus(_selectedItems[0].Name, true);

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
                    _auth.EditPasswordStatus(_selectedItems[0].Name, false);

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

        }

        private void editUserRfidCod_Click(object sender, EventArgs e)
        {
            try
            {
                // �������������� ������ � ������������� ������������
                _auth.EditRfid(this.userLoginTextBox.Text, this.rfidcodTextBox.Text, _viewReadCard);
                MessageBox.Show(this, "��� �������� ������� �������", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.clearFieldsBtn_Click(null, null);
                // ���������� ������ �������������
                this.RefreshUsersList();
            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch
        } // unblockUserPwdBtn_Click

        #endregion

        private void editUserNameBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // �������������� ������ � ������������� ������������
                _auth.EditUserName(this.userLoginTextBox.Text, this.userUserNameTextBox.Text);

                MessageBox.Show(this, "��� ������������ ������� ��������", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // ���������� ������ �������������
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // ������� ��������� �� ������
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch
        }
    } // class MainForm

} // namespace UserRegTool