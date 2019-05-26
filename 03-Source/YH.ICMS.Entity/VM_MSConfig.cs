using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YH.ICMS.Entity
{
    [Serializable]
    public class VM_MSConfig
    {
        public string TrainDirection
        {
            get; set;
        }

        public string SqlConnect
        {
            get; set;
        }
        public string AutoStart
        {
            get; set;
        }

        public string IP
        {
            get; set;
        }
        public string Port
        {
            get; set;
        }

        public int TimeOut
        {
            get; set;
        }

        public string Com
        {
            get; set;
        }
        public int BaudRate
        {
            get; set;
        }

        public int DataBits
        {
            get; set;
        }
        public int StopBits
        {
            get; set;

        }
        public Byte Station
        {
            get; set;
        }
        public int ComIndex
        {
            get; set;
        }
        public int Parity
        {
            get;set;
        }
        public bool ComFlag 
        {
            get; set;
        }
        public bool DDFlag
        {
            get; set;
        }

        public int VoltageBaudRate
        {
            get; set;
        }

        public void Save(string filename)
        {
            FileStream fs = null;
            String sPath = filename.Substring(0, filename.LastIndexOf('\\'));
            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);
            // serialize it...
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(fs, this);
            }
            catch (Exception ex)
            {
                return;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
    }
}
