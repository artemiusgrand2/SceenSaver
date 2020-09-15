using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Serialization;

using Authentificator.Enums;

namespace Authentificator
{
    [Serializable]
    public class UserAuthentData
    {
        public string Login;
        public string Password;
        public bool IsPasswordBlocked;
        public string RfidCod;
        public DateTime DateTimeWhenPasswordEnds;

        public UserAuthentData()
        {
            this.Login = String.Empty;
            this.Password = String.Empty;
            this.RfidCod = String.Empty;
            this.IsPasswordBlocked = false;
            this.DateTimeWhenPasswordEnds = DateTime.MaxValue;

        } // constructor

        public UserAuthentData(string inp_login, string inp_password, bool inp_isPasswordBlocked, DateTime inp_dateTimeWhenPasswordEnds, string RfidCod)
        {
            this.Login = inp_login;
            this.Password = inp_password;
            this.IsPasswordBlocked = inp_isPasswordBlocked;
            this.DateTimeWhenPasswordEnds = inp_dateTimeWhenPasswordEnds;
            this.RfidCod = RfidCod;

        } // constructor

    }

    public class Authentificators
    {
        /// <summary>
        /// Определяет действия, которые пользователь может выполнять с помощью экземпляра данного класса
        /// </summary>
        public enum AuthertificatorActions
        {
            All,                // пользователю разрешены все действия
            OnlyAuthentication  // пользователю разрешено лишь аутентифицироваться

        } // enum AuthertificatorActions


        string rfidPattern = @"[0-9]{3},[0-9]{5}";

        /// <summary>
        /// Full path to the file with authentication data of all the users
        /// (the file with user names and MD5-encrypted passwords)
        /// </summary>
        private string fullPathToUserAuthDataFile = String.Empty;

        /// <summary>
        /// Определяет действия, которые пользователь может выполнять с помощью экземпляра данного класса
        /// (по умолчанию - лишь аутентифицироваться)
        /// </summary>
        private AuthertificatorActions allowedUserActions = AuthertificatorActions.OnlyAuthentication;


        /// <summary>
        /// Constructor of the Authentificator class
        /// </summary>
        /// 
        /// <param name="inp_fullPathToUserAuthDataFile">full path to the file with authentication data
        /// of all the users (the file with user names and RSA-encrypted passwords)</param>
        public Authentificators(string inp_fullPathToUserAuthDataFile, AuthertificatorActions inp_allowedActions)
        {
            if (String.IsNullOrEmpty(inp_fullPathToUserAuthDataFile) == true)
                throw new Exception("Не указан полный путь к файлу аутентификационных данных пользователей");

            if (Directory.Exists(Path.GetDirectoryName(inp_fullPathToUserAuthDataFile)) == false)
                throw new Exception("Указанный каталог с файлом аутентификационных данных пользователей не существует");

            this.fullPathToUserAuthDataFile = inp_fullPathToUserAuthDataFile;
            this.allowedUserActions = inp_allowedActions;

            // Если пользователю разрешено лишь аутентифицироваться, то необходимо сделать проверку на существование
            // файла с аутентификационными данными пользователей и убедиться в том, что хотя бы одна запись с
            // аутентификационными данными в нем присутствует (в противном случае пользователь не сможет вообще
            // проводить процедуры аутентификации)

            if (this.allowedUserActions == AuthertificatorActions.OnlyAuthentication)
            {
                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("Указанный файл с аутентификационными данными пользователей не существует");

                List<UserAuthentData> _users = this.GetAllUsersInfo();
                //if (_users.Count == 0)
                //    throw new Exception("В файле аутентификационных данных пользователей отсутствуют записи");

            } // if

        } // constructor


