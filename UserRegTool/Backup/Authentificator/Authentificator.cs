using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WorkWithUsers
{
    [Serializable]
    public class UserAuthentData
    {
        public string Login;
        public string Password;
        public bool IsPasswordBlocked;
        public DateTime DateTimeWhenPasswordEnds;

        public UserAuthentData()
        {
            this.Login = String.Empty;
            this.Password = String.Empty;
            this.IsPasswordBlocked = false;
            this.DateTimeWhenPasswordEnds = DateTime.MaxValue;

        } // constructor

        public UserAuthentData(string inp_login, string inp_password, bool inp_isPasswordBlocked, DateTime inp_dateTimeWhenPasswordEnds)
        {
            this.Login = inp_login;
            this.Password = inp_password;
            this.IsPasswordBlocked = inp_isPasswordBlocked;
            this.DateTimeWhenPasswordEnds = inp_dateTimeWhenPasswordEnds;

        } // constructor

    } // class UserAuthentData


    /// <summary>
    /// Provides mechanisms of work with user authentication data
    /// </summary>
    public class Authentificator
    {
        /// <summary>
        /// ���������� ��������, ������� ������������ ����� ��������� � ������� ���������� ������� ������
        /// </summary>
        public enum AuthertificatorActions
        {
            All,                // ������������ ��������� ��� ��������
            OnlyAuthentication  // ������������ ��������� ���� �������������������

        } // enum AuthertificatorActions


        /// <summary>
        /// Full path to the file with authentication data of all the users
        /// (the file with user names and MD5-encrypted passwords)
        /// </summary>
        private string fullPathToUserAuthDataFile = String.Empty;

        /// <summary>
        /// ���������� ��������, ������� ������������ ����� ��������� � ������� ���������� ������� ������
        /// (�� ��������� - ���� �������������������)
        /// </summary>
        private AuthertificatorActions allowedUserActions = AuthertificatorActions.OnlyAuthentication;


        /// <summary>
        /// Constructor of the Authentificator class
        /// </summary>
        /// 
        /// <param name="inp_fullPathToUserAuthDataFile">full path to the file with authentication data
        /// of all the users (the file with user names and RSA-encrypted passwords)</param>
        public Authentificator(string inp_fullPathToUserAuthDataFile, AuthertificatorActions inp_allowedActions)
        {
            if (String.IsNullOrEmpty(inp_fullPathToUserAuthDataFile) == true)
                throw new Exception("�� ������ ������ ���� � ����� ������������������ ������ �������������");

            if (Directory.Exists(Path.GetDirectoryName(inp_fullPathToUserAuthDataFile)) == false)
                throw new Exception("��������� ������� � ������ ������������������ ������ ������������� �� ����������");

            this.fullPathToUserAuthDataFile = inp_fullPathToUserAuthDataFile;
            this.allowedUserActions = inp_allowedActions;

            // ���� ������������ ��������� ���� �������������������, �� ���������� ������� �������� �� �������������
            // ����� � ������������������� ������� ������������� � ��������� � ���, ��� ���� �� ���� ������ �
            // ������������������� ������� � ��� ������������ (� ��������� ������ ������������ �� ������ ������
            // ��������� ��������� ��������������)

            if (this.allowedUserActions == AuthertificatorActions.OnlyAuthentication)
            {
                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("��������� ���� � ������������������� ������� ������������� �� ����������");

                List<UserAuthentData> _users = this.GetAllUsersInfo();
                if (_users.Count == 0)
                    throw new Exception("� ����� ������������������ ������ ������������� ����������� ������");

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
                        throw new Exception("������ � ��������� " + inp_whatIsChecked + " ������ ���� ��������� �����");
                    } // if
                    else
                    {
                        if (Array.IndexOf(_latinLettersArray, inp_authentData[_i]) < 0)
                            throw new Exception("������ � ��������� " + inp_whatIsChecked + " ������ ���� ��������� �����");
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
                                throw new Exception("�������� ��������� " + inp_whatIsChecked + " ������ �������� �� ��������� ����, ���� � ����� �������������");
                            } // if
                            else
                            {
                                if (Array.IndexOf(_latinLettersArray, inp_authentData[_i]) < 0)
                                    throw new Exception("�������� ��������� " + inp_whatIsChecked + " ������ �������� �� ��������� ����, ���� � ����� �������������");
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
            // �������� ��� ����� ������������
            // -------------------------------

            if (String.IsNullOrEmpty(inp_loginName) == true)
                throw new Exception("�� ������ ��� ������������");

            this.CheckAuthentDataSpelling(inp_loginName, "��� ������������");

            if (inp_loginName.Length < 8)
                throw new Exception("����� ��������� ��� ������������ ������ ���� �� ����� 8 ��������");
            if (inp_loginName.Length > 24)
                throw new Exception("����� ��������� ��� ������������ �� ������ ��������� 24 ��������");

            // �������� ��� ������
            // -------------------

            if (String.IsNullOrEmpty(inp_password) == true)
                throw new Exception("�� ������ ������");

            this.CheckAuthentDataSpelling(inp_password, "������");

            if (inp_password.Length < 8)
                throw new Exception("����� ��������� ������ ������ ���� �� ����� 8 ��������");
            if (inp_password.Length > 16)
                throw new Exception("����� ��������� ������ �� ������ ��������� 16 ��������");

        } // CheckLoginData


        public enum UserAuthentResult
        {
            OK,
            WrongPassword,
            PasswordBlocked,
            PasswordExpired,
            AuthentDataNotFound

        } // enum UserAuthentResult


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
            // ������� ������������ �������� ������������ �������� ����� ������������ � ������
            // -------------------------------------------------------------------------------

            this.CheckLoginData(inp_loginName, inp_password);

            // �� ����� ������������ ���� ������ ������ � ������� ������������������
            // ������ ���� ������������� � �������� �����.
            // ���� ������ ������� - ��������� ���������������� �������� ������������ (����������������� ������ � ���� ��� ��������).
            // ���� ������� ����������� ��������������, ��������� ������ ������������.
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
                            {// ���� ������ ������ ���������, ����� ��������� ��������� ��������� ��� ����� ������������
                               

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
                _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                _streamReader = new StreamReader(_stream, Encoding.Unicode);

                _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                if (_userAuthentData != null)
                {
                    foreach (UserAuthentData _uad in _userAuthentData)
                    {
                        // ���� �� ���������� ���������� � �������, �������� ��
                        _allUsersInfo.Add(new UserAuthentData(_uad.Login, String.Empty, _uad.IsPasswordBlocked, _uad.DateTimeWhenPasswordEnds));

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

                if (_userAuthentData != null)
                {
                    try { _userAuthentData.Clear(); } // try
                    catch { /* No message is needed here*/ } // catch
                    _userAuthentData = null;
                } // if

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
        public void SaveUserAuthentData(UserAuthentData inp_userAuthentData)
        {
            if (inp_userAuthentData == null)
                return;

            // ������� ������������ �������� ������������ �������� ����� ������������ � ������
            // -------------------------------------------------------------------------------

            this.CheckLoginData(inp_userAuthentData.Login, inp_userAuthentData.Password);

            // ��� ������ ��������� � ���, ��� ������������������ ������ - ����������, �������� ��������� �� ����������
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
                // ���� ���� � ������������������� ������� ������������� ����������, �� �� ���� ��������� ���
                // ������������������ ������ (����� ��������� �� �� ������� ������ ����� ������������)
                // ---------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == true)
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);

                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // ���������, ������������ �� � ������������ ������� ������������������ ������ ������������� (���� ������ �������� �� ����)
                    // ����� ����� (���� ������������, �� ��������� ��������� ���������� ������������������ ������)
                    // ------------------------------------------------------------------------------------------------------------------------

                    if (_userAuthentData != null)
                    {
                        foreach (UserAuthentData _uad in _userAuthentData)
                        {
                            if (_uad.Login == inp_userAuthentData.Login)
                                throw new Exception("������������ � ����� ������ ��� ����������");

                        } // foreach

                    } // if

                    // ����� ������������ ��������� ���������� ���������� � ����� ������������, ������ ��������� ����� �����
                    // -----------------------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // if (���� � ������������������� ������� ������������� ����������)

                // � ����� ������ ���� � ������������������� ������� ������������� ���������� �������, ���� ��� ��
                // ���������� (������������ ������������ ����) � ��������� � ���� ���������� � ����� ������������
                // -----------------------------------------------------------------------------------------------              

                // ��������� ����� ������������������ ������ (�������������� ������ ������)
                // ------------------------------------------------------------------------

                _userAuthentData.Add(new UserAuthentData(inp_userAuthentData.Login,
                                                        _byteConverter.GetString(_md5.ComputeHash(_byteConverter.GetBytes(inp_userAuthentData.Password))),
                                                        inp_userAuthentData.IsPasswordBlocked,
                                                        inp_userAuthentData.DateTimeWhenPasswordEnds));

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

                // ���� ������������������ ������ �� ������� ���������, ��� ���� ���� ������� ��������� ����� �����
                // � ������������������� ������� �������������, �� ������ ��������� ����� ���������� �������� (��� ����
                // ���� ��������� ����� �� ���������)

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
                throw new Exception("�� ������� ��� ������������ ��� ��������");

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
                // ���� ���� � ������������������� ������� ������������� �� ����������, �� ������� ������ -> ������
                // ------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("���� � ������������������� ������� ������������� �� ����������");

                // ���� �� ���� � ������������������� ������� ������������� ����������, �� �� ���� ��������� ���
                // ������������������ ������ (����� ��������� �� �� ������� ��������� ��� �������� ����� ������������)
                // ---------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // ����� ������������ ��������� �������� ���������� � ������������, ������ ��������� ����� �����
                    // ---------------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // ���������, ������������ �� � ������������ ������� ������������������ ������ ������������� (���� ������ �������� �� ����)
                // �������� ����� (���� ��� ���� �������� ������������� ����, �� ������ ��������� �� ������)
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
                        throw new Exception("������������ � �������� ������ �� ����������");

                } // if
                else
                    throw new Exception("������������ � �������� ������ �� ����������");

                // ������� ������������������ ������ ��������� ������������
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

                // ���� ������������������ ������ �� ������� ���������, ��� ���� ���� ������� ��������� ����� �����
                // � ������������������� ������� �������������, �� ������ ��������� ����� ���������� �������� (��� ����
                // ���� ��������� ����� �� ���������)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // DeleteUser


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
            // ������� ������������ �������� ������������ �������� ����� ������������ � ������
            // -------------------------------------------------------------------------------

            this.CheckLoginData(inp_loginName, inp_password);

            // ��� ������ ��������� � ���, ��� ������������������ ������ - ����������, �������� ��������� �������������� ������ ������������
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
                // ���� ���� � ������������������� ������� ������������� �� ����������, �� ������������� ������ -> ������
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("���� � ������������������� ������� ������������� �� ����������");

                // ���� �� ���� � ������������������� ������� ������������� ����������, �� �� ���� ��������� ���
                // ������������������ ������ (����� ��������� �� �� ������� ������������, ��� ������ ���������� ���������������)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // ����� ������������ ��������� �������������� ����������, ������ ��������� ����� �����
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // ���������, ������������ �� � ������������ ������� ������������������ ������ ������������� (���� ������ �������� �� ����)
                // �������� ����� (���� ��� ���� �������� ������������� ����, �� ������ ��������� �� ������)
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
                        throw new Exception("������������ � �������� ������ �� ����������");

                } // if
                else
                    throw new Exception("������������ � �������� ������ �� ����������");

                // ��������� ����� ������ (�������������� ������ ���)
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

                // ���� ������������������ ������ �� ������� ���������, ��� ���� ���� ������� ��������� ����� �����
                // � ������������������� ������� �������������, �� ������ ��������� ����� ���������� �������� (��� ����
                // ���� ��������� ����� �� ���������)

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
                throw new Exception("�� ������� ��� ������������ ��� �������������� ����� �������� ������");

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
                // ���� ���� � ������������������� ������� ������������� �� ����������, �� ������������� ������ -> ������
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("���� � ������������������� ������� ������������� �� ����������");

                // ���� �� ���� � ������������������� ������� ������������� ����������, �� �� ���� ��������� ���
                // ������������������ ������ (����� ��������� �� �� ������� ������������, ��� ������ ���������� ���������������)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // ����� ������������ ��������� �������������� ����������, ������ ��������� ����� �����
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // ���������, ������������ �� � ������������ ������� ������������������ ������ ������������� (���� ������ �������� �� ����)
                // �������� ����� (���� ��� ���� �������� ������������� ����, �� ������ ��������� �� ������)
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
                        throw new Exception("������������ � �������� ������ �� ����������");

                } // if
                else
                    throw new Exception("������������ � �������� ������ �� ����������");

                // ��������� ����� ���� �������� ������ ������������ (���� �� ������� �� �����������)
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

                // ���� ������������������ ������ �� ������� ���������, ��� ���� ���� ������� ��������� ����� �����
                // � ������������������� ������� �������������, �� ������ ��������� ����� ���������� �������� (��� ����
                // ���� ��������� ����� �� ���������)

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
                throw new Exception("�� ������� ��� ������������ ��� ��������� ������� ���������� ������");

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
                // ���� ���� � ������������������� ������� ������������� �� ����������, �� ������������� ������ -> ������
                // ------------------------------------------------------------------------------------------------------

                if (File.Exists(this.fullPathToUserAuthDataFile) == false)
                    throw new Exception("���� � ������������������� ������� ������������� �� ����������");

                // ���� �� ���� � ������������������� ������� ������������� ����������, �� �� ���� ��������� ���
                // ������������������ ������ (����� ��������� �� �� ������� ������������, ��� ������ ���������� ���������������)
                // -------------------------------------------------------------------------------------------------------------

                else
                {
                    _stream = new FileStream(this.fullPathToUserAuthDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _streamReader = new StreamReader(_stream, Encoding.Unicode);
                    _userAuthentData = (List<UserAuthentData>)_serializer.Deserialize(_streamReader);

                    _stream.Close();
                    _streamReader.Close();

                    // ����� ������������ ��������� �������������� ����������, ������ ��������� ����� �����
                    // ------------------------------------------------------------------------------------

                    File.Copy(this.fullPathToUserAuthDataFile, _reservFileName, true);
                    _createdFileCopy = true;

                } // else

                // ���������, ������������ �� � ������������ ������� ������������������ ������ ������������� (���� ������ �������� �� ����)
                // �������� ����� (���� ��� ���� �������� ������������� ����, �� ������ ��������� �� ������)
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
                        throw new Exception("������������ � �������� ������ �� ����������");

                } // if
                else
                    throw new Exception("������������ � �������� ������ �� ����������");

                // ��������� ����� ������ ������ ������������ (���� �� ������� �� �����������)
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

                // ���� ������������������ ������ �� ������� ���������, ��� ���� ���� ������� ��������� ����� �����
                // � ������������������� ������� �������������, �� ������ ��������� ����� ���������� �������� (��� ����
                // ���� ��������� ����� �� ���������)

                if ((_savedDataSuccesfully == false) && (_createdFileCopy == true))
                {
                    File.Copy(_reservFileName, this.fullPathToUserAuthDataFile, true);

                } // if

            } // finally

        } // EditPasswordStatus

    } // class Authentificator

} // namespace WorkWithUsers