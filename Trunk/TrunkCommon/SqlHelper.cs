using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Data.OleDb;
using System.Xml;
using System.Collections;

namespace TrunkCommon
{
    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of SqlClient
    /// </summary>
    public sealed class SqlHelper
    {
        #region private utility methods & constructors

        // Since this class provides only static methods, make the default constructor private to prevent 
        // instances from being created with "new SqlHelper()"
        private SqlHelper() { }

        /// <summary>
        /// This method is used to attach array of SqlParameters to a SqlCommand.
        /// 
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        /// 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (SqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if ((p.Direction == ParameterDirection.InputOutput ||
                          p.Direction == ParameterDirection.Input) &&
                          (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// This method assigns dataRow column values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                // Do nothing if we get no data
                return;
            }

            int i = 0;
            // Set the parameters values
            foreach (SqlParameter commandParameter in commandParameters)
            {
                // Check the parameter name
                if (commandParameter.ParameterName == null ||
                  commandParameter.ParameterName.Length <= 1)
                    throw new Exception(
                      string.Format(
                        "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                        i, commandParameter.ParameterName));
                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                i++;
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                // Do nothing if we get no data
                return;
            }

            // We must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            // Iterate through the SqlParameters, assigning the values from the corresponding position in the 
            // value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                // If the current array value derives from IDbDataParameter, then assign its Value property
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                    if (paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command
        /// </summary>
        /// <param name="command">The SqlCommand to be prepared</param>
        /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        #endregion private utility methods & constructors

        #region ExecuteNonQuery

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(transaction, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery

        #region ExecuteDataset

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }

        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion ExecuteDataset

        #region ExecuteReader

        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum SqlConnectionOwnership
        {
            /// <summary>Connection is owned and managed by SqlHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

        /// <summary>
        /// Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// 
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
        /// <returns>SqlDataReader containing the results of the command</returns>
        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                // Create a reader
                SqlDataReader dataReader;

                // Call ExecuteReader with the appropriate CommandBehavior
                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }

                // Detach the SqlParameters from the command object, so they can be used again.
                // HACK: There is a problem here, the output parameter values are fletched 
                // when the reader is closed, so if the parameters are detached from the command
                // then the SqlReader can磘 set its values. 
                // When this happen, the parameters can磘 be used again in other command.
                bool canClear = true;
                foreach (SqlParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                        canClear = false;
                }

                if (canClear)
                {
                    cmd.Parameters.Clear();
                }

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                    connection.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteReader(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                // Call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
            }
            catch
            {
                // If we fail to return the SqlDatReader, we need to close the connection ourselves
                if (connection != null) connection.Close();
                throw;
            }

        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            // Pass through the call to the private overload using a null transaction value and an externally owned connection
            return ExecuteReader(connection, (SqlTransaction)null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteReader(transaction, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Pass through to private overload, indicating that the connection is owned by the caller
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteReader

        #region ExecuteScalar

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteScalar(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();

            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(transaction, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // PPull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar

        #region ExecuteXmlReader
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteXmlReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

                // Create the DataAdapter & DataSet
                XmlReader retval = cmd.ExecuteXmlReader();

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                return retval;
            }
            catch
            {
                if (mustCloseConnection)
                    connection.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteXmlReader(transaction, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            XmlReader retval = cmd.ExecuteXmlReader();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteXmlReader

        #region FillDataset
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                FillDataset(connection, commandType, commandText, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        public static void FillDataset(string connectionString, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public static void FillDataset(string connectionString, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                FillDataset(connection, spName, dataSet, tableNames, parameterValues);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        public static void FillDataset(SqlConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        public static void FillDataset(SqlConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params SqlParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public static void FillDataset(SqlConnection connection, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType,
            string commandText,
            DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params SqlParameter[] commandParameters)
        {
            FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public static void FillDataset(SqlTransaction transaction, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType,
          string commandText, DataSet dataSet, string[] tableNames,
          params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            // Create a command and prepare it for execution
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
            {

                // Add the table mappings specified by the user
                if (tableNames != null && tableNames.Length > 0)
                {
                    string tableName = "Table";
                    for (int index = 0; index < tableNames.Length; index++)
                    {
                        if (tableNames[index] == null || tableNames[index].Length == 0) throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        dataAdapter.TableMappings.Add((index > 0) ? (tableName + index.ToString()) : tableName, tableNames[index]);
                    }
                }

                // Fill the DataSet using default values for DataTable names, etc
                dataAdapter.Fill(dataSet);

                // Detach the SqlParameters from the command object, so they can be used again
                command.Parameters.Clear();
            }

            if (mustCloseConnection)
                connection.Close();
        }
        #endregion

        #region UpdateDataset
        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
        /// <param name="dataSet">The DataSet used to update the data source</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null) throw new ArgumentNullException("insertCommand");
            if (deleteCommand == null) throw new ArgumentNullException("deleteCommand");
            if (updateCommand == null) throw new ArgumentNullException("updateCommand");
            if (tableName == null || tableName.Length == 0) throw new ArgumentNullException("tableName");

            // Create a SqlDataAdapter, and dispose of it after we are done
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
            {
                // Set the data adapter commands
                dataAdapter.UpdateCommand = updateCommand;
                dataAdapter.InsertCommand = insertCommand;
                dataAdapter.DeleteCommand = deleteCommand;

                // Update the dataset changes in the data source
                dataAdapter.Update(dataSet, tableName);

                // Commit all the changes made to the DataSet
                dataSet.AcceptChanges();
            }
        }
        #endregion
    }

    /// <summary>
    /// 参数化SQL语句 v4.3<para/>
    /// 注意：该类不可序列化。不再使用时请调用Dispose方法！
    /// </summary>
    public class XSQL : IDisposable
    {
        #region 执行SQL
        /// <summary>
        /// 执行参数化SQL语句
        /// </summary>
        /// <returns>所影响行数</returns>
        public int ExecuteNonQuery()
        {
            if (this.Command.Connection.State == ConnectionState.Closed)
            {
                this.Command.Connection.Open();
            }
            return this.Command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行参数化SQL语句
        /// </summary>
        /// <returns>查询结果数据表</returns>
        public DataTable ExecuteDataTable()
        {
            SqlDataAdapter ada = null;
            DataTable result = new DataTable();
            try
            {
                if (this.Command.Connection.State == ConnectionState.Closed)
                {
                    this.Command.Connection.Open();
                }
                ada = new SqlDataAdapter(this.Command);
                ada.Fill(result);
                return result;
            }
            finally
            {
                if (ada != null)
                    ada.Dispose();
            }
        }

        /// <summary>
        /// 执行参数化SQL语句
        /// </summary>
        /// <returns>查询结果数据集</returns>
        public DataSet ExecuteDataSet()
        {
            SqlDataAdapter ada = null;
            DataSet result = new DataSet();
            try
            {
                if (this.Command.Connection.State == ConnectionState.Closed)
                {
                    this.Command.Connection.Open();
                }
                ada = new SqlDataAdapter(this.Command);
                ada.Fill(result);
                return result;
            }
            finally
            {
                if (ada != null)
                    ada.Dispose();
            }
        }

        /// <summary>
        /// 执行参数化SQL语句
        /// </summary>
        /// <returns>查询结果对象</returns>
        public object ExecuteScalar()
        {
            if (this.Command.Connection.State == ConnectionState.Closed)
            {
                this.Command.Connection.Open();
            }
            return this.Command.ExecuteScalar();
        }

        /// <summary>
        /// 执行参数化SQL语句
        /// </summary>
        /// <returns>查询结果数据读取器</returns>
        public SqlDataReader ExecuteReader()
        {
            SqlDataReader reader = null;
            if (this.Command.Connection.State == ConnectionState.Closed)
            {
                this.Command.Connection.Open();
            }
            reader = this.Command.ExecuteReader();
            return reader;
        }
        #endregion

        #region 构造
        public XSQL(string connStr)
        {
            this.Init(new SqlConnection(connStr), 60, IsolationLevel.ReadUncommitted, true);
        }
        public XSQL(SqlConnection conn)
        {
            this.Init(conn, 60, IsolationLevel.ReadUncommitted, true);
        }
        public XSQL(SqlConnection conn, params string[] parameterizeSQL)
        {
            this.Init(conn, 60, IsolationLevel.ReadUncommitted, true, parameterizeSQL);
        }
        public XSQL(SqlConnection conn, int execTimeout, IsolationLevel isolation, bool isNoCount, params string[] parameterizeSQL)
        {
            this.Init(conn, execTimeout, isolation, isNoCount, parameterizeSQL);
        }

        private void Init(SqlConnection conn, int execTimeout, IsolationLevel isolation, bool isNoCount, params string[] parameterizeSQL)
        {
            this.Command = new SqlCommand(string.Concat(parameterizeSQL));
            this.Command.Connection = conn;
            this.Command.CommandTimeout = execTimeout;
            this.Isolation = isolation;
            this.IsNoCount = isNoCount;
        }
        #endregion
        /// <summary>
        /// 获取或设置参数化SQL语句<para/>
        /// 参数请使用@PararmName的形式占位，字符串或日期等类型参数也无需单引号
        /// </summary>
        public string ParameterizeSQL
        {
            get { return this.Command.CommandText; }
            set { this.Command.CommandText = value; }
        }

        #region 添加参数化语句
        public void AppendSelectSQL(string fields, string tableName, string filter, string sort)
        {
            if (string.IsNullOrEmpty(fields))
                fields = "*";
            if (!string.IsNullOrEmpty(filter))
                filter = " where " + filter;
            if (!string.IsNullOrEmpty(sort))
                sort = " order by " + sort;
            this.AppendParameterizeSQL(string.Concat("select ", fields, " from ", tableName, filter, sort, ";"));
        }
        public void AppendUpdateSQL(string tableName, string setValue, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
                filter = " where " + filter;
            this.AppendParameterizeSQL(string.Concat("update ", tableName, " set ", setValue, filter, ";"));
        }
        public void AppendInsertSQL(string tableName, string values)
        {
            this.AppendParameterizeSQL(string.Concat("insert into ", tableName, " values(", values, ");"));
        }
        public void AppendDeleteSQL(string tableName, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
                filter = " where " + filter;
            this.AppendParameterizeSQL(string.Concat("delete from ", tableName, filter, ";"));
        }


        public void AppendParameterizeSQL(string parameterizeSQL)
        {
            if (string.IsNullOrEmpty(parameterizeSQL))
                return;
            this.Command.CommandText += parameterizeSQL;
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, byte value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, short value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, int value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, long value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, float value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, double value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, decimal value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, bool value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, char value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, DateTime value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, Guid value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        public void AppendParameterizeSQL(string parameterizeSQL, string parameterName, string value)
        {
            this.AppendParameterizeSQL(parameterizeSQL);
            this.AddParameter(parameterName, value);
        }
        #endregion

        #region 添加参数
        public void AddParameter(string parameterName, byte value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.TinyInt, 1).Value = value;
        }
        public void AddParameter(string parameterName, short value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.SmallInt, 2).Value = value;
        }
        public void AddParameter(string parameterName, int value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.Int, 4).Value = value;
        }
        public void AddParameter(string parameterName, long value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.BigInt, 1).Value = value;
        }
        public void AddParameter(string parameterName, float value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.Real, 4).Value = value;
        }
        public void AddParameter(string parameterName, double value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.Float, 8).Value = value;
        }
        public void AddParameter(string parameterName, decimal value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.Decimal, 9).Value = value;
        }
        public void AddParameter(string parameterName, bool value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.Bit, 1).Value = value;
        }
        public void AddParameter(string parameterName, char value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.Char, value < 0xFF ? 1 : 2).Value = value;
        }
        public void AddParameter(string parameterName, DateTime value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.DateTime, 8).Value = value;
        }
        public void AddParameter(string parameterName, Guid value)
        {
            this.Command.Parameters.Add(parameterName, SqlDbType.UniqueIdentifier, 16).Value = value;
        }
        public void AddParameter(string parameterName, string value)
        {
            if (value == null)
                value = string.Empty;
            int length = value.Length;
            length = length > 4000 ? -1 : length;
            this.Command.Parameters.Add(parameterName, SqlDbType.NVarChar, length).Value = value;
        }
        #endregion

        /// <summary>
        /// 获取模拟执行的SQL语句,仅用于测试(注意与实际不同于此SQL参数名区分大小写)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(this.Command.CommandText);
            foreach (SqlParameter p in this.Command.Parameters)
            {
                if (p.Direction == ParameterDirection.Input)
                {
                    if (p.SqlDbType == SqlDbType.Char
                        || p.SqlDbType == SqlDbType.NChar
                        || p.SqlDbType == SqlDbType.NVarChar
                        || p.SqlDbType == SqlDbType.VarChar
                        || p.SqlDbType == SqlDbType.UniqueIdentifier)
                        sb = sb.Replace(p.ParameterName, "'" + p.Value.ToString() + "'");
                    else if (p.SqlDbType == SqlDbType.DateTime
                        || p.SqlDbType == SqlDbType.SmallDateTime)
                        sb = sb.Replace(p.ParameterName, "'" + ((DateTime)p.Value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'");
                    else
                        sb = sb.Replace(p.ParameterName, p.Value.ToString());
                }
            }
            return sb.ToString();
        }

        private SqlCommand _Command = null;
        /// <summary>
        /// 获取XSQL内部SqlCommand命令对象
        /// </summary>
        public SqlCommand Command
        {
            set { this._Command = value; }
            get { return this._Command; }
        }
        /// <summary>
        /// 等待命令执行的时间（以秒为单位）。默认值为 30 秒。
        /// </summary>
        public int Timeout
        {
            get { return this.Command.CommandTimeout; }
            set { this.Command.CommandTimeout = value; }
        }

        private IsolationLevel _Isolation = IsolationLevel.ReadUncommitted;
        /// <summary>
        /// 设置事务的隔离界别（默认ReadUncommitted允许脏读）
        /// </summary>
        public IsolationLevel Isolation
        {
            get { return this._Isolation; }
            set { this._Isolation = value; }
        }

        private bool _IsNoCount = true;
        /// <summary>
        /// 设置是否禁用返回影响行数（true可提高效率，默认true）
        /// </summary>
        public bool IsNoCount
        {
            get { return this._IsNoCount; }
            set { this._IsNoCount = value; }
        }

        /// <summary>
        /// 根据参数自动生成ParameterizeSQL分页TSQL语句，(注意，除第一页外，从第二页或第二页以上返回字段中包含一列绝对行号RowNo)
        /// </summary>
        /// <param name="tableName">要查询的表、视图、表连接的名称</param>
        /// <param name="fields">要查询的字段</param>
        /// <param name="orderby">排序规则,如:"CreateDate desc,PRI asc"</param>
        /// <param name="filter">参数化条件语句(没有填null)</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="groupby">分组语句列名,无需分组请填null(注意：分组设置列必须包含查询列的所有字段，或者必须在查询列上使用相应的聚合函数)</param>
        /// <param name="pageIndex">索取数据所在页号(以1开始,小于1则以1代替。如果页码大于数据的最大页数则以最大页数代替</param>
        public void Pager_SetQuery(string tableName, string fields, string orderby, string filter, string groupby, int pageSize, int pageIndex)
        {
            StringBuilder sb = new StringBuilder(1024);
            if (string.IsNullOrEmpty(groupby))
            {
                sb.Append("select @totalCount");
                sb.Append("=count(*) from ");
                sb.Append(tableName);
                if (!string.IsNullOrEmpty(filter))
                {
                    sb.Append(" where ");
                    sb.Append(filter);
                }
                sb.Append(";");
            }
            else
            {
                sb.Append("select @totalCount");
                sb.Append("=count(*) from (select 0 nums from ");
                sb.Append(tableName);
                if (!string.IsNullOrEmpty(filter))
                {
                    sb.Append(" where ");
                    sb.Append(filter);
                }
                if (!string.IsNullOrEmpty(groupby))
                {
                    sb.Append(" group by ");
                    sb.Append(groupby);
                }
                sb.Append(") as xsql_tmp;");
            }
            if (pageIndex <= 1)
            {
                sb.Append("select @pageIndex=1;select top ");
                sb.Append(pageSize);
                sb.Append(" ");
                sb.Append(fields);
                sb.Append(" from ");
                sb.Append(tableName);
                if (!string.IsNullOrEmpty(filter))
                {
                    sb.Append(" where ");
                    sb.Append(filter);
                }
                if (!string.IsNullOrEmpty(groupby))
                {
                    sb.Append(" group by ");
                    sb.Append(groupby);
                }
                sb.Append(" order by ");
                sb.Append(orderby);
            }
            else
            {
                sb.Append("select @pageIndex=case when @totalCount/");
                sb.Append(pageSize);
                sb.Append(">=");
                sb.Append(pageIndex - 1);
                sb.Append(" then ");
                sb.Append(pageIndex);
                sb.Append(" else @totalCount/");
                sb.Append(pageSize);
                sb.Append("+1 end;");
                sb.Append("select * from (select ");
                sb.Append(fields);
                sb.Append(",row_number() over(order by ");
                sb.Append(orderby);
                sb.Append(") as RowNo from ");
                sb.Append(tableName);
                if (!string.IsNullOrEmpty(filter))
                {
                    sb.Append(" where ");
                    sb.Append(filter);
                }
                if (!string.IsNullOrEmpty(groupby))
                {
                    sb.Append(" group by ");
                    sb.Append(groupby);
                }
                sb.Append(") as t where RowNo between ");
                sb.Append(pageSize);
                sb.Append("*(@pageIndex-1)+1");
                sb.Append(" and ");
                sb.Append(pageSize);
                sb.Append("*@pageIndex");
            }
            this.ParameterizeSQL = sb.ToString();
            this.Command.Parameters.Add("@totalCount", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
            this.Command.Parameters.Add("@pageIndex", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
        }

        /// <summary>
        /// 获取分页查询后的真实页码
        /// </summary>
        public int Pager_RealPageIndex
        {
            get { return (int)this.Command.Parameters["@pageIndex"].Value; }
        }

        /// <summary>
        /// 获取分页查询后的结果条数
        /// </summary>
        public int Pager_TotalCount
        {
            get { return (int)this.Command.Parameters["@totalCount"].Value; }
        }

        /// <summary>
        /// 清空参数化语句和参数
        /// </summary>
        public void Clear()
        {
            this.Command.CommandText = string.Empty;
            this.Command.Parameters.Clear();
        }

        #region IDisposable 成员
        /// <summary>
        /// 释放所用系统资源
        /// </summary>
        public void Dispose()
        {
            if (this.Command != null)
            {
                if (this.Command.Connection != null && this.Command.Connection.State != ConnectionState.Closed)
                {
                    this.Command.Connection.Close();
                }
                this.Command.Dispose();
            }
        }
        #endregion
    }//end class



    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelperParameterCache()"
        private SqlHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            SqlCommand cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            SqlCommandBuilder.DeriveParameters(cmd);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (SqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached SqlParameter array
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of SqlParamters</returns>
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            SqlParameter[] cachedParameters = paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            using (SqlConnection clonedConnection = (SqlConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            SqlParameter[] cachedParameters;

            cachedParameters = paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                SqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }

        #endregion Parameter Discovery Functions
    }

    ///  
    /// The OleDbHelper class is intended to encapsulate high performance, scalable best practices for  
    /// common uses of OleDbClient. 
    /// 
    ///  
    public sealed class OleDbHelper
    {
        #region private utility methods & constructors

        //Since this class provides only static methods, make the default constructor private to prevent  
        //instances from being created with "new OleDbHelper()". 
        private OleDbHelper() { }

        ///  
        /// This method is used to attach array's of OleDbParameters to an OleDbCommand. 
        ///  
        /// This method will assign a value of DbNull to any parameter with a direction of 
        /// InputOutput and a value of null.   
        ///  
        /// This behavior will prevent default values from being used, but 
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput) 
        /// where the user provided no input value. 
        ///  
        /// The command to which the parameters will be added 
        /// an array of OleDbParameters tho be added to command 
        private static void AttachParameters(OleDbCommand command, OleDbParameter[] commandParameters)
        {
            foreach (OleDbParameter p in commandParameters)
            {
                //check for derived output value with no value assigned 
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        ///  
        /// This method assigns an array of values to an array of OleDbParameters. 
        ///  
        /// array of OleDbParameters to be assigned values 
        /// array of objects holding the values to be assigned 
        private static void AssignParameterValues(OleDbParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                //do nothing if we get no data 
                return;
            }

            // we must have the same number of values as we pave parameters to put them in 
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            //iterate through the OleDbParameters, assigning the values from the corresponding position in the  
            //value array 
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }

        ///  
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters  
        /// to the provided command. 
        ///  
        /// the OleDbCommand to be prepared 
        /// a valid OleDbConnection, on which to execute this command 
        /// a valid OleDbTransaction, or 'null' 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// an array of OleDbParameters to be associated with the command or 'null' if no parameters are required 
        private static void PrepareCommand(OleDbCommand command, OleDbConnection connection, OleDbTransaction transaction, CommandType commandType, string commandText, OleDbParameter[] commandParameters)
        {
            //if the provided connection is not open, we will open it 
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //associate the connection with the command 
            command.Connection = connection;

            //set the command text (stored procedure name or OleDb statement) 
            command.CommandText = commandText;

            //if we were provided a transaction, assign it. 
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            //set the command type 
            command.CommandType = commandType;

            //attach the command parameters if they are provided 
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            return;
        }


        #endregion private utility methods & constructors

        #region ExecuteNonQuery

        ///  
        /// Execute an OleDbCommand (that returns no resultset and takes no parameters) against the database specified in  
        /// the connection string.  
        ///  
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders"); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteNonQuery(connectionString, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns no resultset) against the database specified in the connection string  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an array of OleDbParameters used to execute the command 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create & open an OleDbConnection, and dispose of it after we are done. 
            using (OleDbConnection cn = new OleDbConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string 
                return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
            }
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns no resultset) against the database specified in  
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36); 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute an OleDbDbCommand (that returns no resultset and takes no parameters) against the provided OleDbConnection.  
        ///  
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders"); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(OleDbConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteNonQuery(connection, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns no resultset) against the specified OleDbConnection  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an array of OleDbParamters used to execute the command 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(OleDbConnection connection, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, connection, (OleDbTransaction)null, commandType, commandText, commandParameters);

            //finally, execute the command. 
            return cmd.ExecuteNonQuery();
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns no resultset) against the specified OleDbConnection  
        /// using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36); 
        ///  
        /// a valid OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(OleDbConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute an OleDbCommand (that returns no resultset and takes no parameters) against the provided OleDbTransaction.  
        ///  
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders"); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(OleDbTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteNonQuery(transaction, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns no resultset) against the specified OleDbTransaction 
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an array of OleDbParamters used to execute the command 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(OleDbTransaction transaction, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            //finally, execute the command. 
            return cmd.ExecuteNonQuery();
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns no resultset) against the specified  
        /// OleDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36); 
        ///  
        /// a valid OleDbTransaction 
        /// the name of the stored procedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an int representing the number of rows affected by the command 
        public static int ExecuteNonQuery(OleDbTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery

        #region ExecuteDataSet

        ///  
        /// Execute an OleDbCommand (that returns a resultset and takes no parameters) against the database specified in  
        /// the connection string.  
        ///  
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders"); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteDataset(connectionString, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset) against the database specified in the connection string  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// an array of OleDbParamters used to execute the command 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create & open an OleDbConnection, and dispose of it after we are done. 
            using (OleDbConnection cn = new OleDbConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string 
                return ExecuteDataset(cn, commandType, commandText, commandParameters);
            }
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns a resultset) against the database specified in  
        /// the conneciton string using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the name of the stored procedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset and takes no parameters) against the provided OleDbConnection.  
        ///  
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders"); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(OleDbConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteDataset(connection, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset) against the specified OleDbConnection  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// an array of OleDbParamters used to execute the command 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(OleDbConnection connection, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, connection, (OleDbTransaction)null, commandType, commandText, commandParameters);

            //create the DataAdapter & DataSet 
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            DataSet ds = new DataSet();

            //fill the DataSet using default values for DataTable names, etc. 
            da.Fill(ds);

            //return the dataset 
            return ds;
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns a resultset) against the specified OleDbConnection  
        /// using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36); 
        ///  
        /// a valid OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(OleDbConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset and takes no parameters) against the provided OleDbTransaction.  
        ///  
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders"); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(OleDbTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteDataset(transaction, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset) against the specified OleDbTransaction 
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// an array of OleDbParamters used to execute the command 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(OleDbTransaction transaction, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            //create the DataAdapter & DataSet 
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            DataSet ds = new DataSet();

            //fill the DataSet using default values for DataTable names, etc. 
            da.Fill(ds);

            //return the dataset 
            return ds;
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns a resultset) against the specified  
        /// OleDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36); 
        ///  
        /// a valid OleDbTransaction 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// a dataset containing the resultset generated by the command 
        public static DataSet ExecuteDataset(OleDbTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataSet

        #region ExecuteReader

        ///  
        /// this enum is used to indicate weather the connection was provided by the caller, or created by OleDbHelper, so that 
        /// we can set the appropriate CommandBehavior when calling ExecuteReader() 
        ///  
        private enum OleDbConnectionOwnership
        {
            /// Connection is owned and managed by OleDbHelper 
            Internal,
            /// Connection is owned and managed by the caller 
            External
        }

        ///  
        /// Create and prepare an OleDbCommand, and call ExecuteReader with the appropriate CommandBehavior. 
        ///  
        ///  
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed. 
        ///  
        /// If the caller provided the connection, we want to leave it to them to manage. 
        ///  
        /// a valid OleDbConnection, on which to execute this command 
        /// a valid OleDbTransaction, or 'null' 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// an array of OleDbParameters to be associated with the command or 'null' if no parameters are required 
        /// indicates whether the connection parameter was provided by the caller, or created by OleDbHelper 
        /// OleDbDataReader containing the results of the command 
        private static OleDbDataReader ExecuteReader(OleDbConnection connection, OleDbTransaction transaction, CommandType commandType, string commandText, OleDbParameter[] commandParameters, OleDbConnectionOwnership connectionOwnership)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

            //create a reader 
            OleDbDataReader dr;

            // call ExecuteReader with the appropriate CommandBehavior 
            if (connectionOwnership == OleDbConnectionOwnership.External)
            {
                dr = cmd.ExecuteReader();
            }
            else
            {
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }

            return dr;
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset and takes no parameters) against the database specified in  
        /// the connection string.  
        ///  
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders"); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteReader(connectionString, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset) against the database specified in the connection string  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an array of OleDbParameters used to execute the command 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create & open an OleDbbConnection 
            OleDbConnection cn = new OleDbConnection(connectionString);
            cn.Open();

            try
            {
                //call the private overload that takes an internally owned connection in place of the connection string 
                return ExecuteReader(cn, null, commandType, commandText, commandParameters, OleDbConnectionOwnership.Internal);
            }
            catch
            {
                //if we fail to return the OleDbDataReader, we neeed to close the connection ourselves 
                cn.Close();
                throw;
            }
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns a resultset) against the database specified in  
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36); 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset and takes no parameters) against the provided OleDbConnection.  
        ///  
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders"); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(OleDbConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteReader(connection, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset) against the specified OleDbConnection  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an array of OleDbParamters used to execute the command 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(OleDbConnection connection, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //pass through the call to the private overload using a null transaction value and an externally owned connection 
            return ExecuteReader(connection, (OleDbTransaction)null, commandType, commandText, commandParameters, OleDbConnectionOwnership.External);
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns a resultset) against the specified OleDbConnection  
        /// using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36); 
        ///  
        /// a valid OleDbConnection 
        /// the name of the stored procedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(OleDbConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset and takes no parameters) against the provided OleDbTransaction.  
        ///  
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders"); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command   
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(OleDbTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteReader(transaction, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute an OleDbCommand (that returns a resultset) against the specified OleDbTransaction 
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///   OleDbDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or PL/SQL command  
        /// an array of OleDbParameters used to execute the command 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(OleDbTransaction transaction, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //pass through to private overload, indicating that the connection is owned by the caller 
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, OleDbConnectionOwnership.External);
        }

