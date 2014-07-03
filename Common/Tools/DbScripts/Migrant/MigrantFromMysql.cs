﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Util.Db;
using Util.Common;

namespace Tools.DbScripts.Migrant
{
    /// <summary>
    /// 参考框架betterlife->Bettlife.Net
    /// 两种数据库之间的规格进行betterlife使用的Mysql数据库到Betterlife.Net使用的Sqlserver数据库的移植
    /// </summary>
    /// <see cref="https://github.com/skygreen2001/betterlife"/>
    public static class MigrantFromMysql
    {
        /**
         * Mysql的数据库表信息
         */
        private static Dictionary<string, Dictionary<string, string>> tableInfos;
        /**
         * 所有需要定义表主键是GUID类型的表名称列表
         */
        private static String[] tablesIDTypeGuid = { "Logsystem","Loguser" };
        /**
         * 所有的表名列表
         */
        private static List<String> tablenames = new List<String>();
        /// <summary>
        /// 默认数据库
        /// </summary>
        private const string sampleDb = "BetterlifeNet";
        /// <summary>
        /// 默认初始化SQL脚本
        /// </summary>
        private const string initDataFile="DbScripts\\initdata.txt";
        /// <summary>
        /// 移植数据库脚本
        /// </summary>
        public static string run()
        {
            string result = "";

            tableInfos = UtilMysql.TableinfoList();

            //生成数据库主体部分
            result = CreateDbDefine();

            //生成数据库注释部分
            result += CreateDbComment();

            //创建数据库的外键部分
            result += CreateDbKeyDefine();
            if (UtilMysql.Database_Name.Contains(sampleDb))
            {
                result += InitBetterlifeNetData();
            }
            return result;
        }

        /// <summary>
        /// 修改数据库主键部分
        /// </summary>
        private static string CreateDbKeyDefine()
        {
            string result = "/****** 创建数据库的外键部分    Script Date:" + DateTime.Now + " ******/\r\n";
            string tablename, refer_tablename;
            string column_name, sql_template;
            foreach (Dictionary<string, string> tableInfo in tableInfos.Values)
            {
                //获取表名
                tablename = tableInfo["Name"];
                tablename = UtilString.UcFirst(tablename);
                Dictionary<string, Dictionary<string, string>> columnInfos;
                columnInfos = UtilMysql.FieldInfoList(tablename);
                //获取主键名称
                foreach (Dictionary<string, string> columnInfo in columnInfos.Values)
                {
                    column_name = columnInfo["Field"];
                    column_name = UtilString.UcFirst(column_name);
                    if (column_name.ToUpper().Contains("_ID"))
                    {
                        refer_tablename = column_name.Replace("_ID", "");
                        refer_tablename = refer_tablename.Replace("_id", "");
                        if (refer_tablename.ToUpper().Equals("PARENT"))
                        {
                            refer_tablename = tablename;
                        }
                        if (tablenames.Contains(refer_tablename))
                        {
                            sql_template = @"
ALTER TABLE [dbo].[{0}]  WITH CHECK ADD CONSTRAINT [FK_{0}_{1}] FOREIGN KEY([{2}])
REFERENCES [dbo].[{1}] ([ID])
GO

ALTER TABLE [dbo].[{0}] CHECK CONSTRAINT [FK_{0}_{1}]
GO
";
                            result += string.Format(sql_template, tablename, refer_tablename, column_name);
                        }

                    }
                }

            }
            return result;
        }

        /// <summary>
        /// 生成数据库注释部分
        /// </summary>
        private static string CreateDbComment()
        {
            string result = "/****** 创建数据库所有表的注释说明备注    Script Date:" + DateTime.Now + " ******/\r\n";

            string tablename, tableComment;
            string column_name, column_comment;
            tablenames.Clear();
            foreach (Dictionary<string, string> tableInfo in tableInfos.Values)
            {
                //获取表名
                tablename = tableInfo["Name"];
                tablename = UtilString.UcFirst(tablename);
                tablenames.Add(tablename);
                tableComment = tableInfo["Comment"];
                result += string.Format("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{0}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{1}'\r\nGO\r\n\r\n", tableComment, tablename);

                Dictionary<string, Dictionary<string, string>> columnInfos;
                columnInfos = UtilMysql.FieldInfoList(tablename);
                //获取主键名称
                foreach (Dictionary<string, string> columnInfo in columnInfos.Values)
                {
                    column_name = columnInfo["Field"];
                    column_name = UtilString.UcFirst(column_name);
                    column_comment = columnInfo["Comment"];
                    result += string.Format("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{0}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{1}', @level2type=N'COLUMN',@level2name=N'{2}'\r\nGO\r\n\r\n", column_comment, tablename, column_name);
                }
            }
            foreach (string tablename_n in tablenames)
            {
                Console.Write("\"" + tablename_n + "\"" + ",");
            }
            return result;
        }

