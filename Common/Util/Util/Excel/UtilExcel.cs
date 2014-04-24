//ʹ��Excel��Com�������Ҫ��װOffice.
//�ο��ڶ��ַ�ʽ:http://blog.csdn.net/zhaoyu008/article/details/6294454
#define IS_USE_EXCEL_COM
//��ʹ��Excel��Com���
//#undef IS_USE_EXCEL_COM
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Web;
#if IS_USE_EXCEL_COM
using Excel = Microsoft.Office.Interop.Excel;
#endif
namespace Util.Common
{
    /// <summary>
    /// �����ࣺ���뵼��Excel�Ļ���
    /// http://www.cnblogs.com/springyangwc/archive/2011/08/12/2136498.html
    /// </summary>
    public class UtilExcel
    {
#if IS_USE_EXCEL_COM
        private Excel.Application app = null;
        private Excel.Workbook workbook = null;
        private Excel.Worksheet worksheet = null;
        private Excel.Range workSheet_range = null;
#endif

        public UtilExcel()
        {
            createDoc();
        }

        public void createDoc()
        {
#if IS_USE_EXCEL_COM
            try
            {
                app = new Excel.Application();
                app.Visible = false;
                workbook = app.Workbooks.Add(1);
                worksheet = (Excel.Worksheet)workbook.Sheets[1];
            }
            catch (Exception e)
            {
                Console.Write("Error:" + e.Message);
            }
            finally
            {

            }
#endif
        }

        public void InsertData(ExcelBE be)
        {
#if IS_USE_EXCEL_COM
            worksheet.Cells[be.Row, be.Col] = be.Text;
            worksheet.Name = "Summary";
            workSheet_range = worksheet.get_Range(be.StartCell, be.EndCell);
            workSheet_range.Merge(be.IsMerge);
            workSheet_range.Interior.Color = GetColorValue(be.InteriorColor);
            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.ColumnWidth = be.ColumnWidth;
            workSheet_range.RowHeight = be.RowHeight;
            //to-do:ö��HorizontalAlignment
            if (be.HorizontalAlignmentIndex == 1)
            {
                workSheet_range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft; ;
            }
            else if (be.HorizontalAlignmentIndex == 2)
            {
                workSheet_range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter; ;
            }
            //to-do:���õ�Ԫ��߿�(δ���ڲ���)
            workSheet_range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            workSheet_range.Borders.Color = System.Drawing.Color.White.ToArgb();

            workSheet_range.Font.Color = string.IsNullOrEmpty(be.FontColor) ? System.Drawing.Color.White.ToArgb() : System.Drawing.Color.Black.ToArgb();
            workSheet_range.Font.Name = be.FontName;
            workSheet_range.Font.Bold = be.FontBold;
            workSheet_range.Font.Size = be.FontSize;
            workSheet_range.NumberFormat = be.Formart;
#endif
        }

        private int GetColorValue(string interiorColor)
        {
            switch (interiorColor)
            {
                case "YELLOW":
                    return System.Drawing.Color.Yellow.ToArgb();
                case "GRAY":
                    return System.Drawing.Color.Gray.ToArgb();
                case "GAINSBORO":
                    return System.Drawing.Color.Gainsboro.ToArgb();
                case "Turquoise":
                    return System.Drawing.Color.Turquoise.ToArgb();
                case "PeachPuff":
                    return System.Drawing.Color.PeachPuff.ToArgb();
                default:
                    return System.Drawing.Color.White.ToArgb();
            }
        }

        /// <summary>
        /// ��ȡexcel�ļ�����
        /// </summary>
        /// <param name="filepath">�ļ�����·��</param>
        /// <param name="fields">�ֶ�ӳ��</param>
        /// <returns></returns>
        public DataTable CallExcel(string filepath, Dictionary<string, string> fields)
        {
            string strConn = GetExcelConnectionString(filepath);
            OleDbConnection objConn = new OleDbConnection(strConn);
            objConn.Open();
            string sql = "select * from [Worksheet$]";
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, objConn);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            dt = exchangeColName(dt, fields);
            objConn.Close();
            objConn.Dispose();
            return dt;
        }

        /// <summary>
        /// ��ȡ��ƷExcel
        /// </summary>
        /// <param name="filepath">�ļ�����·��</param>
        /// <param name="productfield">��Ʒ�ֶ�ӳ��</param>
        /// <param name="imagefield">��ƷͼƬ�ֶ�ӳ��</param>
        /// <returns></returns>
        public static Dictionary<String, DataTable> CallExcelFromBonli(string filepath, Dictionary<string, string> productfields, Dictionary<string, string> imagefields)
        {
            string strConn = GetExcelConnectionString(filepath);
            OleDbConnection objConn = new OleDbConnection(strConn);
            objConn.Open();
            string productsql = "select * from [product$]";
            OleDbDataAdapter productAdapter = new OleDbDataAdapter(productsql, objConn);
            string imagesql = "select * from [seriesimg$]";
            OleDbDataAdapter imageAdapter = new OleDbDataAdapter(imagesql, objConn);
            DataTable product = new DataTable();
            productAdapter.Fill(product);
            DataTable image = new DataTable();
            imageAdapter.Fill(image);
            product = exchangeColName(product, productfields);
            image = exchangeColName(image, imagefields);
            Dictionary<String, DataTable> result = new Dictionary<String, DataTable>();
            result.Add("Product", product);
            result.Add("Image", image);
            objConn.Close();
            objConn.Dispose();
            return result;
        }

        /// <summary>
        /// ���ı�ͷתΪ���ݱ��ֶ�
        /// </summary>
        /// <param name="dt">excel����</param>
        /// <param name="fields">��������</param>
        /// <returns></returns>
        public static DataTable exchangeColName(DataTable dt, Dictionary<string, string> fields)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                bool exist = fields.ContainsKey(dt.Columns[i].ColumnName);
                if (exist)
                {
                    dt.Columns[i].ColumnName = fields[dt.Columns[i].ColumnName];
                }
            }
            return dt;
        }

        /// <summary>
        /// �ж����ӷ�
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetExcelConnectionString(string filepath)
        {
            string connectionString = string.Empty;
            string fileExtension = filepath.Substring(filepath.LastIndexOf(".") + 1);
            switch (fileExtension)
            {
                case "xls":
                    connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
                    break;
                case "xlsx":
                    connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
                    break;
            }
            return connectionString;
        }

        /// <summary>
        /// ����
        /// </summary>
        public void doExport()
        {
            app.Visible = true;
        }
    }
}
