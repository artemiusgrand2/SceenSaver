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
        /// Экземпляр класса, предназначенного для работы с аутентификационными данными пользователей
        /// </summary>
        private static Authentificator auth = null;

        /// <summary>
        /// Если = true, то при загрузке главной формы приложения, загрузка приложения отменяется
        /// </summary>
        private static bool exitApplicationOnFormLoad = false;

        /// <summary>
        /// Полный путь к файлу аутентификационных данных пользователей
        /// </summary>
        private static string userAuthentDataFilePath = String.Empty;

        /// <summary>
        /// Время (в часах), в течение которого действует вновь создаваемый аккаунт пользователя (а точнее, его пароль).
        /// По умолчанию пароль действует в течение часа
        /// </summary>
        private static int userPasswordDefLifeTime = 1;

        #endregion


        #region Public Methods
        // -------------------

        /// <summary>
        /// Конструктор основной формы приложения
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();
            this.InitializeUsersList();

            try
            {
                // Считываем из конфигурационного файла полный путь к файлу аутентификационных данных пользователей
                // ------------------------------------------------------------------------------------------------
                userAuthentDataFilePath = ConfigurationManager.AppSettings[@"userAuthentDataFilePath"];

                // Считываем из конфигурационного файла время действия пароля пользователя
                // -----------------------------------------------------------------------
                if ((Int32.TryParse(ConfigurationManager.AppSettings[@"userPasswordDefLifeTime"], out userPasswordDefLifeTime)) == false)
                    userPasswordDefLifeTime = 1; // by default
                // The lifetime cannot be less then 0!
                if (userPasswordDefLifeTime < 0)
                    userPasswordDefLifeTime = 0;             

                // Пытаемся создать экземпляр класса, предназначенного для работы с аутентификационными данными пользователей
                // ----------------------------------------------------------------------------------------------------------
                auth = new Authentificator(userAuthentDataFilePath, Authentificator.AuthertificatorActions.All);

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Устанавливаем флаг отмены загрузки приложения
                exitApplicationOnFormLoad = true;
                return;

            } // catch

            // Определяем значения по умолчанию для полей на форме
            // ---------------------------------------------------
            DateTime _userPasswordDefTimeEnd = DateTime.Now + new TimeSpan(userPasswordDefLifeTime, 0, 0);

            this.dateWhenPasswordEnds.Format = DateTimePickerFormat.Custom; // поле будет отображать только дату
            this.dateWhenPasswordEnds.Value = _userPasswordDefTimeEnd; // установка даты окончания срока действия пароля по умолчанию - в соответствии с настройками программы 

            this.timeWhenPasswordEnds.Format = DateTimePickerFormat.Time; // поле будет отображать только время
            this.timeWhenPasswordEnds.Value = _userPasswordDefTimeEnd; // установка времени окончания срока действия пароля по умолчанию - в соответствии с настройками программы 

        } // constructor


        /// <summary>
        /// Действия, выполняемые в момент загрузки главной формы приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Если флаг отмены загрузки приложения установлен, то отменяем загрузку приложения
            // --------------------------------------------------------------------------------
            if (exitApplicationOnFormLoad == true)
            {
                // Exiting application
                Application.Exit();
                return;

            } // if

            // Извлекаем и отображаем все имена пользователей (зарегистрированных)
            // -------------------------------------------------------------------
            try
            {
                this.RefreshUsersList();
            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
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
            // Извлекаем информацию обо всех зарегистрированных пользователях
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
                    // Выводим сообщение об ошибке
                    MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Устанавливаем флаг отмены загрузки приложения
                    exitApplicationOnFormLoad = true;

                } // catch

            } // if

            // Отображаем информацию обо всех зарегистрированных пользователях
            // ---------------------------------------------------------------
            this.registeredUsersListView.Items.Clear();

            if (_allRegisteredUsers != null)
            {
                foreach (UserAuthentData _userData in _allRegisteredUsers)
                    this.AddUsersListViewItem(_userData);

            } // if

        } // RefreshUsersList


        /// <summary>
        /// Добавление нового пользователя (регистрация пользователя)
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
                // Добавление нового пользователя
                auth.SaveUserAuthentData(_userData);

                MessageBox.Show(this, "Пользователь " + this.userNameTextBox.Text + " успешно зарегистрирован",
                    "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // Обновление списка пользователей
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // addUserBtn_Click


        /// <summary>
        /// Редактирование пароля существующего пользователя (некогда зарегистрированного)
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void editUserPwdBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Редактирование пароля у существующего пользователя
                auth.EditPassword(this.userNameTextBox.Text, this.passwordTextBox.Text);

                MessageBox.Show(this, "Пароль успешно изменен", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // Обновление списка пользователей
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // editUserPwdBtn_Click


        /// <summary>
        /// Редактирование срока действия пароля существующего пользователя (некогда зарегистрированного)
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
                // Редактирование срока действия пароля у существующего пользователя
                auth.EditPasswordAction(this.userNameTextBox.Text, _dt);

                MessageBox.Show(this, "Срок действия пароля пользователя " + this.userNameTextBox.Text + " успешно изменен",
                    "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.clearFieldsBtn_Click(null, null);

                // Обновление списка пользователей
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // editPwdEndDateBtn_Click


        /// <summary>
        /// Удаление существующего пользователя (некогда зарегистрированного)
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
                    if (MessageBox.Show(this, "Удалить учетную запись пользователя " + _selectedItems[0].Name + "?",
                        "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // Удаление существующего пользователя
                        auth.DeleteUser(_selectedItems[0].Name);

                        this.clearFieldsBtn_Click(null, null);

                        // Обновление списка пользователей
                        this.RefreshUsersList();

                    } // if

                } // if

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // delUserBtn_Click


        /// <summary>
        /// Очистка полей ввода аутентификационных данных пользователей
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
        /// Отображение в соответствующих полях информации о выделенном в таблице пользователе
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

                    this.blockUserPwdBtn.Enabled = !Convert.ToBoolean(_selectedItems[0].SubItems[2].Text); // Разрешение/запрет блокировки пароля
                    this.unblockUserPwdBtn.Enabled = !this.blockUserPwdBtn.Enabled; // Разрешение/запрет разблокировки пароля

                } // if
                else
                {
                    this.blockUserPwdBtn.Enabled = true;
                    this.unblockUserPwdBtn.Enabled = true;

                } // else

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // registeredUsersListView_SelectedIndexChanged


        /// <summary>
        /// Блокирует пароль выбранного пользователя
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

                    MessageBox.Show(this, "Пароль пользователя " + _selectedItems[0].Name + " был заблокирован",
                        "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.clearFieldsBtn_Click(null, null);

                    // Обновление списка пользователей
                    this.RefreshUsersList();

                } // if

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // blockUserPwdBtn_Click


        /// <summary>
        /// Снимает блокировку пароля выбранного пользователя
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

                    MessageBox.Show(this, "Пароль пользователя " + _selectedItems[0].Name + " был разблокирован",
                        "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.clearFieldsBtn_Click(null, null);

                    // Обновление списка пользователей
                    this.RefreshUsersList();

                } // if

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show(this, _ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            } // catch

        } // unblockUserPwdBtn_Click

        #endregion

    } // class MainForm

} // namespace UserRegTool