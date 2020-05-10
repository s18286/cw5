using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_5.Services
{
    public class SqlServerDbService : IStudentsDbService
    {

        private string connectionString = "Data Source = db - mssql; Initial Catalog = s18282; Integrated Security = True";
        private SqlConnection connection;
        
        public SqlServerDbService()
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        public void ExecuteInsert(SqlCommand command)
        {
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                command.Connection = connection;
                command.Transaction = transaction;
                command.ExecuteScalar();
            }
            catch (SqlException)
            {
                transaction.Rollback();
            }
            
        }

        public List<object[]> ExecuteSelect(SqlCommand command)
        {
            List<object[]> result = new List<object[]>();
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                command.Connection = connection;
                command.Transaction = transaction;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    object[] newObject = new object[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        newObject[i] = reader[i];
                    }
                    result.Add(newObject);
                }
                reader.Close();
            }
            catch (SqlException)
            {
                transaction.Rollback();
            }
            connection.Close();
            return result;
        }

        public SqlConnection GetConnection()
        {
            return connection;
        }
    }
}
