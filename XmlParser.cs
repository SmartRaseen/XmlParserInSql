using System;
using System.Xml;
using System.Data.SqlClient;
using System.IO;

namespace XmlParseInSql
{
    class XmlParser
    {
        public static SqlConnection connection;

        private readonly XmlConfig _config;
        
        public XmlParser()
        {
            _config = new XmlConfig();
            var files = Directory.GetFiles(_config.InputDirectory, "*.xml", SearchOption.AllDirectories);
            connection = new SqlConnection(_config.Connection);

            try
            {
                connection.Open();

                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        GetXmlData(file);
                    }

                    else
                        Console.WriteLine("{0} is not a valid file or directory.", file);
                }
            }

            finally
            {
                connection.Close();
            }
        }

        internal int CheckEmpHistoryIDExists(int JobId)
        {
            int id = 0;
            try
            {
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = "select EmpHistoryId from Job Where JobID =  @JobId  ";
                    sqlCommand.Parameters.AddWithValue("@JobId",JobId);
                    using (SqlDataReader dr = sqlCommand.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Console.WriteLine(" Data Already Exists...");
                            id = Convert.ToInt32(dr["EmpHistoryId"]);
                        }
                        else
                        {
                            Console.WriteLine(" Data Doesn't Exists...");
                            return 0;
                        }
                    }
                }
            }
            catch(Exception Ex)
            {
                Console.WriteLine("Error in getting EmployeeHistory ID : {0}",Ex.Message);
            }
            return id;
        }

        internal void GetXmlData(string file)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(file);

                string JobData = xmlDocument.SelectSingleNode("//jobid").InnerText;
                string JobTitle = xmlDocument.SelectSingleNode("//jobtitle").InnerText;
                string Employer = xmlDocument.SelectSingleNode("//employer").InnerText;
                string Description = xmlDocument.SelectSingleNode("//description").InnerText;
                string from = xmlDocument.SelectSingleNode("//from").InnerText;
                string to = xmlDocument.SelectSingleNode("//to").InnerText;

                var JobID = Convert.ToInt32(JobData);
                var EmpHistoryID = CheckEmpHistoryIDExists(JobID);

                if (EmpHistoryID != 0 && JobID != 0)
                {
                    using (SqlCommand sqlCommand = connection.CreateCommand())
                    {
                        sqlCommand.CommandText = "UPDATE EmployeeHistory Set EmpFrom =  @from , EmpTo = @to  Where EmpHistoryID = @EmpHistoryID ";
                        sqlCommand.Parameters.AddWithValue("@from",from);
                        sqlCommand.Parameters.AddWithValue("@to",to);
                        sqlCommand.Parameters.AddWithValue("@EmpHistoryID",EmpHistoryID);
                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.CommandText = "UPDATE Job SET jobtitle = @JobTitle, employer = @Employer, description = @Description  WHERE jobid = @JobID ";
                        sqlCommand.Parameters.AddWithValue("@JobTitle", JobTitle);
                        sqlCommand.Parameters.AddWithValue("@Employer", Employer);
                        sqlCommand.Parameters.AddWithValue("@Description", Description);
                        sqlCommand.Parameters.AddWithValue("@JobID", JobID);
                        sqlCommand.ExecuteNonQuery();
                        Console.WriteLine("\n Updating EmployeeHistory Table is Completed..!");
                        Console.WriteLine("\n Updating Job Completed..!");
                    }
                }

                else
                {
                    var empHistoryId = ProcessEmpHistory(from, to);
                    ProcessJobData(JobID, JobTitle, Employer, Description, empHistoryId);
                }
            }

            catch (Exception Ex)
            {
                Console.WriteLine("Error In Getting Xml Data : {0}", Ex.Message);
            }
        }

        internal int ProcessEmpHistory(string from, string to)
        {
            int id = 0;
            try
            {
                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = "INSERT INTO EmployeeHistory (EmpFrom,EmpTo) VALUES ( @from, @to )";
                    sqlCommand.Parameters.AddWithValue("@from",from);
                    sqlCommand.Parameters.AddWithValue("@to",to);
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.CommandText = "SELECT MAX(EmpHistoryID) from EmployeeHistory";

                    using (SqlDataReader dr = sqlCommand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            id = dr.GetInt32(0);
                        }
                        dr.Close();
                    }
                }
            }

            catch(Exception Ex)
            {
                Console.WriteLine("Error in Processing EmployeeHistoryID : {0}",Ex.Message);
            }
            return id;
        }

        internal void ProcessJobData(int JobId, string JobTitle , string Employer , string Description, int empHistoryID)
        {
            try
            {

                using (SqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = "INSERT INTO Job (JobID,Jobtitle,Employer,Description,EmpHistoryID ) VALUES (@JobId, @JobTitle, @Employer, @Description, @empHistoryID)";
                    sqlCommand.Parameters.AddWithValue("@JobId", JobId);
                    sqlCommand.Parameters.AddWithValue("@JobTitle", JobTitle);
                    sqlCommand.Parameters.AddWithValue("@Employer", Employer);
                    sqlCommand.Parameters.AddWithValue("@Description", Description);
                    sqlCommand.Parameters.AddWithValue("@empHistoryID", empHistoryID);
                    sqlCommand.ExecuteNonQuery();
                }
            }

            catch(Exception Ex)
            {
                Console.WriteLine("Error in Processing Job Data {0} : ",Ex.Message);
            }
        }
    }
}
