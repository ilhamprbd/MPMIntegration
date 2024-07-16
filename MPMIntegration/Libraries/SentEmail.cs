
using MPMIntegration.APIModel;
using MPMIntegration.Libraries;
using MPMIntegration.Repos;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;


namespace MPMIntegration.Libraries
{
    class SentEmail
    {

        public string ParticipantRecon()
        {

            string connectionString = ConfigurationManager.ConnectionStrings["AJPCore"].ConnectionString;
            string strPathFile = ConfigurationManager.AppSettings["DocsRecon"];

            try
            {

                // Create a SqlConnection
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    connection.InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);
                    // Create a SqlCommand for the stored procedure
                    using (SqlCommand command = new SqlCommand("[SP_EMAIL_NEW_PLACING_MPM]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;


                        // Add parameters
                        command.Parameters.AddWithValue("@FilePathCSV", strPathFile);

                        // Execute the stored procedure
                        command.ExecuteNonQuery();

                    }
                }

                Console.WriteLine("Sent Email to user: " + strPathFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending Email " + ex.Message);
            }

            return strPathFile;
        }

        private static void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            // Print each message to the console
            foreach (SqlError error in e.Errors)
            {
                Console.WriteLine("Message from SQL Server: " + error.Message);
            }
        }

    }


}