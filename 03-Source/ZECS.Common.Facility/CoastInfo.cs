using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ZECS.Common.Facility
{
    public class CoastInfo
    {
        /*
        public bool Init(string strCon, string strDBType)
        {
            m_dbAccess.ConnString = strCon;
            m_dbAccess.DatabaseType = strDBType;
            m_dbAccess.Connect();

            //1. load all bollards
            DataSet ds = m_dbAccess.ReturnDataSet("select * from T_COAST_INFO where TYPE='BOLLARD'", "T_COAST_INFO");
            DataTable dt = ds.Tables["T_COAST_INFO"];

            foreach (DataRow row in dt.Rows)
            {
                Bollard bol = new Bollard();
                bol.ID = row["KEY"].ToString();
                bol.Pos = int.Parse(row["VALUE1"].ToString());

                m_dicBollard.Add( bol.ID, bol);
            }


            //2. load start and end
            ds = m_dbAccess.ReturnDataSet("select * from T_COAST_INFO where TYPE='NORMAL' and KEY='START' ", "T_COAST_INFO");
            dt = ds.Tables["T_COAST_INFO"];

            //3. load 



            return true;
        }
        */

        public int GetStart()
        {
            return m_Start;
        }

        public int GetEnd()
        {
            return m_End;
        }

        public bool GetHighBollardDir()
        {
            return m_HighBollardDir;
        }

        public List<string> GetAllBoLard()
        {
            return null;
        }

        public Bollard GetBollard(string strID)
        {
            return null;
        }

    //    private DbAccess_Base m_dbAccess = new DbAccess_Base();
        private Dictionary<string, Bollard> m_dicBollard = new Dictionary<string, Bollard>();

        private int m_Start;
        private int m_End;
        private bool m_HighBollardDir;
    }

    public class Bollard
    {
        public int Pos
        {
            get { return m_Pos; }
            set { m_Pos = value; }
        }

        public string ID
        {
            get { return m_strID; }
            set { m_strID = value; }
        }

        private int m_Pos;
        private string m_strID;
    }
}
