using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ICMS.DataGateway;
using ICMS.Commons;
using ICMS.Modules.BaseComponents.Beans;

namespace ICMS.Core
{
    class ModuleController
    {

        private ModuleDAO moduleDao;

        public ModuleController()
        {
            moduleDao=new ModuleDAO();
        }

        public List<ModuleEntity> GetModuleItems()
        {
            List<ModuleEntity> moduleEntities=new List<ModuleEntity>();
            DataSet ds = moduleDao.GetModuleDs();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ModuleEntity entity = new ModuleEntity();
                    entity.Id = ds.Tables[0].Rows[i]["ID"].ToString(); 
                    entity.LineName = ds.Tables[0].Rows[i]["LINE_NAME"].ToString();
                    entity.StationCode = ds.Tables[0].Rows[i]["STATION_CODE"].ToString();
                    entity.StationName = ds.Tables[0].Rows[i]["STATION_NAME"].ToString();
                    entity.AssemblyName = ds.Tables[0].Rows[i]["ASSEMBLY_NAME"].ToString();
                    entity.ModuleNameSpace = ds.Tables[0].Rows[i]["MODULE_NAMESPACE"].ToString();
                    entity.ClassName = ds.Tables[0].Rows[i]["CLASS_NAME"].ToString();
					entity.ClientIp = ds.Tables[0].Rows[i]["CLIENT_IP"].ToString();
                    entity.PortNumber = ds.Tables[0].Rows[i]["PORT_NUMBER"].ToString();
                    entity.EqType = ds.Tables[0].Rows[i]["EQ_TYPE"].ToString();
                    if (entity.StationName.Length > 0)
                    {
                        moduleEntities.Add(entity);
                    } 
                }
            }
            return moduleEntities;
        }
    }
}
