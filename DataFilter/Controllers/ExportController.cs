using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using DataFilter.Models;
using DataFilter.DAL;
using System.Data;
using HelperComponent;
using System.Text;
using System.IO;

namespace DataFilter.Controllers
{
    public class ExportController : Controller
    {
        static List<TableField> m_B2BFilterFields = DBAccesser.GetFilterFields(true);
        static List<TableField> m_B2CFilterFields = DBAccesser.GetFilterFields(false);

        public ActionResult B2BDataExport()
        {
            ViewBag.DefaultFilterCount = Convert.ToInt32(ConfigurationManager.AppSettings["defaultFilterCount"]);
            ViewBag.IsB2BData = true;
            ViewBag.Title = "Export B2B Data";
            return View("ExportData", m_B2BFilterFields);
        }

        public ActionResult B2CDataExport()
        {
            ViewBag.DefaultFilterCount = Convert.ToInt32(ConfigurationManager.AppSettings["defaultFilterCount"]);
            ViewBag.IsB2BData = false;
            ViewBag.Title = "Export B2C Data";
            return View("ExportData", m_B2CFilterFields);
        }

        [HttpPost]
        public string ExportData()
        {
            bool isB2BData = Convert.ToBoolean(Request.QueryString["isB2BData"]);

            string sqlSelect = Request.Form["Columns"];
            string sqlWhere = string.Empty;
            for (int i = 0; i < Request.Form.Keys.Count; i++)
            {
                string fieldNameKey = string.Format("Filters[{0}][FieldName]", i);
                string operationSignKey = string.Format("Filters[{0}][OperationSign]", i);
                string inputValueKey = string.Format("Filters[{0}][InputValue]", i);
                if (Request.Form[fieldNameKey] != null && Request.Form[operationSignKey] != null && Request.Form[inputValueKey] != null)
                {
                    sqlWhere += string.Format(" {0} {1} {2} and", Request.Form[fieldNameKey], Request.Form[operationSignKey], Request.Form[inputValueKey]);
                }
            }
            string sql = string.Empty;
            if (isB2BData)
            {
                sql = string.Format("select distinct {0} from dataCenter.dbo.B2BMasters with(nolock) left join dataCenter.dbo.B2BContacts with(nolock) on B2BContacts.公司名称 = B2BMasters.公司名称 where {1}", sqlSelect, sqlWhere.TrimEnd(" and".ToArray()));
            }
            else
            {
                sql = string.Format("select distinct {0} from dataCenter.dbo.B2CMasters with(nolock) where {1}", sqlSelect, sqlWhere.TrimEnd(" and".ToArray()));
            }

            DataSet ds = DBAccesser.GetData(DBAccesser.DefaultConnection, sql);

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                string excelXmlStr = ExcelHelper.DataTableToExcelTableXML(ds.Tables[0], null);

                string fileName = string.Format("{0}_{1}.xml", DateTime.Now.ToFileTimeUtc(), isB2BData ? "B2BDatas" : "B2CDatas");
                string filePath = Server.MapPath("~/TempExcelFiles/DownLoad/" + fileName);
                SaveExcel(filePath, excelXmlStr);

                return fileName;
            }
            else
            {
                return "error-nodata";
            }
        }

        public FileResult DownLoadExcel(string fileName)
        {
            var filePath = Server.MapPath(string.Format(@"~/TempExcelFiles/DownLoad/{0}", fileName));
            return File(filePath, "application/ms-excel", fileName);
        }

        private void SaveExcel(string path, string text)
        {
            if (!System.IO.File.Exists(path))
            {
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(path))
                {
                    sw.Write(text);
                }
            }
        }


    }
}
