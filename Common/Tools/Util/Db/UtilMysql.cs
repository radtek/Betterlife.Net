﻿//使用Mysql调用数据库
//需要下载ADO.NET Driver for MySQL (Connector/NET)：http://www.mysql.com/products/connector/
#define IS_USE_MYSQL 
//不使用Mysql调用数据库
//#undef IS_USE_MYSQL
#if IS_USE_MYSQL
using MySql.Data.MySqlClient;
#endif
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tools.Util.Db
{
    /// <summary>
    /// 工具类：MySQL数据库信息
    /// Insert, Update, Search and Delete (CRUD operation) using ASP.Net and MySQL
    ///     http://www.codeproject.com/Articles/438259/Insert-Update-Search-and-Delete-CRUD-operation-usi
    /// How to Connect to MySQL Using C#
    ///     http://www.codeproject.com/Tips/423233/How-to-Connect-to-MySQL-Using-Csharp
    /// 中文：C# 连接 MySQL 并进行数据库操作（入门篇）    
    ///     http://www.oschina.net/question/12_62083
    /// </summary>
    public class UtilMysql : UtilDb
    {
        /// <summary>
        /// 连接数据库字符串
        /// </summary>
        private static string ConnStr = "server=127.0.0.1;"+
                                        "user id=root;"+
                                        "password=;"+
                                        "persist security info=True;"+
                                        "database={0};";
        public static string Database_Name = "BetterlifeNet";
#region SQL定义
        /// <summary>
        /// 查看所有数据库
        /// </summary>
        private static string Sql_Databases = "SHOW DATABASES";
        /// <summary>
        /// 查看数据库所有的数据表
        /// </summary>
        private static string Sql_Tables = "SHOW TABLES";
        /// <summary>
        /// 查看数据库所有的数据表信息
        /// </summary>
        private static string Sql_Tables_Info = "show table status";
        /// <summary>
        /// 查看指定表所有的列
        /// </summary>
        private static string Sql_Table_Columns = "SHOW FULL FIELDS IN {0}";

        #endregion
#if IS_USE_MYSQL
        /// <summary>
        /// 数据库连接
        /// </summary>
        private static MySqlConnection myConnection;
#endif        
#region 基本操作
        /// <summary>
        /// 连接数据库
        /// </summary>
        private static void Connect()
        {
            string connString = string.Format(ConnStr, Database_Name);
#if IS_USE_MYSQL
            myConnection = new MySqlConnection(connString);
#endif
        }

        /// <summary>
        /// 执行sql查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable SqlExecute(string sql)
        {
            DataTable result = new DataTable();
#if IS_USE_MYSQL
            if (myConnection == null)Connect();
            try
            {
                using (MySqlCommand myCommand = myConnection.CreateCommand())
                {
                    myCommand.CommandText = sql;
                    myConnection.Open();
                    using (MySqlDataReader reader = myCommand.ExecuteReader())
                    {
                        result.Load(reader);
                        reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                String errorInfo = e.ToString();
                if (errorInfo.Contains("Unknown database"))
                {
                    MessageBox.Show("默认数据库不存在，请先在Mysql数据库中安装文件夹[Init_Db\\Mysql\\BetterlifeNet.sql]下的数据库");
                }
                //throw (e);
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Close();
            }
#endif
            return result;
        }
        
        /// <summary>
        /// 数据库关闭
        /// </summary>
        private static void Close()
        {
#if IS_USE_MYSQL
            if (myConnection.State == ConnectionState.Open)
            {
                myConnection.Close();
            }
#endif
        }
        #endregion

#region 查询数据库表信息

        /// <summary>
        /// 设置指定数据库
        /// </summary>
        /// <param name="databaseName"></param>
        public static void SetDatabase(string databaseName)
        {
            Database_Name = databaseName;
            Connect();
        }

        /// <summary>
        /// 返回所有的数据库列表
        /// </summary>
        /// <returns></returns>
        public static List<string> AllDatabaseNames()
        {
            List<string> result = new List<string>();
            DataTable tables = UtilMysql.SqlExecute(Sql_Databases);
            string database_name;
            foreach (DataRow item in tables.Rows)
            {
                database_name = (string)item.ItemArray[0];
                result.Add(database_name);
            }
            return result;
        }

        /// <summary>
        /// 查询所有表名
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> TableList()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            DataTable tables = UtilMysql.SqlExecute(Sql_Tables);
            string tablename, key;
            foreach (DataRow item in tables.Rows)
            {
                tablename = (string)item.ItemArray[0];
                key = tablename.ToLower();
                result[key] = tablename;
            }
            return result;
        }

        /// <summary>
        /// 获取所有的表信息包括表注释信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> TableinfoList()
        {
            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
            DataTable tables = UtilMysql.SqlExecute(Sql_Tables_Info);
            string tablename, key;
            foreach (DataRow item in tables.Rows)
            {
                tablename = (string)item.ItemArray[0];
                Dictionary<string, string> tableInfo = new Dictionary<string, string>();
                tableInfo["Name"] = tablename;
                tableInfo["Comment"] = (string)item.ItemArray[item.ItemArray.Count()-1];
                key = tablename.ToLower();
                result[key] = tableInfo;
            }
            return result;
        }

        /// <summary>
        /// 获取指定表所有的列信息
        /// </summary>
        /// <param name="table_name"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> FieldInfoList(string table_name)
        {
            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();

            string sql = string.Format(Sql_Table_Columns, table_name);
            DataTable columns = UtilMysql.SqlExecute(sql);
            string column_name, column_type_length;
            foreach (DataRow item in columns.Rows)
            {
                Dictionary<string, string> columnInfo = new Dictionary<string, string>();
                column_name = (string)item.ItemArray[0];
                columnInfo["Field"] = column_name;
                column_type_length = (string)item.ItemArray[1];
                columnInfo["Type"] = column_type_length;
                columnInfo["Type_Only"] = Column_Type_Only(column_type_length);
                columnInfo["Length"] = Column_Length(column_type_length);
                columnInfo["Null"] = (string)item.ItemArray[3];

                string fkPk = "";
                string[] tbl = table_name.Split('_');
                if (column_name.ToUpper().Contains("_ID") && (column_name.ToUpper().Contains(tbl[tbl.Length - 1].ToUpper())))
                {
                    fkPk = "PK";
                }

                if (column_name.ToUpper().Contains("_ID") && (!column_name.ToUpper().Contains(tbl[tbl.Length - 1].ToUpper())))
                {
                    fkPk = "FK";
                }
                columnInfo["Fkpk"] = fkPk;

                columnInfo["Default"] = (string.IsNullOrEmpty(item.ItemArray[5].ToString())) ? null : (string)item.ItemArray[5];
                columnInfo["Comment"] = (string)item.ItemArray[item.ItemArray.Count() - 1];
                result[column_name] = columnInfo;
            }
            return result;
        }

        /// <summary>
        /// 表中列的类型定义
        /// </summary>
        /// <param name="ColumnTypeLength">列类型，包括列长度</param>
        /// <returns></returns>
        private static string Column_Type_Only(string ColumnTypeLength)
        {
            if (ColumnTypeLength.Contains("enum"))
            {
                return ColumnTypeLength;
            }
            else if (ColumnTypeLength.Contains("("))
            {
                string[] typeLength = ColumnTypeLength.Split(new char[] { '[', ']', '(', ')' });
                return typeLength[0];
            }
            else
            {
                return ColumnTypeLength;
            }
        }

        /// <summary>
        /// 表中列的长度定义
        /// </summary>
        /// <param name="ColumnTypeLength">列类型，包括列长度</param>
        /// <returns></returns>
        private static string Column_Length(string ColumnTypeLength)
        {
            if (ColumnTypeLength.Contains("enum"))
            {
                return "1";
            }
            else if (ColumnTypeLength.Contains("timestamp"))
            {
                return "10";
            }
            else if (ColumnTypeLength.Contains("("))
            {
                string[] typeLength = ColumnTypeLength.Split(new char[] { '[', ']', '(', ')' });
                return typeLength[1];
            }
            else
            {
                return "1";
            }
        }
        #endregion
    }
}