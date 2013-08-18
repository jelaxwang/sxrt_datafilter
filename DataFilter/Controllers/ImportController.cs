using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Text;
using DataFilter.DAL;
using HelperComponent;
using DataFilter.Models;

namespace DataFilter.Controllers
{
    public class ImportController : Controller
    {
        static string m_B2BMastersTableName = "B2BMasters", m_B2BContactsTableName = "B2BContacts", m_B2CMastersTableName = "B2CMasters";
        static List<TableField> m_B2BMastersFields = DBAccesser.GetTableFields(m_B2BMastersTableName);
        static List<TableField> m_B2BContactsFields = DBAccesser.GetTableFields(m_B2BContactsTableName);
        static List<TableField> m_B2CMastersFields = DBAccesser.GetTableFields(m_B2CMastersTableName);

        public ActionResult B2BDataImport()
        {
            ViewBag.IsB2BData = true;
            ViewBag.Title = "Import B2B Data";
            return View("ImportData");
        }

        public ActionResult B2CDataImport()
        {
            ViewBag.IsB2BData = false;
            ViewBag.Title = "Import B2C Data";
            return View("ImportData");
        }

        [HttpPost]
        public ActionResult ImportData(string isB2BData)
        {
            ViewBag.IsB2BData = Convert.ToBoolean(isB2BData);
            ViewBag.Title = ViewBag.IsB2BData ? "Import B2B Data" : "Import B2C Data";


            if (Request.Files[0] != null && Request.Files[0].ContentLength > 0)
            {
                string fileName = string.Format("{0}_{1}", DateTime.Now.ToFileTimeUtc(), Path.GetFileName(Request.Files[0].FileName));
                string filePath = Server.MapPath("~/TempExcelFiles/UpLoad/" + fileName);
                Request.Files[0].SaveAs(filePath);

                DataTable tb = ExcelHelper.GetTableFromExcel("导入数据", filePath);

                if (Convert.ToBoolean(isB2BData))
                {
                    DataTable masterDatas = GetPartData(tb, m_B2BMastersFields.Select(p => p.FieldName).ToList());
                    DataTable contactDatas = GetPartData(tb, m_B2BContactsFields.Select(p => p.FieldName).ToList());

                    foreach (DataRow item in masterDatas.Rows)
                    {
                        string tempSql = GetUpdateSql(m_B2BMastersTableName, item, "公司名称");
                        if (tempSql.Length > 0)
                            DBAccesser.GetData(DBAccesser.DefaultConnection, tempSql);
                    }

                    foreach (DataRow item in contactDatas.Rows)
                    {
                        string tempSql = GetUpdateSql(m_B2BContactsTableName, item, "公司名称", "联系人姓名");
                        if (tempSql.Length > 0)
                            DBAccesser.GetData(DBAccesser.DefaultConnection, tempSql);
                    }
                }
                else
                {
                    DataTable masterDatas = GetPartData(tb, m_B2CMastersFields.Select(p => p.FieldName).ToList());
                    foreach (DataRow item in masterDatas.Rows)
                    {
                        DBAccesser.GetData(DBAccesser.DefaultConnection, GetUpdateSql(m_B2CMastersTableName, item, "姓名", "联系电话"));
                    }
                }

                DeleteUploadExcel(filePath);
                ViewBag.ImportMsg = "导入成功!";
            }

            return View("ImportData");
        }

        private bool HasFile(HttpPostedFileBase file)
        {
            return (file != null && file.ContentLength > 0) ? true : false;
        }

