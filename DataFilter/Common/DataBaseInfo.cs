using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using HelperComponent;

namespace DataFilter.Common
{
    public class DataBaseInfo
    {
        static Dictionary<string, string> m_DictConnectionStrings;
        public static Dictionary<string, string> ConnectionStrings
        {
            get
            {
                if (m_DictConnectionStrings == null || m_DictConnectionStrings.Count <= 0)
                {
                    m_DictConnectionStrings = new Dictionary<string, string>();
                    DESCryptographyHelper desHelper = new DESCryptographyHelper();
                    for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
                    {
                        string connName = ConfigurationManager.ConnectionStrings[i].Name;
                        string connString = ConfigurationManager.ConnectionStrings[i].ConnectionString;

                        m_DictConnectionStrings.Add(connName, desHelper.Decrypt(connString, ConfigurationManager.AppSettings["secretKey"]));
                    }
                }

                return m_DictConnectionStrings;
            }
        }
    }
}