        /// <summary>
        /// 生成数据库主体部分
        /// </summary>
        private static string CreateDbDefine()
        {
            string result = "/****** 创建数据库所有表    Script Date:" + DateTime.Now + " ******/\r\n";
            result += "USE " + UtilString.UcFirst(UtilMysql.Database_Name) + "\r\n";
            string tablename, refer_tablename, tableComment, columnDefine;
            string sqlTemplate, column_name, column_type, column_null, column_default, resetSeed;
            string defaultValue;
            Dictionary<string, Dictionary<string, string>> columnInfos;
            foreach (Dictionary<string, string> tableInfo in tableInfos.Values)
            {
                //获取表名
                tablename = tableInfo["Name"];
                tablename = UtilString.UcFirst(tablename);
                tableComment = tableInfo["Comment"];
                columnInfos = UtilMysql.FieldInfoList(tablename);
                columnDefine = "";
                defaultValue = "";
                resetSeed = "";
                //获取主键名称
                foreach (Dictionary<string, string> columnInfo in columnInfos.Values)
                {
                    column_name = columnInfo["Field"];
                    column_name = UtilString.UcFirst(column_name);
                    if (column_name.ToUpper().Equals("ID"))
                    {

                        if (tablesIDTypeGuid.Contains(tablename))
                        {
                            columnDefine += "[ID] [uniqueidentifier] NOT NULL,";
                            defaultValue += string.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_ID]  DEFAULT (newid()) FOR [ID]\r\nGO\r\n", tablename);
                        }
                        else
                        {
                            columnDefine += "[ID][int] IDENTITY(1,1) NOT NULL,";
                            resetSeed += "DBCC CHECKIDENT ('" + tablename + "', RESEED, 1)\r\n";
                        }
                    }
                    else
                    {
                        //获取列名|列类型|是否Null
                        column_type = columnInfo["Type"];
                        column_type = ConvertType(column_type);
                        column_null = (columnInfo["Null"].Equals("YES")) ? "NULL" : "NOT NULL";

                        column_default = columnInfo["Default"];
                        if (column_name.ToUpper().Contains("_ID"))
                        {
                            refer_tablename = column_name.Replace("_ID", "");
                            refer_tablename = refer_tablename.Replace("_id", "");
                            if (refer_tablename.ToUpper().Equals("PARENT"))
                            {
                                refer_tablename = tablename;
                            }
                            if (tablesIDTypeGuid.Contains(refer_tablename))
                            {
                                column_type = "[uniqueidentifier]";
                            }
                            else
                            {
                                column_type = "[int]";
                            }
                            column_null = "NOT NULL";
                        }
                        if (UtilString.Contains(column_name.ToUpper(), "TIME", "DATE"))
                        {
                            if (UtilString.Contains(column_name.ToUpper(), "TIMES"))
                            {
                                column_type = "[int]";
                            }
                            else
                            {
                                column_type = "[datetime]";
                            }
                        }

                        if (!string.IsNullOrEmpty(column_default) && (!column_name.ToUpper().Equals("UPDATETIME")))
                        {
                            if (!column_name.ToUpper().Contains("_ID"))
                            {
                                defaultValue += string.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}]\r\nGO\r\n", tablename, column_name, column_default);
                            }
                        }

                        if ((column_name.ToUpper().Equals("COMMITTIME")) || (column_name.ToUpper().Equals("UPDATETIME")))
                        {
                            defaultValue += string.Format("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [DF_{0}_{1}]  DEFAULT (getdate()) FOR [{1}]\r\nGO\r\n", tablename, column_name);
                        }

                        string[] type_length = column_type.Split(new char[4] { '[', ']', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                        if (type_length.Length == 2)
                        {
                            string length = type_length[1];
                            int i_len = UtilNumber.Parse(length);
                            if (i_len > 4000)
                            {
                                i_len = 4000;
                                column_type = "[" + type_length[0] + "]" + "(" + i_len + ")";
                            }
                        }

                        columnDefine += string.Format("     [{0}]  {1}  {2},", column_name, column_type, column_null);
                    }
                    columnDefine += "\r\n";
                }
                columnDefine = columnDefine.Substring(0, columnDefine.Length - 2);
                //生成表脚本
                sqlTemplate = @"
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[{0}] (
     {1}
 CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([ID] ASC)
 WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)ON [PRIMARY]
) ON [PRIMARY]
GO

";
                result += string.Format(sqlTemplate, tablename, columnDefine);

                //生成列默认值
                result += defaultValue;
                //自增长重置为0
                result += resetSeed;
            }
            return result;
        }

        /// <summary>
        /// 数据库:BetterlifeNet:初始化数据
        /// </summary>
        /// <returns></returns>
        private static string InitBetterlifeNetData()
        {
            string result = UtilFile.ReadFile2String(initDataFile);
            return result;
        }

        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <param name="oldType">原数据类型</param>
        /// <returns>新数据类型</returns>
        private static string ConvertType(string oldType)
        {
            string newType = oldType;
            string[] otArr = null;
            if (oldType.Contains("("))
            {
                otArr = oldType.Split(new char[2] { '(', ')' });
                oldType = otArr[0];

                switch (oldType)
                {
                    case "varchar":
                        oldType = "nvarchar";
                        break;
                    default:
                        break;
                }

                if ((otArr != null) && (otArr.Count() >= 2))
                {

                    if (oldType.Contains("enum"))
                    {
                        newType = "[tinyint]";
                    }
                    else if (oldType.Contains("int"))
                    {
                        newType = "[int]";
                    }
                    else if (oldType.Contains("bit"))
                    {
                        newType = "[bit]";
                    }
                    else
                    {
                        newType = "[" + oldType + "](" + otArr[1] + ")";
                    }
                }
            }
            else
            {
                switch (oldType)
                {
                    case "longtext":
                        oldType = "text";
                        break;
                    default:
                        break;
                }
                newType = "[" + oldType + "]";
            }
            return newType;
        }
    }
}