        ///  
        /// Execute a stored procedure via an OleDbCommand (that returns a resultset) against the specified 
        /// OleDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  OleDbDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36); 
        ///  
        /// a valid OleDbTransaction 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an OleDbDataReader containing the resultset generated by the command 
        public static OleDbDataReader ExecuteReader(OleDbTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteReader

        #region ExecuteScalar

        ///  
        /// Execute a OleDbCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in  
        /// the connection string.  
        ///  
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount"); 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or T-OleDb command 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteScalar(connectionString, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute a OleDbCommand (that returns a 1x1 resultset) against the database specified in the connection string  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or T-OleDb command 
        /// an array of OleDbParamters used to execute the command 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create & open a OleDbConnection, and dispose of it after we are done. 
            using (OleDbConnection cn = new OleDbConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string 
                return ExecuteScalar(cn, commandType, commandText, commandParameters);
            }
        }

        ///  
        /// Execute a stored procedure via a OleDbCommand (that returns a 1x1 resultset) against the database specified in  
        /// the conneciton string using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36); 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute a OleDbCommand (that returns a 1x1 resultset and takes no parameters) against the provided OleDbConnection.  
        ///  
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount"); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or T-OleDb command 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(OleDbConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteScalar(connection, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute a OleDbCommand (that returns a 1x1 resultset) against the specified OleDbConnection  
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbConnection 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or T-OleDb command 
        /// an array of OleDbParamters used to execute the command 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(OleDbConnection connection, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, connection, (OleDbTransaction)null, commandType, commandText, commandParameters);

