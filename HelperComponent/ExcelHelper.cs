using System;
using System.IO;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Data.OleDb;

namespace HelperComponent
{
    public class ExcelHelper
    {
        /// <summary>
        /// 将 DataTable 转化为 ExcelXml 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="displayColumnNames"></param>
        /// <returns></returns>
        public static string DataTableToExcelTableXML(DataTable dt, string[] displayColumnNames)
        {
            // 表开始 
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<ss:Workbook xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sb.AppendLine(" <ss:Styles>");
            sb.AppendLine("     <ss:Style ss:ID=\"1\">");
            sb.AppendLine("         <ss:Font ss:Bold=\"1\"/>");
            sb.AppendLine("         <ss:Interior ss:Color=\"#FFC000\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("     </ss:Style>");
            sb.AppendLine(" </ss:Styles>");

            sb.AppendLine(" <ss:Worksheet ss:Name=\"Sheet1\">");
            sb.AppendLine("  <ss:Table>");
            // 
            // 输出标题 
            // 
            sb.AppendLine("   <ss:Row>");
            if (displayColumnNames != null)
            {
                // 输出展示列标题 
                for (int i = 0; i < displayColumnNames.Length; i++)
                    sb.AppendLine("    <ss:Cell  ss:StyleID=\"1\"><ss:Data ss:Type=\"String\">" + displayColumnNames[i] + "</ss:Data></ss:Cell>");
            }
            else
            {
                // 输出所有列标题 
                for (int i = 0; i < dt.Columns.Count; i++)
                    sb.AppendLine("    <ss:Cell  ss:StyleID=\"1\"><ss:Data ss:Type=\"String\">" + dt.Columns[i].Caption + "</ss:Data></ss:Cell>");
            }
            sb.AppendLine("   </ss:Row>");
            // 
            // 输出数据 
            // 
            if (displayColumnNames != null)
            {
                // 输出指定列数据 
                foreach (DataRow dr in dt.Rows)
                {
                    sb.AppendLine("   <ss:Row>");
                    foreach (string colName in displayColumnNames)
                        sb.AppendLine("    <ss:Cell><ss:Data ss:Type=\"String\">" + dr[colName].ToString() + "</ss:Data></ss:Cell>");
                    sb.AppendLine("   </ss:Row>");
                }
            }
            else
            {
                //输出所有列数据 
                foreach (DataRow dr in dt.Rows)
                {
                    sb.AppendLine("   <ss:Row>");
                    Object[] ary = dr.ItemArray;
                    for (int i = 0; i <= ary.GetUpperBound(0); i++)
                        sb.AppendLine("    <ss:Cell><ss:Data ss:Type=\"String\">" + ary[i].ToString() + "</ss:Data></ss:Cell>");
                    sb.AppendLine("   </ss:Row>");
                }
            }
            // 表结束 
            sb.AppendLine("  </ss:Table>");
            sb.AppendLine(" </ss:Worksheet>");
            sb.AppendLine("</ss:Workbook>");
            return sb.ToString();
        }


        /// <summary>
        /// 获取Excel中的数据
        /// </summary>
        /// <param name="nameCellArea">命名单元格区域</param>
        /// <param name="filePath">excel文件路径</param>
        /// <returns></returns>
        public static DataTable GetTableFromExcel(string nameCellArea, string filePath)
        {
            const string connStrTemplate = "Provider=Microsoft.Ace.OleDb.12.0;Data Source={0};Extended Properties='Excel 12.0; HDR=Yes; IMEX=1'";
            DataTable dt = null;
            OleDbConnection conn = new OleDbConnection(string.Format(connStrTemplate, filePath));
            try
            {
                conn.Open();
                if (nameCellArea == null || nameCellArea.Trim().Length == 0)
                {
                    //从excel中获取table,这些table是在excel中定义的"命名单元格区域",即一个"命名单元格区域"就是一个table
                    DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    //sheetName 是一个最大的"命名单元格区域",如果excel中没有定义"命名单元格区域",那么就用最大的命名单元格区域即sheet
                    nameCellArea = schemaTable.Rows[0]["TABLE_NAME"].ToString().Trim();
                }

                string strSQL = "Select * From [" + nameCellArea + "]";
                OleDbDataAdapter da = new OleDbDataAdapter(strSQL, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }

    }

}
