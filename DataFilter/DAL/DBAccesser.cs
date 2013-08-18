using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using DataFilter.Common;
using HelperComponent;
using DataFilter.Models;

namespace DataFilter.DAL
{
    public class DBAccesser
    {
        static string m_B2BMastersTableName = "B2BMasters", m_B2BContactsTableName = "B2BContacts", m_B2CMastersTableName = "B2CMasters";
        public static string DefaultConnection { get { return DataBaseInfo.ConnectionStrings["dataCenter"]; } }

        /// <summary>
        /// 根据sql语句,获取结果
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sqltext"></param>
        /// <returns></returns>
        public static DataSet GetData(string connectionString, string sqltext)
        {
            DataSet ds = SqlHelper.ExecuteDataset(connectionString, CommandType.Text, sqltext);
            return ds;
        }

        /// <summary>
        /// 根据脚本名和脚本参数获取数据
        /// </summary>
        /// <param name="scriptName">脚本名</param>
        /// <param name="sqlParams">脚本参数集合</param>
        /// <returns></returns>
        public static DataSet GetData(string scriptName, params KeyValuePair<string, string>[] sqlParams)
        {
            Script script = new Script(scriptName);
            string sql = script.SQLText.ToLower();
            if (sqlParams != null && sqlParams.Length > 0)
            {
                foreach (var perParam in sqlParams)
                {
                    sql = sql.Replace(perParam.Key.ToLower(), perParam.Value);
                }
            }
            DataSet ds = SqlHelper.ExecuteDataset(DataBaseInfo.ConnectionStrings[script.DataBaseName], CommandType.Text, sql);
            return ds;
        }

        /// <summary>
        /// 获取数据表字段信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<TableField> GetTableFields(string tableName)
        {
            List<TableField> fields = new List<TableField>();
            DataSet ds1 = GetData("GetFields", new KeyValuePair<string, string>("@tableName@", tableName));
            fields = CreateFields(ds1.Tables[0], tableName);
            return fields;
        }

        public static List<TableField> GetFilterFields(bool isB2B)
        {
            List<TableField> filterFields = new List<TableField>();
            if (isB2B)
            {
                filterFields.AddRange(GetTableFields(m_B2BMastersTableName));
                filterFields.AddRange(GetTableFields(m_B2BContactsTableName));
                var temp = filterFields.FirstOrDefault(p => p.TableName.ToLower() == m_B2BContactsTableName.ToLower() && p.FieldName.ToLower() == "公司名称");
                if (temp != null)
                    filterFields.Remove(temp);
            }
            else
            {
                filterFields.AddRange(GetTableFields(m_B2CMastersTableName));
            }

            for (int i = filterFields.Count - 1; i >= 0; i--)
            {
                if (filterFields[i].FieldName.ToLower().IsIn("id", "indate", "lasteditdate"))
                {
                    filterFields.Remove(filterFields[i]);
                }
            }

            return filterFields;
        }

        /// <summary>
        /// 根据数据构建tableField 集合
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static List<TableField> CreateFields(DataTable dt, string tableName)
        {
            List<TableField> filterFields = new List<TableField>();
            if (dt != null)
            {
                foreach (DataRow item in dt.Rows)
                {
                    filterFields.Add(new TableField() { TableName = tableName, FieldName = Convert.ToString(item["FieldName"]), FieldType = Convert.ToString(item["FieldType"]) });
                }
            }
            return filterFields;
        }
    }
}