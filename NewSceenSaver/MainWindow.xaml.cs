using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Configuration;
using Authentificator;
using FilesProcessingLib;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;
using NewScreenSaver.RFIDModul;
using NewScreenSaver.Enums;
using System.ComponentModel;
using NewScreenSaver.OtherScreens;
using NewScreenSaver.Models;

namespace NewScreenSaver
{


    public partial class MainWindow : Window
    {
        #region Переменные и свойства

        /// <summary>
        /// Контроль наличия карточки на считывателе
        /// </summary>
        bool cardOn = true;
        private int number_language = 0;
        /// <summary>
        /// произошла ли авторизация RFID
        /// </summary>
        public static bool isAuth { get; set; }
        /// <summary>
        /// сканер RFID
        /// </summary>
        RFIDScanBase RFIDScan = null;
        /// <summary>
        /// связь с другим экраном
        /// </summary>
        OtherScreen OtherScreen = null;
        /// <summary>
        /// слушатель подключений
        /// </summary>
        ListenScreen ListenScreen = null;
        /// <summary>
        /// иконка
        /// </summary>
        TaskbarIcon taskbarIcon = null;
        // ------------------------------
        /// <summary>
        /// Если =true, то надо выполнять редактирование данных пользователя, т е изменение пароля в связи с истечением времени действия
        /// 
        /// </summary>
        private static bool toEdit = false;
        /// <summary>
        /// Флаг показывет выведино ли форма сообщения MessageBox
        /// </summary>
        private static bool MessageShowActivete = false;
        /// <summary>
        /// Координата курсора по оси x
        /// </summary>
        private static int x_cursor;
        /// <summary>
        /// Координата курсора по оси y
        /// </summary>
        private static int y_cursor;
        /// <summary>
        /// Экземпляр класса, предназначенного для работы с аутентификационными данными пользователей
        /// </summary>
        private static Authentificators  auth = null;
        /// <summary>
        /// Допустимая область нахождения курсора мыши
        /// </summary>
        private static System.Drawing.Rectangle _rectangle_mouse_activete;
        /// <summary>
        /// Full path to the application LOG file
        /// </summary>
        private static string logFilePath = null;
        /// <summary>
        /// Maximum size (in bytes) of the application LOG file
        /// </summary>
        private static int maxLogFileSizeInBytes = 1000;

        /// <summary>
        /// Максимальное количество попыток для пользователя пройти процедуру аутентификации
        /// </summary>
        private static int maxAuthAttemp = int.MaxValue;

        /// <summary>
        /// Время (в часах), в течение которого действует вновь создаваемый аккаунт пользователя (а точнее, его пароль).
        /// По умолчанию пароль действует в течение часа
        /// </summary>
        private static int userPasswordDefLifeTime = 1;

        /// <summary>
        /// Экземпляр класса, предназначенного для перехвата глобального пользовательского ввода (клавиатура, мышь)
        /// </summary>
       private static UserActivityHook actHook = null;

        /// <summary>
        /// Печатные символы, которые пользователю разрешено вводить
        /// </summary>
        private static int[] allowedSymbolsArray = new int[] {
                (char)Keys.A, (char)Keys.B, (char)Keys.C, (char)Keys.D, (char)Keys.E, (char)Keys.F, (char)Keys.G, (char)Keys.H,
                (char)Keys.I, (char)Keys.J, (char)Keys.K, (char)Keys.L, (char)Keys.M, (char)Keys.N, (char)Keys.O, (char)Keys.P,
                (char)Keys.Q, (char)Keys.R, (char)Keys.S, (char)Keys.T, (char)Keys.U, (char)Keys.V, (char)Keys.W, (char)Keys.X,
                (char)Keys.Y, (char)Keys.Z, (char)Keys.OemMinus, (char)Keys.D0, (char)Keys.D1, (char)Keys.D2, (char)Keys.D3,
                (char)Keys.D4, (char)Keys.D5, (char)Keys.D6, (char)Keys.D7, (char)Keys.D8, (char)Keys.D9,(char)Keys.NumPad0, (char)Keys.NumPad1,
                (char)Keys.NumPad2, (char)Keys.NumPad3, (char)Keys.NumPad4, (char)Keys.NumPad5, (char)Keys.NumPad6, (char)Keys.NumPad7,
                (char)Keys.NumPad8, (char)Keys.NumPad9, (char)Keys.CapsLock, (char)Keys.Back, (char)Keys.Enter, (char)Keys.Left, (char)Keys.Right,
                (char)Keys.Up, (char)Keys.Down,  (int)Keys.LShiftKey, (int)Keys.RShiftKey};
        //private static char[] allowedCharsArray = new char[] {
        //        'A','a', 'B','b', 'C','c', 'D','d', 'E','e', 'F','f', 'G','g', 'H','h', 'I','i', 'J','j', 'K','k',
        //        'L','l', 'M','m', 'N','n', 'O','o', 'P','p', 'Q','q', 'R','r', 'S','s', 'T','t', 'U','u', 'V','v',
        //        'W','w', 'X','x', 'Y','y', 'Z','z', '_', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

        /// <summary>
        /// Если = true, то при загрузке главной формы приложения, загрузка приложения отменяется
        /// </summary>
        private static bool exitApplicationOnFormLoad = false;

        /// <summary>
        /// Объект таймера (периодически сигнализирует о том, что пользователю пора аутентифицироваться)
        /// </summary>
        private static System.Timers.Timer authTimer = new System.Timers.Timer();
        /// <summary>
        /// Объект таймера (не дает открыться диспетчеру задач)
        /// </summary>
        private static System.Timers.Timer authTimer_Closetaskmgr = new System.Timers.Timer();
        /// <summary>
        /// Интервал срабатывания таймера (периодичность аутентификации пользователя)
        /// </summary>
        private static int authInterval = 0;

