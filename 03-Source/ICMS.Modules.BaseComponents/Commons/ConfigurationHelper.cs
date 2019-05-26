using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ICMS.Commons
{
    public static class ConfigurationHelper
    {
        public static string GetLocalConfigValue(string appKey)
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(Application.ExecutablePath + ".config");
                XmlNode xNode;
                XmlElement xElem;
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + appKey + "']");
                if (xElem != null)
                    return xElem.GetAttribute("value");
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void SetLocalConfigValue(string appKey, string value)
        {
            var doc = new XmlDocument();
            doc.Load(Application.ExecutablePath + ".config");
            var node = doc.SelectSingleNode(@"//appSettings");
            if (node != null)
            {
                var ele = (XmlElement)node.SelectSingleNode("//add[@key='" + appKey + "']");
                if (ele != null) ele.SetAttribute("value", value);
            }
            doc.Save(Application.ExecutablePath + ".config");

        }
    }
}