        private void DeleteUploadExcel(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        private void WriteToDB(DataRow row, DataColumnCollection columns, bool isB2BData)
        {
            try
            {
                string tableName = isB2BData ? "B2BDatas" : "B2CDatas";
                //filter 需要确认,即需要确认那些字段可以确定唯一一条记录,逻辑意义上, 这些字段在上传的时候是比需要有的字段
                string filter = string.Format("数据编码=N'{0}'", Convert.ToString(row["数据编码"]).Replace("'", "''").Trim());
                string fieldValues = string.Empty;
                StringBuilder sql = new StringBuilder();
                sql.AppendLine(string.Format("if exists(select top 1 1 from dataCenter.dbo.{0} with(nolock) where {1})", tableName, filter));
                sql.AppendLine("begin");
                sql.AppendLine(string.Format("update dataCenter.dbo.{0}", tableName));
                sql.AppendLine("set ");
                for (int i = 0; i < columns.Count; i++)
                {
                    sql.AppendLine(string.Format("{0}=N'{1}'", columns[i].ColumnName, Convert.ToString(row[columns[i]]).Replace("'", "''").Trim()));
                    if (i != columns.Count - 1)
                        sql.Append(",");
                }
                sql.AppendLine(string.Format("where {0}", filter));
                sql.AppendLine("end");
                sql.AppendLine("else");
                sql.AppendLine("begin");
                sql.AppendLine(string.Format("insert into dataCenter.dbo.{0}", tableName));
                sql.AppendLine("(" + string.Join(",", columns.Cast<DataColumn>().Select(p => p.ColumnName)) + ")");
                sql.AppendLine("values(");
                for (int i = 0; i < columns.Count; i++)
                {
                    sql.AppendLine(string.Format("N'{0}'", Convert.ToString(row[columns[i]]).Replace("'", "''").Trim()));
                    if (i != columns.Count - 1)
                        sql.Append(",");
                }
                sql.Append(")");
                sql.AppendLine("end");

                string tempSql = sql.ToString();
                DBAccesser.GetData(DBAccesser.DefaultConnection, tempSql);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// 获取更新或插入某个表的sql 语句
        /// </summary>
        /// <param name="tableName">要更新或插入的表名</param>
        /// <param name="row">要更新或插入的数据</param>
        /// <param name="locationColumnNames">在定位数据行的列名</param>
        /// <returns></returns>
        private string GetUpdateSql(string tableName, DataRow row, params string[] locationColumnNames)
        {
            DataColumnCollection columns = row.Table.Columns;
            StringBuilder sql = new StringBuilder();
            string filter = string.Empty;
            for (int i = 0; i < locationColumnNames.Length; i++)
            {
                if (columns.Contains(locationColumnNames[i].ToLower()))
                {
                    filter += string.Format("{0}='{1}'", locationColumnNames[i], Convert.ToString(row[locationColumnNames[i]]).Replace("'", "''").Trim());
                    if (i != locationColumnNames.Length - 1)
                        filter += " and ";
                }
                else
                {
                    return string.Empty;
                }
            }

            sql.AppendLine(string.Format("if exists(select top 1 1 from dataCenter.dbo.{0} with(nolock) where {1})", tableName, filter));
            sql.AppendLine("begin");
            sql.AppendLine(string.Format("update dataCenter.dbo.{0}", tableName));
            sql.AppendLine("set ");
            for (int i = 0; i < columns.Count; i++)
            {
                if (!columns[i].ColumnName.ToLower().IsIn(locationColumnNames))
                {
                    sql.AppendLine(string.Format("{0}=N'{1}'", columns[i].ColumnName, Convert.ToString(row[columns[i]]).Replace("'", "''").Trim()));
                    if (i != columns.Count - 1)
                        sql.Append(",");
                }
            }
            sql.AppendLine(",LastEditDate=getdate()");
            sql.AppendLine(string.Format("where {0}", filter));
            sql.AppendLine("end");
            sql.AppendLine("else");
            sql.AppendLine("begin");
            sql.AppendLine(string.Format("insert into dataCenter.dbo.{0}", tableName));
            sql.AppendLine("(" + string.Join(",", columns.Cast<DataColumn>().Select(p => p.ColumnName)) + ")");
            sql.AppendLine("values(");
            for (int i = 0; i < columns.Count; i++)
            {
                sql.AppendLine(string.Format("N'{0}'", Convert.ToString(row[columns[i]]).Replace("'", "''").Trim()));
                if (i != columns.Count - 1)
                    sql.Append(",");
            }
            sql.AppendLine(")");
            sql.AppendLine("end");

            return sql.ToString();
        }


        /// <summary>
        /// 获取部分数据
        /// </summary>
        /// <param name="dataSourceTb">数据源</param>
        /// <param name="columnNames">要获取数据源的哪些列</param>
        /// <returns></returns>
        private DataTable GetPartData(DataTable dataSourceTb, List<string> columnNames)
        {
            DataTable datas = new DataTable();
            foreach (string colName in columnNames)
            {
                if (dataSourceTb.Columns.Contains(colName))
                    datas.Columns.Add(dataSourceTb.Columns[colName].ColumnName, dataSourceTb.Columns[colName].DataType);
            }

            foreach (DataRow row in dataSourceTb.Rows)
            {
                DataRow tempRow = datas.NewRow();
                foreach (DataColumn column in datas.Columns)
                {
                    tempRow[column.ColumnName] = row[column.ColumnName];
                }
                datas.Rows.Add(tempRow);
            }
            return datas;
        }
    }
}