        public delegate void HookEventHandler();

        /// <summary>
        /// Логин, под которым пользователь в настоящее время пытается аутентифицироваться
        /// </summary>
        private static string authLogin = null;
        /// <summary>
        /// Неудачное число попыток аутентифицироваться, сделанных одна за другой - т.е. подряд (для текущего логина)
        /// </summary>
        private static int authAttemp = 0;
        private DateTime lastAltClickTime = DateTime.Now;

        public VisibleInterfaceModel VisibleInterfaceModel { get;  } = new VisibleInterfaceModel();

        #endregion

        public MainWindow()
        {
            // Инициализация компонентов на главной форме
            // ------------------------------------------
            InitializeComponent();
            MaxStateWindows();
            this.DataContext = VisibleInterfaceModel;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            logFilePath = ConfigurationManager.AppSettings[@"logFilePath"];
            if ((Int32.TryParse(ConfigurationManager.AppSettings[@"maxLogFileSizeInBytes"], out maxLogFileSizeInBytes)) == false)
                maxLogFileSizeInBytes = 100000; // by default
            // The size of the file cannot be less then 0!
            if (maxLogFileSizeInBytes < 0)
                maxLogFileSizeInBytes = 0;
            //
            SaveMessInFile(DateTime.Now.ToString() + ": Загрузка приложения...", "MainWindow", "160");
            SaveMessInFile(DateTime.Now.ToString() + ": Меняем раскладку клавиатуры на английскую", "MainWindow", "161");
            //устанавливаем английскую раскладку клавиатуры
            for (int i = 0; i < InputLanguage.InstalledInputLanguages.Count; i++)
            {
                if (InputLanguage.InstalledInputLanguages[i].Culture.EnglishName.ToLower().IndexOf("eng") != -1)
                {
                    System.Windows.Forms.InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[i];
                    number_language = i;
                    button_language.Content = InputLanguage.InstalledInputLanguages[number_language].Culture.ThreeLetterISOLanguageName.ToUpper();
                    break;
                }
            }
            //System.Windows.Forms.InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US"));
            // Заносим в LOG-файл информацию о загрузке приложения
            // ---------------------------------------------------
            SaveMessInFile(DateTime.Now.ToString() + ": Загружаем NotifyIcon", "MainWindow", "166");
            taskbarIcon = (TaskbarIcon)System.Windows.Application.Current.FindResource("NotifyIcon");
          //  RecRamka.Fill = new SolidColorBrush(Color.FromArgb(255, 225, 225, 225));
            Background = new SolidColorBrush(Color.FromArgb(255 / 20, 255, 255, 255));
            //
            // Пытаемся создать экземпляр класса, предназначенного для работы с аутентификационными данными пользователей
            // ----------------------------------------------------------------------------------------------------------
            try
            {
                SaveMessInFile(DateTime.Now.ToString() + ": Загружаем данные Authentificators", "MainWindow", "175");
                auth = new Authentificators(ConfigurationManager.AppSettings[@"userAuthentDataFilePath"],
                    Authentificators.AuthertificatorActions.OnlyAuthentication);
                // Считываем из конфигурационного файла время действия пароля пользователя
                // -----------------------------------------------------------------------
                if ((Int32.TryParse(ConfigurationManager.AppSettings[@"userPasswordDefLifeTime"], out userPasswordDefLifeTime)) == false)
                    userPasswordDefLifeTime = 1; // by default
                // The lifetime cannot be less then 0!
                if (userPasswordDefLifeTime < 0)
                    userPasswordDefLifeTime = 0;
                //
                string hostname = "127.0.0.1";
                int port = 2002;
                int listenPort = 2002;
                if (ConfigurationManager.AppSettings.AllKeys.Contains("otherscreen"))
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("listenPort"))
                        int.TryParse(ConfigurationManager.AppSettings["listenPort"], out listenPort);
                    GetOtherScreen(ConfigurationManager.AppSettings["otherscreen"], ref hostname, out port);
                    //слушатель подключений
                    ListenScreen = new OtherScreens.ListenScreen(listenPort);
                    ListenScreen.IsAuthorization += UpdateStatusAuthorization;
                    ListenScreen.Start();
                    //запускаем работу с другим экраном
                    OtherScreen = new OtherScreens.OtherScreen(hostname, port);
                    OtherScreen.IsAuthorization += UpdateStatusAuthorization;
                    OtherScreen.Start();
                }
                //
                var baudRate = 9600;
                if (ConfigurationManager.AppSettings.AllKeys.Contains("baudRate"))
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Contains("baudRate"))
                    {
                        if (int.TryParse(ConfigurationManager.AppSettings["baudRate"], out baudRate))
                            baudRate = Math.Abs(baudRate);
                        else
                            baudRate = 9600;
                    }
                }
                //инициализируем работу с rfid картой
                if (ConfigurationManager.AppSettings.AllKeys.Contains("cardOn"))
                    cardOn = (ConfigurationManager.AppSettings["cardOn"].ToString() == "true") ? true : false;
                //
                var viewReader = ParserCommon.GetViewCard((ConfigurationManager.AppSettings.AllKeys.Contains("viewReader")) ? ConfigurationManager.AppSettings["viewReader"] : string.Empty);
                if (viewReader == Authentificator.Enums.ViewReader.crem)
                    cardOn = true;
                RFIDScan = RFIDScanFactory.Create(viewReader, baudRate, auth, cardOn);
                RFIDScan.Authorization += UpdateStatusAuthorization;
                VisibleInterfaceModel.Initialization(RFIDScan, Dispatcher);
                RFIDScan.Start();
            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                System.Windows.MessageBox.Show(this, _ex.Message, "Error" + " MainWindow251", MessageBoxButton.OK, MessageBoxImage.Error);

                // Сохраняем сообщение об ошибке в LOG-файле
                SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message, "MainWindow", "217");

                // Устанавливаем флаг отмены загрузки приложения
                exitApplicationOnFormLoad = true;
                return;

            } // catch

            // Getting parameters for the application LOG file
            // -----------------------------------------------
            // a) full file path
          
            // b) maximum size (in bytes) of the LOG file
        

            // Getting maximum number of attempts for the user to authenticate
            // ---------------------------------------------------------------
            if ((Int32.TryParse(ConfigurationManager.AppSettings[@"maxAuthAttemp"], out maxAuthAttemp)) == false)
                maxAuthAttemp = int.MaxValue; // by default
            // The number of attempts cannot be less then 0!
            if (maxAuthAttemp < 0)
                maxAuthAttemp = 1;

            // Проверяем, существует ли заданный каталог, где будет располагаться (или уже располагается) LOG-файл приложения
            // --------------------------------------------------------------------------------------------------------------
            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(logFilePath)) == false)
                    throw new Exception("Указанный каталог для LOG-файла приложения не существует");

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                System.Windows.MessageBox.Show(this, _ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Сохраняем сообщение об ошибке в LOG-файле
                SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message,  "MainWindow", "254");

                // Устанавливаем флаг отмены загрузки приложения
                exitApplicationOnFormLoad = true;
                return;

            } // catch          

            // Getting interval between invocations of the callback method of the ' Authenticate User ' timer and
            // initializing our timer
            // --------------------------------------------------------------------------------------------------
            if ((Int32.TryParse(ConfigurationManager.AppSettings[@"authInterval"], out authInterval)) == false)
                authInterval = 3600000; // 1 hour by default
            if (authInterval <= 1000)
                authInterval = 3600000; // 1 hour by default

            authTimer.Elapsed += ProcessAuthenticateTimerProc;
            authTimer.Interval = authInterval;
            //
            authTimer_Closetaskmgr.Elapsed += ProcessAuthenticateTimerProc_taskmgr;
            authTimer_Closetaskmgr.Interval = 1;
            authTimer_Closetaskmgr.AutoReset = true;
            //
            SaveMessInFile(DateTime.Now.ToString() + ": Загрузка приложения завершена...",  "MainWindow", "277");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            System.Windows.MessageBox.Show(args.ExceptionObject.ToString(), "Error319", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void GetOtherScreen(string data, ref string hostname, out int port)
        {
            string[] parser = data.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parser.Length == 2)
            {
                int.TryParse(parser[1], out port);
                hostname = parser[0];
                return;
            }
            //

            port = 2002;
        }



        private void UpdateStatusAuthorization(bool status)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    if(isAuth != status)
                    {
                        isAuth = status;

                        if (status)
                        {
                            if (taskbarIcon.Visibility == System.Windows.Visibility.Hidden)
                            {
                                HideWindow(true);
                                if (cardOn)
                                    authTimer.Stop();
                            }
                        }
                        else
                        {
                            if (taskbarIcon.Visibility == System.Windows.Visibility.Visible)
                            {
                                authTimer.Start();
                                ProcessAuthenticateTimerProc(null, null);
                            }
                        }
                    }
                }
                catch { }
            }
           ));
        }

        private void ShowCommnadClick(object sender, RoutedEventArgs args)
        {
            ProcessAuthenticateTimerProc(null, null);
        }

        private void RefreshUsersList()
        {
            if (auth != null)
            {
                // Извлекаем информацию обо всех зарегистрированных пользователях
                // --------------------------------------------------------------
                List<UserAuthentData> _allUsers = auth.GetAllUsersInfo();

                // Отображаем имена всех пользователей (зарегистрированных)
                // -------------------------------------------------------
                this.loginCombo.Items.Clear();

                if (_allUsers != null)
                {
                    foreach (UserAuthentData _user in _allUsers)
                        this.loginCombo.Items.Add(_user.Login);

                }
            }
        }

        /// <summary>
        /// Реакция на подтверждение пользователем введенных им аутентификационных данных
        /// </summary>
        /// 
        /// <param name="sender"></param>
        /// 
        /// <param name="e"></param>
        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем введенные пользователем аутентификационные данные
            // -----------------------------------------------------------
            // Чувствительность к регистру (как имя, так и пароль)!!!!!!!!!!!!!!!!!!!!!!!!!

            Authentificators.UserAuthentResult _authRes = Authentificators.UserAuthentResult.AuthentDataNotFound;
            var isStartAuthTimer = true;
            if (toEdit)
            {
                //Редактирование данных пользователя 
                // Прежде чем делать проверки на корректность аутентификационных данных,
                // фиксируем (в LOG-файле) попытку пользователя обновить пароль
                // ---------------------------------------------------------------------
                SaveMessInFile(DateTime.Now.ToString() + ": Обновление устаревшего пароля пользователя: " + authLogin, "OKBtn_Click", "361");

                try
                {
                    // Проверим пароль для данного аккаунта 
                    _authRes = auth.Authenticate(authLogin, passwordBox.Password);
                    if (_authRes == Authentificators.UserAuthentResult.WrongPassword)
                    {
                        authAttemp++;

                        if (authAttemp >= maxAuthAttemp)
                        {
                            // Блокируем аккаунт текущего пользователя
                            auth.EditPasswordStatus(authLogin, true);
                            MessageShowActivete = true;
                            // Выводим сообщение об ошибке
                            System.Windows.Forms.MessageBox.Show("Пароль неверен. Истекло максимальное число попыток аутентифицироваться. Аккаунт " + this.loginCombo.Text + " заблокирован");
                            MessageShowActivete = false;
                            // Сохраняем сообщение об ошибке в LOG-файле
                            SaveMessInFile(DateTime.Now.ToString() + ": Пароль неверен. Истекло максимальное число попыток аутентифицироваться. Аккаунт " + this.loginCombo.Text + " заблокирован", "OKBtn_Click", "380");

                        } // if
                        else
                        {
                            MessageShowActivete = true;
                            System.Windows.Forms.MessageBox.Show("Неверно указан текущий пароль. ");
                            MessageShowActivete = false;
                        }
                    }
                    else
                    {// пароль правильный можно редактировать
                        if (this.EditPassword(authLogin, this.passwordBox.Password, newPasswordBox.Password, newPasswordBoxEnter.Password))
                        {
                            this.RefreshUsersList();
                            authAttemp = 0;
                            //Если редактирование прошло успешно, то
                            //Восстановить вид окна и сбросить флаг редактирования
                            SaveMessInFile(DateTime.Now.ToString() + ": Обновление пароля  выполнено успешно ", "OKBtn_Click", "398");
                            SettingsNewPassword(System.Windows.Visibility.Hidden);
                            Title = "Аутентификация пользователя";
                            

                            // После изменения пароля Аутентификацию будем считать успешно пройденно, т.к. это уже было сделано перед редактированием
                            _authRes = Authentificators.UserAuthentResult.OK;
                        }//if
                    }//else
                }
                catch (Exception _ex)
                {
                   // authAttemp = 0;
                    MessageShowActivete = true;
                    // Выводим сообщение об ошибке
                    System.Windows.Forms.MessageBox.Show(_ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageShowActivete = false;
                    // Сохраняем сообщение об ошибке в LOG-файле
                    SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message, "OKBtn_Click", "416");
                }
            }//if
            else
            {
                // Прежде чем делать проверки на корректность аутентификационных данных,
                // фиксируем (в LOG-файле) попытку пользователя аутентифицироваться
                // ---------------------------------------------------------------------
                SaveMessInFile(DateTime.Now.ToString() + ": Аутентификация пользователя: " + this.loginCombo.Text, "OKBtn_Click", "424");

                // Если пользователь ввел имя и пароль, которые должны привести к завершению работы приложения, то
                // завершаем работу приложения
                if ((this.loginCombo.Text.ToUpper() == "TSS") && (this.passwordBox.Password.ToUpper() == "CLOSE"))
                {
                    try { actHook.Stop(); }
                    catch { }
                    try { authTimer_Closetaskmgr.Stop(); }
                    catch { }
                    Close();
                    return;
                } //

                // Если пользователь ввел "резервные" имя и пароль, то их не проверяем
                if ((this.loginCombo.Text.ToUpper() == "TSS") && (this.passwordBox.Password == "1"))
                {
                    _authRes = Authentificators.UserAuthentResult.OK;
                    isStartAuthTimer = false;
                    
                }
                // Если же введенные имя и пароль отличны от "резервных", то они подлежат проверке
                else
                {
                    if (authLogin != this.loginCombo.Text)
                    {
                        // Запоминаем имя пользователя при его первой попытке аутентифицироваться
                        authLogin = this.loginCombo.Text;
                        authAttemp = 0;

                    } // if

                    try
                    {
                        _authRes = auth.Authenticate(this.loginCombo.Text, this.passwordBox.Password);

                        if (_authRes == Authentificators.UserAuthentResult.AuthentDataNotFound)
                        {
                            MessageShowActivete = true;
                            // Выводим сообщение об ошибке
                            System.Windows.Forms.MessageBox.Show("Указанный аккаунт не существует");
                            MessageShowActivete = false;
                            // Сохраняем сообщение об ошибке в LOG-файле
                            SaveMessInFile(DateTime.Now.ToString() + ": Указанный аккаунт не существует", "OKBtn_Click", "464");

                        } // if

                        else if (_authRes == Authentificators.UserAuthentResult.WrongPassword)
                        {
                            authAttemp++;

                            if (authAttemp >= maxAuthAttemp)
                            {
                                // Блокируем аккаунт текущего пользователя
                                auth.EditPasswordStatus(this.loginCombo.Text, true);
                                MessageShowActivete = true;
                                // Выводим сообщение об ошибке
                                System.Windows.Forms.MessageBox.Show("Пароль неверен. Истекло максимальное число попыток аутентифицироваться. Аккаунт " + this.loginCombo.Text + " заблокирован");
                                MessageShowActivete = false;
                                // Сохраняем сообщение об ошибке в LOG-файле
                                SaveMessInFile(DateTime.Now.ToString() + ": Пароль неверен. Истекло максимальное число попыток аутентифицироваться. Аккаунт " + this.loginCombo.Text + " заблокирован", "OKBtn_Click", "481");

                            } // if
                            else
                            {
                                MessageShowActivete = true;
                                // Выводим сообщение об ошибке
                                System.Windows.Forms.MessageBox.Show("Неверно указан пароль");
                                MessageShowActivete = false;
                                // Сохраняем сообщение об ошибке в LOG-файле
                                SaveMessInFile(DateTime.Now.ToString() + ": Неверно указан пароль", "OKBtn_Click", "491");

                            } // else

                        } // else if

                        else if (_authRes == Authentificators.UserAuthentResult.PasswordExpired)
                        {
                            MessageShowActivete = true;
                            // Выводим сообщение об ошибке
                            System.Windows.Forms.MessageBox.Show("Срок действия пароля истек");
                            MessageShowActivete = false;
                            // Сохраняем сообщение об ошибке в LOG-файле
                            SaveMessInFile(DateTime.Now.ToString() + ": Срок действия пароля истек. Измените пароль.", "OKBtn_Click", "504");
                            SettingsNewPassword(System.Windows.Visibility.Visible);
                            authAttemp = 0;
                            Title = "Изменить пароль";

                        } // else if

                        else if (_authRes == Authentificators.UserAuthentResult.PasswordBlocked)
                        {
                            MessageShowActivete = true;
                            // Выводим сообщение об ошибке
                            System.Windows.Forms.MessageBox.Show("Аккаунт заблокирован");
                            MessageShowActivete = false;
                            // Сохраняем сообщение об ошибке в LOG-файле
                            SaveMessInFile(DateTime.Now.ToString() + ": Аккаунт заблокирован", "OKBtn_Click", "518");

                        } // else if

                    } // try
                    catch (Exception _ex)
                    {
                        MessageShowActivete = true;
                        // Выводим сообщение об ошибке
                        System.Windows.Forms.MessageBox.Show(_ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageShowActivete = false;
                        // Сохраняем сообщение об ошибке в LOG-файле
                        SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message, "OKBtn_Click", "530");

                    } // catch

                } // else

            }//if(toEdit)

            // После того как процедура аутентификации прошла успешно, прячем окно и запускаем таймер
            if (_authRes == Authentificators.UserAuthentResult.OK)
            {
                HideWindow(isStartAuthTimer);
            }
           
        }

        private void HideWindow(bool isStartAuthTimer)
        {
            loginCombo.Text = String.Empty;
            passwordBox.Password = String.Empty;
            loginCombo.Focus();
            // Сохраняем сообщение об успешной аутентификации в LOG-файле
            SaveMessInFile(DateTime.Now.ToString() + ": Аутентификация прошла успешно","HideWindow", "551");
            isAuth = true;
            if (OtherScreen != null)
                OtherScreen.SendCommand(isAuth);
            // Если перехват ввода-вывода клавиатуры и мыши остановить не удается, то приложение немедленно завершаем
            try
            {
                 actHook.Stop();
                authTimer_Closetaskmgr.Stop();
            } // try
            catch (Exception _ex)
            {
                //MessageShowActivete = true;
                //// Выводим сообщение об ошибке
                //System.Windows.Forms.MessageBox.Show(_ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //MessageShowActivete = false;
                //// Сохраняем сообщение об ошибке в LOG-файле
                //SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message);

                //Close();
                return;

            } // catch

            // Переводим главное окно приложения в режим ожидания
            SetMainWindowState(false);
            // Запускаем таймер
            if (isStartAuthTimer)
                authTimer.Start();
        }

        /// <summary>
        ///  Функция сохранения нового пароля пользователя
        /// </summary>
        /// <param name="login"></param>
        /// <param name="old_password"></param>
        /// <param name="new_password"></param>
        /// <returns></returns>
        private bool EditPassword(string inp_login, string old_password, string new_password, string new_password_enter)
        {
            bool retFl = false;

            if ((new_password == string.Empty) || (old_password == string.Empty) || (new_password_enter == string.Empty))
            {
                MessageShowActivete = true;
                System.Windows.Forms.MessageBox.Show("Поле с паролем не заполнено", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageShowActivete = false;
                return false;
            }
            else
            {
                if (old_password == new_password)
                {
                    MessageShowActivete = true;
                    System.Windows.Forms.MessageBox.Show("Новый пароль должен отличаться от старого", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageShowActivete = false;
                    return false;
                }
                else if (new_password != new_password_enter)
                {
                    MessageShowActivete = true;
                    System.Windows.Forms.MessageBox.Show("Новый пароль не подтвержден", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageShowActivete = false;
                    return false;
                }
            }

            try
            {
                // Редактирование пароля у существующего пользователя
                auth.EditPassword(inp_login, new_password);


                DateTime _dt = DateTime.Now + new TimeSpan(userPasswordDefLifeTime, 0, 0);

                try
                {
                    // Редактирование срока действия пароля у существующего пользователя
                    auth.EditPasswordAction(inp_login, _dt);

                } // try
                catch (Exception _ex)
                {
                    // сообщение об ошибке при  изменении срока действия

                    throw new Exception(_ex.Message);

                } // catch
                MessageShowActivete = true;
                System.Windows.Forms.MessageBox.Show("Пароль успешно изменен", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageShowActivete = false;
                retFl = true;

            } // try
            catch (Exception _ex)
            {
                // Выводим сообщение об ошибке
                MessageShowActivete = true;
                System.Windows.Forms.MessageBox.Show(_ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageShowActivete = false;
                retFl = false;
            } // catch

            return retFl;
        } // EditPassword

        /// <summary>
        /// Таймерная процедура (выполняется в отдельном потоке)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void ProcessAuthenticateTimerProc(object source, System.Timers.ElapsedEventArgs e)
        {
            // Останавливаем таймер (он будет запущен, когда пользователь успешно аутентифицируется)
            authTimer.Stop();
            isAuth = false;
            if (OtherScreen != null)
                OtherScreen.SendCommand(isAuth);
            // Передаем управление основному потоку (т.к. если делать перехват событий клавиатуры и мыши здесь, то ничего не получится)
            Dispatcher.Invoke(new HookEventHandler(SetHook));

        }

        private void MaxStateWindows()
        {
            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;
            this.Left = 0;
            this.Top = 0;
            this.DrawCanvas.Margin = new Thickness((System.Windows.SystemParameters.PrimaryScreenWidth - this.DrawCanvas.Width) / 2,
                                                   (System.Windows.SystemParameters.PrimaryScreenHeight - this.DrawCanvas.Height) / 2, 0, 0);
        }

        private void SetMainWindowState(bool inp_authState)
        {
            if (inp_authState == true)
            {
                Show(); //System.Windows.Application.Current.Shutdown();
                MaxStateWindows();
                //WindowState =  System.Windows.WindowState.Maximized;
                taskbarIcon.Visibility = System.Windows.Visibility.Hidden;
               // showToolStripMenuItem.Enabled = false;
            } // if

            else
            {
                taskbarIcon.Visibility = System.Windows.Visibility.Visible;
               // showToolStripMenuItem.Enabled = true;
                Hide();
              //  WindowState = System.Windows.WindowState.Minimized;

            } // else

        }

        /// <summary>
        /// Устанавливаем Hook (для глобального перехвата событий клавиатуры и мыши)
        /// </summary>
        private void SetHook()
        {
            loginCombo.Text = string.Empty;
            passwordBox.Password = string.Empty;
            authAttemp = 0;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US"));
            //Запускаем таймер, который не дает запустить диспетчер задач
            authTimer_Closetaskmgr.Start();
            // Переводим главное окно приложения в режим работы с пользователем
            this.SetMainWindowState(true);

            // Извлекаем и отображаем все имена пользователей (зарегистрированных)
            // -------------------------------------------------------------------
            try
            {
                this.RefreshUsersList();

            } // try
            catch (Exception _ex)
            {
                MessageShowActivete = true;
                // Выводим сообщение об ошибке
                System.Windows.MessageBox.Show(this, _ex.Message + " SetHook793", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageShowActivete = false;
                // Сохраняем сообщение об ошибке в LOG-файле
                SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message, "SetHook", "721");

            } // catch


            // Пытаемся начать перехват событий клавиатуры и мыши
            // --------------------------------------------------
            try
            {
                if (actHook == null)
                {
                    actHook = new UserActivityHook(new System.Drawing.Rectangle(0, 0, (int)System.Windows.SystemParameters.VirtualScreenWidth, (int)System.Windows.SystemParameters.VirtualScreenHeight));

                    actHook.OnMouseActivity += new System.Windows.Forms.MouseEventHandler(MouseMoved);
                    actHook.KeyDown += new System.Windows.Forms.KeyEventHandler(MyKeyDown);
                    actHook.KeyPress += new System.Windows.Forms.KeyPressEventHandler(MyKeyPress);
                    actHook.KeyUp += new System.Windows.Forms.KeyEventHandler(MyKeyUp);
                } // if

                else
                    actHook.Start();

            } // try
            catch (Exception _ex)
            {
                MessageShowActivete = true;
                // Выводим сообщение об ошибке
                System.Windows.MessageBox.Show(this, _ex.Message + " SetHook823", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageShowActivete = false;
                // Сохраняем сообщение об ошибке в LOG-файле
                SaveMessInFile(DateTime.Now.ToString() + ": " + _ex.Message, "SetHook", "751");

                try { actHook.Stop(); }
                catch { }

                try { authTimer_Closetaskmgr.Stop(); }
                catch { }

                // Переводим главное окно приложения в режим ожидания
                this.SetMainWindowState(false);

                // Запускаем таймер
                authTimer.Start();

            } // catch

        }

        private void SettingsNewPassword(System.Windows.Visibility visible)
        {
            if (visible == System.Windows.Visibility.Visible)
            {
                toEdit = true;
                loginCombo.IsEnabled = false;
                passwordBox.IsEnabled = false;
            }
            else
            {
                toEdit = false;
                loginCombo.IsEnabled = true;
                passwordBox.Password = String.Empty;
                passwordBox.IsEnabled = true;
            }
            newPasswordBox.Password = String.Empty;
            newPasswordBoxEnter.Password = String.Empty;

            newPasslab.Visibility = visible;
            newPasswordBox.Visibility = visible;
            newPasslabEnter.Visibility = visible;
            newPasswordBoxEnter.Visibility = visible;
            ButtonBack.Visibility = visible;
        }


        private void MyKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if ((Array.IndexOf(allowedSymbolsArray, (char)e.KeyData) < 0))
            {
                e.Handled = true;
            }
            // Отпущена допустимая клавиша
            // ---------------------------
            //else
            //{
            //    if (e.Alt)
            //    {
            //        if ((char)e.KeyData == (char)Keys.Tab)
            //        {
            //            e.Handled = true;
            //        }

            //        if (((char)e.KeyData == (char)Keys.LControlKey) || ((char)e.KeyData == (char)Keys.RControlKey))
            //        {
            //            e.Handled = true;
            //        }

            //    }
            //    if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            //    {
            //        if (((char)e.KeyData == (char)Keys.LMenu) || ((char)e.KeyData == (char)Keys.RMenu))
            //        {
            //            e.Handled = true;
            //        }
            //    }
            //    if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Shift)
            //    {
            //        if (((char)e.KeyData == (char)Keys.LMenu) || ((char)e.KeyData == (char)Keys.RMenu))
            //        {
            //            e.Handled = true;
            //        }
            //    }
            //}
        }
        
        private void MyKeyPress(object sender, KeyPressEventArgs e)
        {
            //if ((e.KeyChar != (char)Keys.Tab) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Back))
            //{
            //    e.Handled = true;
            //}
            //// Нажата допустимая клавиша
            //// -------------------------
            //else
            //{
            //if ((System.Windows.Forms.Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            //{
            //    if (e.KeyChar == (char)Keys.Tab)
            //    {
            //        e.Handled = true;
            //    }

            //    else if ((e.KeyChar == (char)Keys.LControlKey) || (e.KeyChar == (char)Keys.RControlKey))
            //    {
            //        e.Handled = true;
            //    }

            //}
            //if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            //{
            //    if ((e.KeyChar == (char)Keys.LMenu) || (e.KeyChar == (char)Keys.RMenu))
            //    {
            //        e.Handled = true;
            //    }
            //}
            //if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Shift)
            //{

            //    if ((e.KeyChar == (char)Keys.LMenu) || (e.KeyChar == (char)Keys.RMenu))
            //    {
            //        e.Handled = true;
            //    }
            //}
            ////}
        }

        private void MouseMoved(object sender, System.Windows.Forms.MouseEventArgs e) { }

        private void MyKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
          //  e.KeyCode == Keys.Shift
            // Нажата недопустимая клавиша
            // ---------------------------    
            if ((Array.IndexOf(allowedSymbolsArray, (char)e.KeyData) < 0))
            {
                e.Handled = true;
            }

            //
            // Нажата допустимая клавиша
            // -------------------------    
            //else
            //{
            //    if (e.KeyValue == 164 || e.KeyValue == 165)
            //    {
            //        //loginCombo.Focus();
            //        //lastAltClickTime = DateTime.Now;
            //        e.Handled = true;
            //    }
            //if (e.Alt)
            //{
            //    //
            //    if ((char)e.KeyData == (char)Keys.Tab)
            //    {
            //        e.Handled = true;
            //    }

            //    else if (((char)e.KeyData == (char)Keys.LControlKey) || ((char)e.KeyData == (char)Keys.RControlKey))
            //    {
            //        e.Handled = true;
            //    }
            //}

            //    if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            //    {
            //        if (((char)e.KeyData == (char)Keys.LMenu) || ((char)e.KeyData == (char)Keys.RMenu))
            //        {
            //            e.Handled = true;
            //        }
            //    }

            //    if ((System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Shift)
            //    {
            //        if (((char)e.KeyData == (char)Keys.LMenu) || ((char)e.KeyData == (char)Keys.RMenu))
            //        {
            //            e.Handled = true;
            //        }
            //    }
            //}
        }


        /// <summary>
        /// Таймерная процедура (выполняется в отдельном потоке)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void ProcessAuthenticateTimerProc_taskmgr(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //
                Dispatcher.Invoke(new Action(() =>
                {
                    if (Console.CapsLock)
                        Warnning.Content = "Нажата клавиша Caps Lock";
                    else Warnning.Content = string.Empty;
                    //
                    if (!IsFocused && !loginCombo.IsFocused && !passwordBox.IsFocused && !newPasswordBox.IsFocused && !MessageShowActivete && !newPasswordBoxEnter.IsFocused
                     && !button_restart.IsFocused && !newPasslab.IsFocused && !loginLbl.IsFocused && !passwordLbl.IsFocused && !newPasslab.IsFocused && !this.IsFocused)
                    {
                        Activate();
                    }
                }
                ));
                //если открыт диспетчер задач закрываем его
                if (System.Diagnostics.Process.GetProcessesByName("taskmgr").Length > 0)
                    System.Diagnostics.Process.GetProcessesByName("taskmgr")[0].Kill();
            }
            catch { }

        } 

        public static bool SaveMessInFile(string inp_mess, string function, string number)
        {
            if (String.IsNullOrEmpty(inp_mess) == true)
                return false; /* cannot save a NULL or an empty message */

            // Clearing the LOG file of the application (if it is necessary) before writing new data in it
            FilesProcessor.ClearFile(logFilePath, maxLogFileSizeInBytes);

            // Creating a text writer object for appending a UTF-8 encoded message in the LOG file of the current application
            StreamWriter _logFileWriter = null;

            try
            {
                if (File.Exists(logFilePath) == false)
                    File.Create(logFilePath);

                // Writing new data in the LOG file of the application
               _logFileWriter = File.AppendText(logFilePath);
               _logFileWriter.WriteLine(string.Format("{0}; Функция - {1}, строка - {2}", inp_mess, function, number));
               _logFileWriter.Flush();
            } // try
            catch
            {
                return false;
            } // catch
            finally
            {
                if (_logFileWriter != null)
                {
                    try { _logFileWriter.Close(); } // try
                    catch { /* No message is needed */ } // catch
                    _logFileWriter = null;
                } 
            } 

            return true;

        }

        /// <summary>
        /// перезагрузка компа
        /// </summary>
        private void Shutdown()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@"cmd.exe", @"/C shutdown -r -t 0 -f");
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                System.Diagnostics.Process.Start(info);
            }
            catch { }
        }

        private void SetFillRectangle(byte factor)
        {
            GradientStopCollection collection = new GradientStopCollection();
            collection.Add(new GradientStop(Color.FromArgb((byte)(255 / factor), (RecRamka.Fill as LinearGradientBrush).GradientStops[0].Color.R,
                (RecRamka.Fill as LinearGradientBrush).GradientStops[0].Color.G, (RecRamka.Fill as LinearGradientBrush).GradientStops[0].Color.B), (RecRamka.Fill as LinearGradientBrush).GradientStops[0].Offset));
            collection.Add(new GradientStop(Color.FromArgb((byte)(255 / factor), (RecRamka.Fill as LinearGradientBrush).GradientStops[1].Color.R,
             (RecRamka.Fill as LinearGradientBrush).GradientStops[1].Color.G, (RecRamka.Fill as LinearGradientBrush).GradientStops[1].Color.B), (RecRamka.Fill as LinearGradientBrush).GradientStops[1].Offset));
            LinearGradientBrush brush  = new LinearGradientBrush(collection);
            brush.StartPoint = new Point((RecRamka.Fill as LinearGradientBrush).StartPoint.X, (RecRamka.Fill as LinearGradientBrush).StartPoint.Y);
            brush.EndPoint = new Point((RecRamka.Fill as LinearGradientBrush).EndPoint.X, (RecRamka.Fill as LinearGradientBrush).EndPoint.Y);
            RecRamka.Fill = brush; 
            //
            button_OK.Background = new SolidColorBrush(Color.FromArgb((byte)(255 / factor), (button_OK.Background as SolidColorBrush).Color.R, (button_OK.Background as SolidColorBrush).Color.G, (button_OK.Background as SolidColorBrush).Color.B));
            button_restart.Background = new SolidColorBrush(Color.FromArgb((byte)(255 / factor), (button_restart.Background as SolidColorBrush).Color.R, (button_restart.Background as SolidColorBrush).Color.G, (button_restart.Background as SolidColorBrush).Color.B));
        }

        private void button_opraticy_Click(object sender, RoutedEventArgs e)
        {
            if (button_opraticy.Content.ToString() == "Прозрачно")
            {
                SetFillRectangle(5);
                button_opraticy.Content = "Полупрозрачно";
            }
            else
            {
                SetFillRectangle(1);
                button_opraticy.Content = "Прозрачно";
            }
        }

        private void button_restart_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Перезагружать компьютер ?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                SaveMessInFile(DateTime.Now.ToString() + ": " + "Произошла перезагрузка компьютера !!!", "", "");
                Shutdown();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Если флаг отмены загрузки приложения установлен, то отменяем загрузку приложения
            // --------------------------------------------------------------------------------
            if (exitApplicationOnFormLoad == true)
            {
                Close();
                return;

            } 
            // Заносим в LOG-файл информацию о загрузке приложения
            // ---------------------------------------------------
            SaveMessInFile(DateTime.Now.ToString() + ": Приложение загружено", "Window_Loaded", "1083");
            //определяем допустимую область нахождения курсора мыши
            _rectangle_mouse_activete = new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
            //устанавливаем стартовые позиции курсора для запоминания
            x_cursor = 0;
            y_cursor = 0;
            // Начинаем ловить события мыши и клавиатуры и требовать от пользователя аутентифицироваться
            // -----------------------------------------------------------------------------------------
            this.SetHook();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Сохраняем сообщение об отмене загрузки приложения в LOG-файле
            SaveMessInFile(DateTime.Now.ToString() + ": Завершение работы приложения", "Window_Closing", "1097");

            //this.SetMainWindowState(true);

            // Освобождение ресурсов:

            // 1) Authenticator
            auth = null;
            if(RFIDScan != null)
                RFIDScan.Stop();

           // 2) Hook
            if (actHook != null)
            {
                try { actHook.Stop(); }
                catch { }

                actHook = null;

            } // if

            if (OtherScreen != null)
                OtherScreen.Stop();
            //
            if (ListenScreen != null)
                ListenScreen.Stop();
            // 3) Timer
            if (authTimer != null)
            {
                if (authTimer.Enabled == true)
                    try { authTimer.Stop(); }
                    catch { }

                authTimer = null;

            } // if

            if (authTimer_Closetaskmgr != null)
            {
                if (authTimer_Closetaskmgr.Enabled == true)
                    try { authTimer_Closetaskmgr.Stop(); }
                    catch { }

                authTimer_Closetaskmgr = null;

            } // if

            taskbarIcon.Dispose();
            authTimer_Closetaskmgr = null;
            //
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// ширина текста в пикселях
        /// </summary>
        /// <param name="combox"></param>
        /// <returns></returns>
        public static double WidthText(System.Windows.Controls.ComboBox combox)
        {
            Typeface typeface = new Typeface(combox.FontFamily, combox.FontStyle, combox.FontWeight, combox.FontStretch);
            System.Windows.Media.Brush brush = new SolidColorBrush();
            FormattedText formatedText = new FormattedText(combox.Text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeface, combox.FontSize, brush);
            return formatedText.Width;
        }

        private void loginCombo_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OKBtn_Click(null, null);
        }

        private void passwordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Console.CapsLock)
                Warnning.Content = "Нажата клавиша Caps Lock";
            else Warnning.Content = string.Empty;
            //
            if (e.Key == Key.Enter)
                OKBtn_Click(null, null);
        }

        private void loginCombo_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OKBtn_Click(null, null);
        }

        //private void loginCombo_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if ((DateTime.Now - lastAltClickTime).TotalMilliseconds <= 500 && !loginCombo.Focusable)
        //        loginCombo.Focus();
        //}

        //private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if ((DateTime.Now - lastAltClickTime).TotalMilliseconds <= 500 && !passwordBox.Focusable)
        //        passwordBox.Focus();
        //}

        //private void newPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if ((DateTime.Now - lastAltClickTime).TotalMilliseconds <= 500 && !newPasswordBox.Focusable)
        //        newPasswordBox.Focus();
        //}

        //private void newPasswordBoxEnter_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if ((DateTime.Now - lastAltClickTime).TotalMilliseconds <= 500 && !newPasswordBoxEnter.Focusable)
        //        newPasswordBoxEnter.Focus();
        //}

        private void newPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (newPasswordBox.Password == newPasswordBoxEnter.Password && !string.IsNullOrEmpty(newPasswordBox.Password))
            {
                newPasswordBox.Background = Brushes.Green;
                newPasswordBoxEnter.Background = Brushes.Green;
            }
            else
            {
                newPasswordBox.Background = Brushes.Red;
                newPasswordBoxEnter.Background = Brushes.Red;
            }
        }

        private void loginCombo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (WidthText(loginCombo) > loginCombo.Width - 30)
            {
                loginCombo.Text = loginCombo.Text.Substring(0, loginCombo.Text.Length - 1);
                (e.OriginalSource as System.Windows.Controls.TextBox).CaretIndex = loginCombo.Text.Length;
            }
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            SettingsNewPassword(System.Windows.Visibility.Hidden);
        }

        private void button_language_Click(object sender, RoutedEventArgs e)
        {
            if(number_language + 1 >= InputLanguage.InstalledInputLanguages.Count)
            {
                number_language = 0;
                System.Windows.Forms.InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[number_language];
                button_language.Content = InputLanguage.InstalledInputLanguages[number_language].Culture.ThreeLetterISOLanguageName.ToUpper();
            }
            else
            {
                number_language++;
                System.Windows.Forms.InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[number_language];
                button_language.Content = InputLanguage.InstalledInputLanguages[number_language].Culture.ThreeLetterISOLanguageName.ToUpper();
            }
        }

    }
}
