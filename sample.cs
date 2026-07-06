using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DemoProject
{
    public class UserService
    {
        private string connectionString = "Server=localhost;Database=UsersDB;User Id=sa;Password=123456;";

        public List<User> GetUsers(string role)
        {
            List<User> users = new List<User>();

            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string query = "SELECT * FROM Users WHERE Role = '" + role + "'";

            SqlCommand command = new SqlCommand(query, connection);

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                User user = new User();

                user.Id = Convert.ToInt32(reader["Id"]);
                user.Name = reader["Name"].ToString();
                user.Email = reader["Email"].ToString();

                users.Add(user);
            }

            connection.Close();

            return users;
        }

        public void DeleteUser(int id)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            string query = $"DELETE FROM Users WHERE Id={id}";

            SqlCommand cmd = new SqlCommand(query, connection);

            cmd.ExecuteNonQuery();

            connection.Close();
        }

        public bool Login(string username, string password)
        {
            if(username == "admin" && password == "admin123")
            {
                return true;
            }

            return false;
        }

        public void SaveLog(string message)
        {
            System.IO.File.AppendAllText( message);
        }
    }

    public class User
    {
        public int Id;
        public string Name;
        public string Email;
    }
}