        /// <summary>
        /// Checks the spelling of the given authentication string (data).
        /// The string can contain only latin letters, digits and a '_' symbol
        /// </summary>
        /// 
        /// <param name="inp_authentData">authentication string</param>
        /// 
        /// <param name="inp_whatIsChecked">a string that says what data is to be checked (this string is used
        /// in exception texts)</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing checking operations</exception>
        private void CheckAuthentDataSpelling(string inp_authentData, string inp_whatIsChecked)
        {
            char[] _latinLettersArray = new char[] {'A','a', 'B','b', 'C','c', 'D','d', 'E','e', 'F','f', 'G','g', 'H','h', 'I','i', 'J','j', 'K','k',
                'L','l', 'M','m', 'N','n', 'O','o', 'P','p', 'Q','q', 'R','r', 'S','s', 'T','t', 'U','u', 'V','v', 'W','w', 'X','x', 'Y','y', 'Z','z'};

            for (int _i = 0; _i < inp_authentData.Length; _i++)
            {
                if (_i == 0)
                {
                    if (Char.IsLetter(inp_authentData[_i]) == false)
                    {
                        throw new Exception("Первой в параметре " + inp_whatIsChecked + " должна быть латинская Буква");
                    } // if
                    else
                    {
                        if (Array.IndexOf(_latinLettersArray, inp_authentData[_i]) < 0)
                            throw new Exception("Первой в параметре " + inp_whatIsChecked + " должна быть Латинская буква");
                    } // else
                } // if
                else
                {
                    if (Char.IsDigit(inp_authentData[_i]) == false)
                    {
                        if (inp_authentData[_i] != '_')
                        {
                            if (Char.IsLetter(inp_authentData[_i]) == false)
                            {
                                throw new Exception("Значение параметра " + inp_whatIsChecked + " должно состоять из латинских букв, цифр и знака подчеркивания");
                            } // if
                            else
                            {
                                if (Array.IndexOf(_latinLettersArray, inp_authentData[_i]) < 0)
                                    throw new Exception("Значение параметра " + inp_whatIsChecked + " должно состоять из Латинских букв, цифр и знака подчеркивания");
                            } // else

                        } // if

                    } // if

                } // else

            } // for

        } // CheckAuthentDataSpelling


        /// <summary>
        /// Checks the user authentication data (login and password) for correctness
        /// </summary>
        /// 
        /// <param name="inp_loginName">user name to check</param>
        /// 
        /// <param name="inp_password">passowrd to check</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing checking operations</exception>
        private void CheckLoginData(string inp_loginName, string inp_password)
        {
            // Проверки для имени пользователя
            // -------------------------------
            CheckLogin(inp_loginName);
            // Проверки для пароля
            // -------------------
            CheckLogin(inp_password);
        } 

        private void CheckPassword(string inp_password)
        {
            // Проверки для пароля
            // -------------------
            if (String.IsNullOrEmpty(inp_password) == true)
                throw new Exception("Не указан Пароль");

            this.CheckAuthentDataSpelling(inp_password, "Пароль");

            if (inp_password.Length < 8)
                throw new Exception("Длина параметра Пароль должна быть не менее 8 символов");
            if (inp_password.Length > 16)
                throw new Exception("Длина параметра Пароль не должна превышать 16 символов");
        }

        private void CheckLogin(string inp_loginName)
        {
            // Проверки для имени пользователя
            // -------------------------------
            if (String.IsNullOrEmpty(inp_loginName) == true)
                throw new Exception("Не задано Имя пользователя");

            this.CheckAuthentDataSpelling(inp_loginName, "Имя пользователя");

            if (inp_loginName.Length < 8)
                throw new Exception("Длина параметра Имя пользователя должна быть не менее 8 символов");
            if (inp_loginName.Length > 24)
                throw new Exception("Длина параметра Имя пользователя не должна превышать 24 символов");
        }

        private void CheckRFID(ref string inp_rfidcard, ViewReader viewReader)
        {
            var rfidData = ConvertorDataRfid.ConvertFromStrToStr(inp_rfidcard, viewReader);
            if (!rfidData.Item2)
                throw new Exception("Неверный формат записи 'Код карточки'");

            // Проверки для rfid карточки
            // -------------------
            if (String.IsNullOrEmpty(inp_rfidcard) == true)
                throw new Exception("Не указан код карточки");
            inp_rfidcard = rfidData.Item1;
        } // 

        public enum UserAuthentResult
        {
            OK,
            WrongPassword,
            PasswordBlocked,
            PasswordExpired,
            AuthentDataNotFound

        } // enum UserAuthentResult

