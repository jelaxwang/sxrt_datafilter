using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DataFilter.Common
{
    public class Script
    {
        static string sqlScriptsPath = System.Web.HttpContext.Current.Server.MapPath(@"~/Configs/SQLScripts.xml");
        static XElement scriptXml = XElement.Load(sqlScriptsPath);

        string m_scriptName;
        string m_SQLText;
        string m_DataBaseName;
        public Script(string scriptName)
        {
            m_scriptName = scriptName;
            foreach (var item in scriptXml.Descendants())
            {
                if (item.Attribute("name").Value == scriptName)
                {
                    m_SQLText = item.Value;
                    m_DataBaseName = item.Attribute("database").Value;
                    break;
                }
            }
        }

        public string ScriptName { get { return m_scriptName; } }
        public string SQLText { get { return m_SQLText; } }
        public string DataBaseName { get { return m_DataBaseName; } }
    }
}