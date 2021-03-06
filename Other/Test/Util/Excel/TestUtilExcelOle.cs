﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Database;
using System.Data;
using Util.DataType.Datatable;
using System.Collections.Generic;
using System.Linq;
using Administrator=Database.Admin;
using Database.Domain.Enums;
using Util.Common;
using Business;
using System.IO;

namespace Test.Util.Excel
{
    /// <summary>
    /// 测试:采用OLE方式读写Excel
    /// </summary>
    [TestClass]
    public class TestUtilExcelOle
    {
        private static BetterlifeNetEntities db;

        /// <summary>
        /// 初始化
        /// </summary>
        [TestInitialize]
        public void init()
        {
            Gc.init(EnumAppType.AppExe);
            db = new BetterlifeNetEntities();
        }

        /// <summary>
        /// 导出系统管理员
        /// </summary>
        [TestMethod]
        public void exportAdmin()
        {
            var admins = db.Admin.OrderByDescending(e => e.ID).Take(50);
            if (admins != null)
            {
                var query = admins.AsEnumerable();

                foreach (Administrator row in query)
                {
                    if (row.Roletype != null) row.RoletypeShow = EnumRoletype.RoletypeShow(Convert.ToChar(row.Roletype));
                    if (row.Seescope != null) row.SeescopeShow = EnumSeescope.SeescopeShow(Convert.ToChar(row.Seescope));
                    row.Department_Name = row.Department.Department_Name;
                }

                DataTable dt = UtilDataTable.ToDataTable(query);
                dt.TableName = "Admin";
                UtilDataTable.DeleteColumns(dt, "Seescope", "Roletype", "Department_ID");
                Dictionary<string, string> dic = new Dictionary<string, string>()
                {
                    {"ID","编号"},
                    {"Department_Name","部门名称"},
                    {"Username","用户名称"},
                    {"Password","密码"},
                    {"Realname","真实姓名"},
                    {"RoletypeShow","扮演角色"},
                    {"SeescopeShow","视野"},
                    {"LoginTimes","登录次数"},
                    {"CommitTime","创建时间"},
                    {"UpdateTime","更新时间"}
                };
                UtilDataTable.ReplaceColumnName(dt, dic);

                string fileName = Path.Combine(Gc.UploadPath, "attachment", "admin", UtilDateTime.NowS() + ".xls");
                UtilExcelOle.DataTableToExcel(fileName, dt);
                Console.WriteLine("导出文件:"+fileName);
            }
        }

        /// <summary>
        /// 导入系统管理员
        /// </summary>
        [TestMethod]
        public void importAdmin()
        {
            string fileName = "admin20140611084350.xls";
            fileName = Path.Combine(Gc.UploadPath, "attachment", "admin", fileName);
            //Excel导出入到DataTable
            DataTable dt = UtilExcelOle.ExcelToDataTableBySheet(fileName,"Admin");
            if (dt != null)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>()
                {
                    {"编号","ID"},
                    {"部门名称","Department_Name"},
                    {"用户名称","Username"},
                    {"密码","Password"},
                    {"真实姓名","Realname"},
                    {"扮演角色","RoletypeShow"},
                    {"视野","SeescopeShow"},
                    {"登录次数","LoginTimes"},
                    {"创建时间","CommitTime"},
                    {"更新时间","UpdateTime"}
                };

                UtilDataTable.ReplaceColumnName(dt, dic);

                //检查数据格式是否正确
                //1、检查Excel中是否有用户名重复数据
                if (UtilDataTable.HasRepeat(dt, "Username"))
                {
                    Console.WriteLine("Excel中有重复用户名");
                    return;
                }
                //2、检查Excel中用户名是否存在与数据中相同数据
                List<Admin> admins = db.Admin.ToList();
                DataTable tableDt = UtilDataTable.ToDataTable<Admin>(admins);
                if (UtilDataTable.HasRepeat(tableDt, dt, "Username"))
                {
                    Console.WriteLine("Excel中与数据库有重复用户名");
                    //return;
                }
                //循环插入数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Admin admin = new Admin();
                    UtilDataTable.ToObject(admin, dt.Rows[i], dt.Columns);
                    Department dep = db.Department.Where(e => e.Department_Name.Equals(admin.Department_Name)).SingleOrDefault();
                    admin.Department_ID = dep.ID;
                    admin.Seescope = EnumSeescope.SeescopeByShow(admin.SeescopeShow);
                    admin.Roletype = EnumRoletype.RoletypeByShow(admin.RoletypeShow);
                    //admin.CommitTime = DateTime.Now;
                    //admin.UpdateTime = DateTime.Now;
                    db.Admin.Add(admin);
                }
                db.SaveChanges();
            }

        }
    }
}