            //execute the command & return the results 
            return cmd.ExecuteScalar();
        }

        ///  
        /// Execute a stored procedure via a OleDbCommand (that returns a 1x1 resultset) against the specified OleDbConnection  
        /// using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36); 
        ///  
        /// a valid OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(OleDbConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        ///  
        /// Execute a OleDbCommand (that returns a 1x1 resultset and takes no parameters) against the provided OleDbTransaction.  
        ///  
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount"); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or T-OleDb command 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(OleDbTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OleDbParameters 
            return ExecuteScalar(transaction, commandType, commandText, (OleDbParameter[])null);
        }

        ///  
        /// Execute a OleDbCommand (that returns a 1x1 resultset) against the specified OleDbTransaction 
        /// using the provided parameters. 
        ///  
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new OleDbParameter("@prodid", 24)); 
        ///  
        /// a valid OleDbTransaction 
        /// the CommandType (stored procedure, text, etc.) 
        /// the stored procedure name or T-OleDb command 
        /// an array of OleDbParamters used to execute the command 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(OleDbTransaction transaction, CommandType commandType, string commandText, params OleDbParameter[] commandParameters)
        {
            //create a command and prepare it for execution 
            OleDbCommand cmd = new OleDbCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            //execute the command & return the results 
            return cmd.ExecuteScalar();

        }

        ///  
        /// Execute a stored procedure via a OleDbCommand (that returns a 1x1 resultset) against the specified 
        /// OleDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the  
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order. 
        ///  
        ///  
        /// This method provides no access to output parameters or the stored procedure's return value parameter. 
        ///  
        /// e.g.:   
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36); 
        ///  
        /// a valid OleDbTransaction 
        /// the name of the stored prcedure 
        /// an array of objects to be assigned as the input values of the stored procedure 
        /// an object containing the value in the 1x1 resultset generated by the command 
        public static object ExecuteScalar(OleDbTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go 
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache) 
                OleDbParameter[] commandParameters = OleDbHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order 
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OleDbParameters 
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params 
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar
    }

    ///  
    /// OleDbHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the 
    /// ability to discover parameters for stored procedures at run-time. 
    ///  
    public sealed class OleDbHelperParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent  
        //instances from being created with "new OleDbHelperParameterCache()". 
        private OleDbHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        ///  
        /// resolve at run-time the appropriate set of OleDbParameters for a stored procedure 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the name of the stored prcedure 
        /// weather or not to onclude ther return value parameter 
        ///  
        private static OleDbParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            using (OleDbConnection cn = new OleDbConnection(connectionString))
            using (OleDbCommand cmd = new OleDbCommand(spName, cn))
            {
                cn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                OleDbCommandBuilder.DeriveParameters(cmd);

                if (!includeReturnValueParameter)
                {
                    if (ParameterDirection.ReturnValue == cmd.Parameters[0].Direction)
                        cmd.Parameters.RemoveAt(0);
                }

                OleDbParameter[] discoveredParameters = new OleDbParameter[cmd.Parameters.Count];

                cmd.Parameters.CopyTo(discoveredParameters, 0);

                return discoveredParameters;
            }
        }

        //deep copy of cached OleDbParameter array 
        private static OleDbParameter[] CloneParameters(OleDbParameter[] originalParameters)
        {
            OleDbParameter[] clonedParameters = new OleDbParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (OleDbParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        ///  
        /// add parameter array to the cache 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the stored procedure name or T-OleDb command 
        /// an array of OleDbParamters to be cached 
        public static void CacheParameterSet(string connectionString, string commandText, params OleDbParameter[] commandParameters)
        {
            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        ///  
        /// retrieve a parameter array from the cache 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the stored procedure name or T-OleDb command 
        /// an array of OleDbParameters 
        public static OleDbParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            string hashKey = connectionString + ":" + commandText;

            OleDbParameter[] cachedParameters = (OleDbParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        ///  
        /// Retrieves the set of OleDbParameters appropriate for the stored procedure 
        ///  
        ///  
        /// This method will query the database for this information, and then store it in a cache for future requests. 
        ///  
        /// a valid connection string for a OleDbConnection 
        /// the name of the stored prcedure 
        /// an array of OleDbParameters 
        public static OleDbParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, true);
        }

        ///  
        /// Retrieves the set of OleDbParameters appropriate for the stored procedure 
        ///  
        ///  
        /// This method will query the database for this information, and then store it in a cache for future requests. 
        ///  
        /// a valid connection string for an OleDbConnection 
        /// the name of the stored procedure 
        /// a bool value indicating weather the return value parameter should be included in the results 
        /// an array of OleDbParameters 
        public static OleDbParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            OleDbParameter[] cachedParameters;

            cachedParameters = (OleDbParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                cachedParameters = (OleDbParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
            }

            return CloneParameters(cachedParameters);
        }

        #endregion Parameter Discovery Functions

    }
}