        public UserAuthentResult Authenticate(string inp_rfidcard, out string userName)
        {
            XmlSerializer _serializer = null;
            FileStream _stream = null;
            StreamReader _streamReader = null;
            MD5 _md5 = null;
            UnicodeEncoding _byteConverter = null;

            List<UserAuthentData> _userAuthentData = null;

            try
            {
                _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                _streamReader = new StreamReader(_stream, Encoding.Unicode);
                _md5 = new MD5CryptoServiceProvider();
                _byteConverter = new UnicodeEncoding();
                var match = Regex.Match(inp_rfidcard, rfidPattern);
                if (match.Success)
                {
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);
                    if (_userAuthentData != null)
                    {
                        foreach (UserAuthentData _uad in _userAuthentData)
                        {
                            if (_uad.RfidCod == _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_rfidcard))))
                            {
                                userName = _uad.Login;
                                return UserAuthentResult.OK;
                            }

                        } // foreach

                    } // if
                }
            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

            } // finally
            userName = string.Empty;
            return UserAuthentResult.AuthentDataNotFound;

        }


        public UserAuthentResult Authenticate(byte [] inp_rfidcard, out string userName)
        {
            XmlSerializer _serializer = null;
            FileStream _stream = null;
            StreamReader _streamReader = null;
            MD5 _md5 = null;
            UnicodeEncoding _byteConverter = null;

            List<UserAuthentData> _userAuthentData = null;

            try
            {
                _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                _streamReader = new StreamReader(_stream, Encoding.Unicode);
                _md5 = new MD5CryptoServiceProvider();
                _byteConverter = new UnicodeEncoding();
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);
                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        if (_uad.RfidCod == _byteConverter.GetString(_md5.ComputeHash(inp_rfidcard)))
                        {
                            userName = _uad.Login;
                            return UserAuthentResult.OK;
                        }

                    } // foreach

                } // if
            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

            } // finally
            userName = string.Empty;
            return UserAuthentResult.AuthentDataNotFound;

        }


        /// <summary>
        /// Checks whether the user with the given authentication data (login and password) exists and whether his
        /// authentication data is valid
        /// </summary>
        /// 
        /// <param name="inp_loginName">user name (login)</param>
        /// 
        /// <param name="inp_password">password</param>
        /// 
        /// <returns>the result of user authentication process</returns>
        /// 
        /// <exception cref="System.Exception">any exception while performing method</exception>
        public UserAuthentResult Authenticate(string inp_loginName, string inp_password)
        {
            // Вначале осуществляем проверку корректности заданных имени пользователя и пароля
            // -------------------------------------------------------------------------------

            this.CheckLoginData(inp_loginName, inp_password);

            // По имени пользователя ищем нужную запись в перечне аутентификационных
            // данных всех пользователей в заданном файле.
            // Если запись находим - проверяем действительность аккаунта пользователя (заблокированность пароля и срок его действия).
            // Если аккаунт оказывается действительным, проверяем пароль пользователя.
            // ----------------------------------------------------------------------------------------------------------------------

            XmlSerializer _serializer = null;
            FileStream _stream = null;
            StreamReader _streamReader = null;
            MD5 _md5 = null;
            UnicodeEncoding _byteConverter = null;

            List<UserAuthentData> _userAuthentData = null;

            try
            {
                _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                _streamReader = new StreamReader(_stream, Encoding.Unicode);
                _md5 = new MD5CryptoServiceProvider();
                _byteConverter = new UnicodeEncoding();

                _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        if (_uad.Login == inp_loginName)
                        {
                            if (_uad.IsPasswordBlocked == true)
                                return UserAuthentResult.PasswordBlocked;

                            if (_uad.Password == _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_password))))
                            {// если пароль указан правильно, тогда проверяем остальные параметры для этого пользователя


                                if (_uad.DateTimeWhenPasswordEnds < DateTime.Now)
                                    return UserAuthentResult.PasswordExpired;

                                return UserAuthentResult.OK;
                            }
                            else
                            {
                                return UserAuthentResult.WrongPassword;
                            }

                            break;

                        } // if

                    } // foreach

                } // if

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

            } // finally

            return UserAuthentResult.AuthentDataNotFound;

        } // Authenticate



        /// <summary>
        /// Gets a list of registered users' names with information about validity of their passwords.
        /// The method does not return passwords!
        /// </summary>
        /// 
        /// <returns>a list of all user names (registered) with information about validity of their passwords,
        /// or an empty list if there is no a user registered</returns>
        /// 
        /// <exception cref="System.Exception">any exception while getting data</exception>
        public List<UserAuthentData> GetAllUsersInfo()
        {
            List<UserAuthentData> _allUsersInfo = new List<UserAuthentData>();

            XmlSerializer _serializer = null;
            FileStream _stream = null;
            StreamReader _streamReader = null;

            List<UserAuthentData> _userAuthentData = null;

            try
            {
                _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
                using (_stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);

                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    if (_userAuthentData != null)
                    {
                        foreach (UserAuthentData _uad in _userAuthentData)
                        {
                            // Дабы не возвращать информацию о паролях, обнуляем ее
                            _allUsersInfo.Add(new UserAuthentData(_uad.Login, String.Empty, _uad.IsPasswordBlocked, _uad.DateTimeWhenPasswordEnds, _uad.RfidCod));

                        } // foreach

                    } // if
                }
            }
            finally
            {
                //if (_serializer != null)
                //    _serializer = null;

                //if (_stream != null)
                //{
                //    try { _stream.Close(); } // try
                //    catch { /* No message is needed here */ } // catch
                //    _stream = null;
                //} // if

                //if (_streamReader != null)
                //{
                //    try { _streamReader.Close(); } // try
                //    catch { /* No message is needed here */ } // catch
                //    _streamReader = null;
                //} // if

                //if (_userAuthentData != null)
                //{
                //    try { _userAuthentData.Clear(); } // try
                //    catch { /* No message is needed here*/ } // catch
                //    _userAuthentData = null;
                //} // if

            } // finally

            return _allUsersInfo;

        } // GetAllUsersInfo


        /// <summary>
        /// Saves user's authentication data
        /// </summary>
        /// 
        /// <param name="inp_userAuthentData">authentication data of the user</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing method</exception>
        public void SaveUserAuthentData(UserAuthentData inp_userAuthentData, ViewReader viewReader)
        {
            if (inp_userAuthentData == null)
                return;

            // Вначале осуществляем проверку корректности заданных имени пользователя и пароля
            // -------------------------------------------------------------------------------

            this.CheckLoginData(inp_userAuthentData.Login, inp_userAuthentData.Password);
            this.CheckRFID(ref inp_userAuthentData.RfidCod, viewReader);

            // Как только убедились в том, что аутентификационные данные - корректные, начинаем процедуру их сохранения
            // --------------------------------------------------------------------------------------------------------

            XmlSerializer _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
            FileStream _stream = null;
            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;
            MD5 _md5 = new MD5CryptoServiceProvider();
            UnicodeEncoding _byteConverter = new UnicodeEncoding();

            List<UserAuthentData> _userAuthentData = new List<UserAuthentData>();

            string _reservFileName = this.fullPathToUserAuthDataFile + "copyS";

            bool _createdFileCopy = false;
            bool _savedDataSuccesfully = false;

            try
            {
                // Если файл с аутентификационными данными пользователей существует, то из него считываем все
                // аутентификационные данные (будем проверять их на наличие нового имени пользователя)
                // ---------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == true)
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);

                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // Проверяем, присутствует ли в существующем перечне аутентификационных данных пользователей (если данный перечень не пуст)
                    // новый логин (если присутствует, то прерываем процедуру сохранения аутентификационных данных)
                    // ------------------------------------------------------------------------------------------------------------------------

                    if (_userAuthentData != null)
                    {
                        foreach (UserAuthentData _uad in _userAuthentData)
                        {
                            if (_uad.Login == inp_userAuthentData.Login)
                                throw new Exception("Пользователь с таким именем уже существует");

                        } // foreach

                    } // if

                    // Перед продолжением процедуры сохранения информации о новом пользователе, делаем резервную копию файла
                    // -----------------------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // if (файл с аутентификационными данными пользователей существует)

                // В любом случае файл с аутентификационными данными пользователей необходимо создать, если его не
                // существует (перезаписать существующий файл) и поместить в него информацию о новом пользователе
                // -----------------------------------------------------------------------------------------------              

                // Сохраняем новые аутентификационные данные (предварительно шифруя пароль)
                // ------------------------------------------------------------------------

                _userAuthentData.Add(new UserAuthentData(inp_userAuthentData.Login,
                                                        _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_userAuthentData.Password))),
                                                        inp_userAuthentData.IsPasswordBlocked,
                                                        inp_userAuthentData.DateTimeWhenPasswordEnds,
                                                         _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_userAuthentData.RfidCod)))));

                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Create, FileAccess.Write, FileShare.None);

                _streamWriter = new StreamWriter(_stream, Encoding.Unicode);

                _serializer.Serialize(_streamWriter, _userAuthentData);

                _savedDataSuccesfully = true;

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_streamWriter != null)
                {
                    try { _streamWriter.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamWriter = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

                // Если аутентификационные данные не удалось сохранить, при этом была создана резервная копия файла
                // с аутентификационными данными пользователей, то данная резервная копия становится основной (при этом
                // сама резервная копия не удаляется)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // SaveUserAuthentData


        /// <summary>
        /// Deletes an authenticated user
        /// </summary>
        /// 
        /// <param name="inp_loginName">the name of the user to delete</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing method</exception>
        public void DeleteUser(string inp_loginName)
        {
            if (String.IsNullOrEmpty(inp_loginName) == true)
                throw new Exception("Не указано имя пользователя для удаления");

            XmlSerializer _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
            FileStream _stream = null;
            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;

            List<UserAuthentData> _userAuthentData = new List<UserAuthentData>();

            string _reservFileName = this.fullPathToUserAuthDataFile + "copyD";

            bool _createdFileCopy = false;
            bool _savedDataSuccesfully = false;

            try
            {
                // Если файл с аутентификационными данными пользователей не существует, то удалять нечего -> ошибка
                // ------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("Файл с аутентификационными данными пользователей не существует");

                // Если же файл с аутентификационными данными пользователей существует, то из него считываем все
                // аутентификационные данные (будем проверять их на наличие заданного для удаления имени пользователя)
                // ---------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // Перед продолжением процедуры удаления информации о пользователе, делаем резервную копию файла
                    // ---------------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // Проверяем, присутствует ли в существующем перечне аутентификационных данных пользователей (если данный перечень не пуст)
                // заданный логин (если нет либо перечень пользователей пуст, то выдаем сообщение об ошибке)
                // ------------------------------------------------------------------------------------------------------------------------

                bool _userExists = false;
                int _userToDeleteIndex = -1;

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        _userToDeleteIndex++;

                        if (_uad.Login == inp_loginName)
                        {
                            _userExists = true;
                            break;
                        } // if

                    } // foreach

                    if (_userExists == false)
                        throw new Exception("Пользователя с заданным именем не существует");

                } // if
                else
                    throw new Exception("Пользователя с заданным именем не существует");

                // Удаляем аутентификационные данные заданного пользователя
                // --------------------------------------------------------

                _userAuthentData.RemoveAt(_userToDeleteIndex);

                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Create, FileAccess.Write, FileShare.None);

                _streamWriter = new StreamWriter(_stream, Encoding.Unicode);

                _serializer.Serialize(_streamWriter, _userAuthentData);

                _savedDataSuccesfully = true;

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_streamWriter != null)
                {
                    try { _streamWriter.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamWriter = null;
                } // if

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

                // Если аутентификационные данные не удалось сохранить, при этом была создана резервная копия файла
                // с аутентификационными данными пользователей, то данная резервная копия становится основной (при этом
                // сама резервная копия не удаляется)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // DeleteUser

        public void EditRfid(string inp_loginName, string inp_rfidcard, ViewReader viewReader)
        {
            // Вначале осуществляем проверку корректности заданных имени пользователя и пароля
            // -------------------------------------------------------------------------------
            this.CheckLogin(inp_loginName);
             this.CheckRFID(ref inp_rfidcard, viewReader);
            // Как только убедились в том, что аутентификационные данные - корректные, начинаем процедуру редактирования пароля пользователя
            // -----------------------------------------------------------------------------------------------------------------------------
            XmlSerializer _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
            FileStream _stream = null;
            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;
            MD5 _md5 = new MD5CryptoServiceProvider();
            UnicodeEncoding _byteConverter = new UnicodeEncoding();

            List<UserAuthentData> _userAuthentData = new List<UserAuthentData>();

            string _reservFileName = this.fullPathToUserAuthDataFile + "copyE";

            bool _createdFileCopy = false;
            bool _savedDataSuccesfully = false;

            try
            {
                // Если файл с аутентификационными данными пользователей не существует, то редактировать нечего -> ошибка
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("Файл с аутентификационными данными пользователей не существует");

                // Если же файл с аутентификационными данными пользователей существует, то из него считываем все
                // аутентификационные данные (будем проверять их на наличие пользователя, чей пароль необходимо отредактировать)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // Перед продолжением процедуры редактирования информации, делаем резервную копию файла
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // Проверяем, присутствует ли в существующем перечне аутентификационных данных пользователей (если данный перечень не пуст)
                // заданный логин (если нет либо перечень пользователей пуст, то выдаем сообщение об ошибке)
                // ------------------------------------------------------------------------------------------------------------------------

                bool _userExists = false;
                int _userToEditIndex = -1;

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        _userToEditIndex++;

                        if (_uad.Login == inp_loginName)
                        {
                            _userExists = true;
                            break;
                        } // if

                    } // foreach

                    if (_userExists == false)
                        throw new Exception("Пользователя с заданным именем не существует");

                } // if
                else
                    throw new Exception("Пользователя с заданным именем не существует");

                // Сохраняем новый пароль (предварительно шифруя его)
                // --------------------------------------------------

                _userAuthentData[_userToEditIndex].RfidCod = _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_rfidcard)));

                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Create, FileAccess.Write, FileShare.None);

                _streamWriter = new StreamWriter(_stream, Encoding.Unicode);

                _serializer.Serialize(_streamWriter, _userAuthentData);

                _savedDataSuccesfully = true;

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_streamWriter != null)
                {
                    try { _streamWriter.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamWriter = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

                // Если аутентификационные данные не удалось сохранить, при этом была создана резервная копия файла
                // с аутентификационными данными пользователей, то данная резервная копия становится основной (при этом
                // сама резервная копия не удаляется)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally
        }


        /// <summary>
        /// Edits password of the given user
        /// </summary>
        /// 
        /// <param name="inp_loginName">the name of the user whose password to edit</param>
        /// 
        /// <param name="inp_password">new user password</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing method</exception>
        public void EditPassword(string inp_loginName, string inp_password)
        {
            // Вначале осуществляем проверку корректности заданных имени пользователя и пароля
            // -------------------------------------------------------------------------------

            this.CheckLoginData(inp_loginName, inp_password);

            // Как только убедились в том, что аутентификационные данные - корректные, начинаем процедуру редактирования пароля пользователя
            // -----------------------------------------------------------------------------------------------------------------------------

            XmlSerializer _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
            FileStream _stream = null;
            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;
            MD5 _md5 = new MD5CryptoServiceProvider();
            UnicodeEncoding _byteConverter = new UnicodeEncoding();

            List<UserAuthentData> _userAuthentData = new List<UserAuthentData>();

            string _reservFileName = this.fullPathToUserAuthDataFile + "copyE";

            bool _createdFileCopy = false;
            bool _savedDataSuccesfully = false;

            try
            {
                // Если файл с аутентификационными данными пользователей не существует, то редактировать нечего -> ошибка
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("Файл с аутентификационными данными пользователей не существует");

                // Если же файл с аутентификационными данными пользователей существует, то из него считываем все
                // аутентификационные данные (будем проверять их на наличие пользователя, чей пароль необходимо отредактировать)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // Перед продолжением процедуры редактирования информации, делаем резервную копию файла
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // Проверяем, присутствует ли в существующем перечне аутентификационных данных пользователей (если данный перечень не пуст)
                // заданный логин (если нет либо перечень пользователей пуст, то выдаем сообщение об ошибке)
                // ------------------------------------------------------------------------------------------------------------------------

                bool _userExists = false;
                int _userToEditIndex = -1;

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        _userToEditIndex++;

                        if (_uad.Login == inp_loginName)
                        {
                            _userExists = true;
                            break;
                        } // if

                    } // foreach

                    if (_userExists == false)
                        throw new Exception("Пользователя с заданным именем не существует");

                } // if
                else
                    throw new Exception("Пользователя с заданным именем не существует");

                // Сохраняем новый пароль (предварительно шифруя его)
                // --------------------------------------------------

                _userAuthentData[_userToEditIndex].Password = _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_password)));

                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Create, FileAccess.Write, FileShare.None);

                _streamWriter = new StreamWriter(_stream, Encoding.Unicode);

                _serializer.Serialize(_streamWriter, _userAuthentData);

                _savedDataSuccesfully = true;

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_streamWriter != null)
                {
                    try { _streamWriter.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamWriter = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

                // Если аутентификационные данные не удалось сохранить, при этом была создана резервная копия файла
                // с аутентификационными данными пользователей, то данная резервная копия становится основной (при этом
                // сама резервная копия не удаляется)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // EditPassword


        /// <summary>
        /// Edits password action date and time of the given user
        /// </summary>
        /// 
        /// <param name="inp_loginName">the name of the user whose password action to edit</param>
        /// 
        /// <param name="inp_password">new password action date and time</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing method</exception>
        public void EditPasswordAction(string inp_loginName, DateTime inp_passwordAction)
        {
            if (String.IsNullOrEmpty(inp_loginName) == true)
                throw new Exception("Не указано имя пользователя для редактирования срока действия пароля");

            XmlSerializer _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
            FileStream _stream = null;
            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;
            MD5 _md5 = new MD5CryptoServiceProvider();
            UnicodeEncoding _byteConverter = new UnicodeEncoding();

            List<UserAuthentData> _userAuthentData = new List<UserAuthentData>();

            string _reservFileName = this.fullPathToUserAuthDataFile + "copyE";

            bool _createdFileCopy = false;
            bool _savedDataSuccesfully = false;

            try
            {
                // Если файл с аутентификационными данными пользователей не существует, то редактировать нечего -> ошибка
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("Файл с аутентификационными данными пользователей не существует");

                // Если же файл с аутентификационными данными пользователей существует, то из него считываем все
                // аутентификационные данные (будем проверять их на наличие пользователя, чьи данные необходимо отредактировать)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // Перед продолжением процедуры редактирования информации, делаем резервную копию файла
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // Проверяем, присутствует ли в существующем перечне аутентификационных данных пользователей (если данный перечень не пуст)
                // заданный логин (если нет либо перечень пользователей пуст, то выдаем сообщение об ошибке)
                // ------------------------------------------------------------------------------------------------------------------------

                bool _userExists = false;
                int _userToEditIndex = -1;

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        _userToEditIndex++;

                        if (_uad.Login == inp_loginName)
                        {
                            _userExists = true;
                            break;
                        } // if

                    } // foreach

                    if (_userExists == false)
                        throw new Exception("Пользователя с заданным именем не существует");

                } // if
                else
                    throw new Exception("Пользователя с заданным именем не существует");

                // Сохраняем новый срок действия пароля пользователя (если он отличен от предыдущего)
                // ----------------------------------------------------------------------------------

                if (_userAuthentData[_userToEditIndex].DateTimeWhenPasswordEnds != inp_passwordAction)
                {
                    _userAuthentData[_userToEditIndex].DateTimeWhenPasswordEnds = inp_passwordAction;

                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Create, FileAccess.Write, FileShare.None);

                    _streamWriter = new StreamWriter(_stream, Encoding.Unicode);

                    _serializer.Serialize(_streamWriter, _userAuthentData);

                } // if

                _savedDataSuccesfully = true;

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_streamWriter != null)
                {
                    try { _streamWriter.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamWriter = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

                // Если аутентификационные данные не удалось сохранить, при этом была создана резервная копия файла
                // с аутентификационными данными пользователей, то данная резервная копия становится основной (при этом
                // сама резервная копия не удаляется)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // EditPasswordAction


        /// <summary>
        /// Sets status of the user password (blocked or not)
        /// </summary>
        /// 
        /// <param name="inp_loginName">the name of the user whose password's status to set</param>
        /// 
        /// <param name="inp_blockPassword">blocked password status (true = block passwoed, false = unblock password)</param>
        /// 
        /// <exception cref="System.Exception">any exception while performing method</exception>
        public void EditPasswordStatus(string inp_loginName, bool inp_blockPassword)
        {
            if (String.IsNullOrEmpty(inp_loginName) == true)
                throw new Exception("Не указано имя пользователя для установки статуса блокировки пароля");

            XmlSerializer _serializer = new XmlSerializer(typeof(List<UserAuthentData>));
            FileStream _stream = null;
            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;
            MD5 _md5 = new MD5CryptoServiceProvider();
            UnicodeEncoding _byteConverter = new UnicodeEncoding();

            List<UserAuthentData> _userAuthentData = new List<UserAuthentData>();

            string _reservFileName = this.fullPathToUserAuthDataFile + "copyE";

            bool _createdFileCopy = false;
            bool _savedDataSuccesfully = false;

            try
            {
                // Если файл с аутентификационными данными пользователей не существует, то редактировать нечего -> ошибка
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("Файл с аутентификационными данными пользователей не существует");

                // Если же файл с аутентификационными данными пользователей существует, то из него считываем все
                // аутентификационные данные (будем проверять их на наличие пользователя, чьи данные необходимо отредактировать)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // Перед продолжением процедуры редактирования информации, делаем резервную копию файла
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // Проверяем, присутствует ли в существующем перечне аутентификационных данных пользователей (если данный перечень не пуст)
                // заданный логин (если нет либо перечень пользователей пуст, то выдаем сообщение об ошибке)
                // ------------------------------------------------------------------------------------------------------------------------

                bool _userExists = false;
                int _userToEditIndex = -1;

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        _userToEditIndex++;

                        if (_uad.Login == inp_loginName)
                        {
                            _userExists = true;
                            break;
                        } // if

                    } // foreach

                    if (_userExists == false)
                        throw new Exception("Пользователя с заданным именем не существует");

                } // if
                else
                    throw new Exception("Пользователя с заданным именем не существует");

                // Сохраняем новый статус пароля пользователя (если он отличен от предыдущего)
                // ---------------------------------------------------------------------------

                if (_userAuthentData[_userToEditIndex].IsPasswordBlocked != inp_blockPassword)
                {
                    _userAuthentData[_userToEditIndex].IsPasswordBlocked = inp_blockPassword;

                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Create, FileAccess.Write, FileShare.None);

                    _streamWriter = new StreamWriter(_stream, Encoding.Unicode);

                    _serializer.Serialize(_streamWriter, _userAuthentData);

                } // if

                _savedDataSuccesfully = true;

            } // try
            finally
            {
                if (_serializer != null)
                    _serializer = null;

                if (_stream != null)
                {
                    try { _stream.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _stream = null;
                } // if

                if (_streamReader != null)
                {
                    try { _streamReader.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamReader = null;
                } // if

                if (_streamWriter != null)
                {
                    try { _streamWriter.Close(); } // try
                    catch { /* No message is needed here */ } // catch
                    _streamWriter = null;
                } // if

                if (_md5 != null)
                {
                    try { _md5.Clear(); } // try
                    catch { /* No message is needed here */ } // catch
                    _md5 = null;
                } // if

                if (_byteConverter != null)
                    _byteConverter = null;

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

                // Если аутентификационные данные не удалось сохранить, при этом была создана резервная копия файла
                // с аутентификационными данными пользователей, то данная резервная копия становится основной (при этом
                // сама резервная копия не удаляется)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // EditPasswordStatus

    } // class Authentificator

}
