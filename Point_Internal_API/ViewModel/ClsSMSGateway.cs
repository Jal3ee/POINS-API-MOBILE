using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Point_Internal_API.Models;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Point_Internal_API.ViewModel
{
    public class ClsSMSGateway
    {
        public MySqlConnection connection;
        public string server;
        public string database;
        public string uid;
        public string password;
        public string port;

        public bool OpenConnection(ref MySqlConnection con)
        {
            try
            {
                server = ConfigurationManager.AppSettings["mySqlServer"].ToString();
                database = ConfigurationManager.AppSettings["mySqlDB"].ToString();
                uid = ConfigurationManager.AppSettings["mySqlUID"].ToString();
                password = ConfigurationManager.AppSettings["mySqlPWD"].ToString();
                port = ConfigurationManager.AppSettings["mySqlPort"].ToString();
                string connectionString;
                connectionString = "Server=" + server + "; Port=" + port + "; Database=" +
                                    database + "; Uid=" + uid + "; Pwd=" + password + ";";

                con = new MySqlConnection(connectionString);
                con.Open();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void Insert(string destinationNo, string message, string senderID, string CreatorID, MySqlConnection con)
        {
            int lengthNo = destinationNo.Length;
            string destinationNo1 = "+628" + destinationNo.Substring(2).Replace(" ", "");

            MySqlCommand com = new MySqlCommand(
                "INSERT INTO `outbox` (DestinationNumber, TextDecoded, CreatorID) " +
                "VALUES('" + destinationNo1 + "', '" + message + "', 'POINT')", con);

            com.ExecuteNonQuery();
        }

        public bool CloseConnection(ref MySqlConnection con)
        {
            try
            {
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }



        public void Select(MySqlConnection con)
        {
            string nomor, text;

            MySqlCommand com = new MySqlCommand("SELECT * FROM `outbox`", con);

            MySqlDataReader dr = com.ExecuteReader();
            while (dr.Read())
            {
                nomor = dr["DestinationNumber"].ToString();
                text = dr["TextDecoded"].ToString();
            }
        }
    }
}