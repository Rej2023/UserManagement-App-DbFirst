using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using UserManagement_App_DbFirst.Models;
using System.Drawing.Printing;
using System.Web.UI;

namespace UserManagement_App_DbFirst.Services
{
    public class UserServices
    {
        bool invalid = false;
        public enum UserHistoryType
        {
            Inserted = 1,
            Updated = 2,
            Deleted = 3,
            Activated = 4,
            Deactivated = 5
        }
        public string Insert_data(User _user, string email)
        {

            string result = "";
            try
            {
                using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
                {
                    var loginUserInfo = DBUser.Users.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
                    var userData = DBUser.Users.FirstOrDefault(d => d.Email.ToLower() == _user.Email.ToLower() && d.UserID != _user.UserID);
                    if (userData != null) //if name exist update data
                    {
                        result = "User already Exists!";
                    }
                    else
                    {
                        if (userData == null)
                        {
                            _user.Inserted = DateTime.Now;
                            _user.Updated = DateTime.Now;
                        }
                        else
                        {
                            _user.Updated = DateTime.Now;
                        }
                        _user.InsertedBy = loginUserInfo != null ? loginUserInfo.UserID : 0;
                        _user.UpdatedBy = loginUserInfo != null ? loginUserInfo.UserID : 0;
                        _user.Active = true;
                        DBUser.Users.Add(_user);
                        var res = DBUser.SaveChanges();
                        if (res == 1)
                        {
                            result = "Success";
                            // save the history
                            setUserHistoryData(_user, loginUserInfo != null ? loginUserInfo.UserID : _user.UserID);
                        }
                        else
                        {
                            result = "Failed";
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public User GetUserById(int id)
        {
            User result = new User();
            try
            {
                string ConnectionString = @"Data Source=CT00702;Initial Catalog=UserManagement2023;Integrated Security=SSPI";
                //Create the SqlConnection object
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cm = connection.CreateCommand())
                    {
                        cm.CommandTimeout = 0;
                        cm.CommandType = CommandType.StoredProcedure;
                        cm.CommandText = "spGetUsers";
                        cm.Parameters.Add("@UserID", SqlDbType.Decimal).Value = id;
                        cm.Parameters.Add("@Email", SqlDbType.VarChar).Value = "%%";
                        cm.Parameters.Add("@OffsetValue", SqlDbType.Decimal).Value = 0;
                        cm.Parameters.Add("@PagingSize", SqlDbType.Decimal).Value = 1;

                        using (SqlDataReader dr = cm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                result = GetUserDto(dr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Occurred: {ex.Message}");
            }
            return result;
        }
        public int GetUserCount()
        {
            User result = new User();
            try
            {
                string ConnectionString = @"Data Source=CT00702;Initial Catalog=UserManagement2023;Integrated Security=SSPI";
                //Create the SqlConnection object
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cm = connection.CreateCommand())
                    {
                        cm.CommandTimeout = 0;
                        cm.CommandType = CommandType.StoredProcedure;
                        cm.CommandText = "spGetUsersCount";

                        using (SqlDataReader dr = cm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                result = GetUserCountDto(dr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Occurred: {ex.Message}");
            }
            return result.TotalRecords;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            User result = new User();
            await Task.Run(() =>
            {
                //using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
                //{
                //    result = DBUser.Users.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
                //}
                try
                {
                    string ConnectionString = @"Data Source=CT00702;Initial Catalog=UserManagement2023;Integrated Security=SSPI";
                    //Create the SqlConnection object
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand cm = connection.CreateCommand())
                        {
                            cm.CommandTimeout = 0;
                            cm.CommandType = CommandType.StoredProcedure;
                            cm.CommandText = "spGetUsers";
                            cm.Parameters.Add("@UserID", SqlDbType.Decimal).Value = decimal.Zero;
                            cm.Parameters.Add("@Email", SqlDbType.VarChar).Value = '%' + email + '%';
                            cm.Parameters.Add("@OffsetValue", SqlDbType.Decimal).Value =0;
                            cm.Parameters.Add("@PagingSize", SqlDbType.Decimal).Value = 1;
                            using (SqlDataReader dr = cm.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result = GetUserDto(dr);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception Occurred: {ex.Message}");
                }
            });
            return result;
        }

        public List<User> GetUsers(int page, int pageSize)
        {
            List<User> result = new List<User>();
            //using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
            //{
            //    result = DBUser.Users.Where(c => c.Deleted == false).ToList();
            //}
            try
            {
                string ConnectionString = @"Data Source=CT00702;Initial Catalog=UserManagement2023;Integrated Security=SSPI";
                //Create the SqlConnection object
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cm = connection.CreateCommand())
                    {
                        cm.CommandTimeout = 0;
                        cm.CommandType = CommandType.StoredProcedure;
                        cm.CommandText = "spGetUsers";
                        cm.Parameters.Add("@UserID", SqlDbType.Decimal).Value = Decimal.Zero;
                        cm.Parameters.Add("@Email", SqlDbType.VarChar).Value = "%%";
                        cm.Parameters.Add("@OffsetValue", SqlDbType.Decimal).Value = (page-1)*pageSize;
                        cm.Parameters.Add("@PagingSize", SqlDbType.Decimal).Value = pageSize;

                        using (SqlDataReader dr = cm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                User userDto = GetUserDto(dr);
                                result.Add(userDto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Occurred: {ex.Message}");
            }
            return result;
        }

        public string UpdateUser(User user, string email)
        {
            string result = "";
            try
            {
                using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
                {
                    var loginUserInfo = DBUser.Users.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
                    User data = DBUser.Users.FirstOrDefault(d => d.UserID == user.UserID);
                    if (data != null)
                    {
                        data.FirstName = user.FirstName;
                        data.LastName = user.LastName;
                        data.Email = user.Email;
                        data.Contact = user.Contact;
                        data.Updated = DateTime.Now;
                        data.UpdatedBy = loginUserInfo != null ? loginUserInfo.UserID : data.UserID;

                        var res = DBUser.SaveChanges();
                        if (res == 1)
                        {
                            result = "Success";
                            // save the history
                            setUserHistoryData(data, loginUserInfo != null ? loginUserInfo.UserID : data.UserID);
                        }
                        else
                        {
                            result = "Failed";
                        }
                    }
                    else
                        result = "Not Found";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public void setUserHistoryData(User user, int loginUserID)
        {
            // save the history
            UserHistory history = new UserHistory();
            history.UserID = user.UserID;
            history.FirstName = user.FirstName;
            history.LastName = user.LastName;
            history.Email = user.Email;
            history.Contact = user.Contact;
            history.Active = user.Active;
            history.Inserted = user.Inserted;
            history.InsertedBy = loginUserID;
            history.Updated = user.Updated;
            history.UpdatedBy = loginUserID;

            Insert_UserHistorydata(history);
        }
        public string UpdateUserStatus(User user, string email)
        {
            string result = "";
            try
            {
                using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
                {
                    var loginUserInfo = DBUser.Users.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
                    User data = DBUser.Users.FirstOrDefault(d => d.UserID == user.UserID);
                    if (data != null)
                    {
                        data.Active = user.Active;
                        var res = DBUser.SaveChanges();
                        if (res == 1)
                        {
                            result = "Success";
                            // save the user history
                            setUserHistoryData(data, loginUserInfo.UserID);
                        }
                        else
                        {
                            result = "Failed";
                        }
                    }
                    else
                        result = "Not Found";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string Delete(int id, string email)
        {
            string result = "";
            try
            {

                using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
                {
                    var loginUserInfo = DBUser.Users.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
                    User data = DBUser.Users.FirstOrDefault(d => d.UserID == id);
                    data.Deleted = true;
                    data.Active = false;
                    var res = DBUser.SaveChanges();
                    if (res == 1)
                    {
                        result = "Success";
                        // save the user history
                        setUserHistoryData(data, loginUserInfo.UserID);
                    }
                    else
                    {
                        result = "Failed";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;

            }
            return result;
        }

        public string Insert_UserHistorydata(UserHistory _user)
        {
            string result = "";
            try
            {
                using (UserManagement2023Entities1 DBUser = new UserManagement2023Entities1())
                {
                    UserHistory history = new UserHistory();
                    history.UserID = _user.UserID;
                    history.FirstName = _user.FirstName;
                    history.LastName = _user.LastName;
                    history.Email = _user.Email;
                    history.Active = _user.Active;
                    history.Contact = _user.Contact;
                    history.Inserted = _user.Inserted;
                    history.InsertedBy = _user.InsertedBy;
                    history.Updated = _user.Updated;
                    history.UpdatedBy = _user.UpdatedBy;

                    DBUser.UserHistories.Add(history);
                    var res = DBUser.SaveChanges();
                    if (res == 1)
                    {
                        result = "Success";
                    }
                    else
                    {
                        result = "Failed";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
                if (invalid)
                    return false;

                // Return true if strIn is in valid email format.
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }

            return match.Groups[1].Value + domainName;
        }

        private User GetUserDto(SqlDataReader dr)
        {

            User userDto = new User();
            userDto.Role = dr["Name"].ToString();
            userDto.UserID = Convert.ToInt32(dr["UserID"]);
            userDto.Email = dr["Email"].ToString();
            userDto.Active = (bool)dr["Active"];
            userDto.Inserted = Convert.ToDateTime(dr["Inserted"]);
            userDto.FirstName = dr["FirstName"].ToString();
            userDto.LastName = dr["LastName"].ToString();
            userDto.Contact = dr["Contact"].ToString();
            userDto.Updated = Convert.ToDateTime(dr["Updated"]);
            userDto.InsertedBy = Convert.ToInt32(dr["InsertedBy"]);
            userDto.UpdatedBy = Convert.ToInt32(dr["UpdatedBy"]);
            userDto.Deleted = (bool)dr["Deleted"];
            
            return userDto;
        }
        private User GetUserCountDto(SqlDataReader dr)
        {
            User userDto = new User();
            userDto.TotalRecords = Convert.ToInt32(dr["TotalRecords"].ToString());
            return userDto;
        }

    }
}