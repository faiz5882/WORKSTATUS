using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Common;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.Utility
{
   public class DashboardSqliteService
    {

        public void InsertUserOrganisation(tbl_OrganisationDetails tbl_Organisation)
        {
            BaseService<tbl_OrganisationDetails> gg = new BaseService<tbl_OrganisationDetails>();
            gg.Add(tbl_Organisation);
        }

        public void InsertUserOrganisationList(List<tbl_OrganisationDetails> tbl_Organisation)
        {
            BaseService<tbl_OrganisationDetails> gg = new BaseService<tbl_OrganisationDetails>();
             gg.AddRange(tbl_Organisation);
        }
        public void InsertUserProjectsByOrganisationID(tbl_Organisation_Projects tbl_OrganisationProjects)
        {
            BaseService<tbl_Organisation_Projects> gg = new BaseService<tbl_Organisation_Projects>();
            gg.Add(tbl_OrganisationProjects);
        }
        public void InsertUserProjectsListByOrganisationID(List<tbl_Organisation_Projects> tbl_OrganisationProjects)
        {
            BaseService<tbl_Organisation_Projects> gg = new BaseService<tbl_Organisation_Projects>();
            gg.AddRange(tbl_OrganisationProjects);
        }
        public List<tbl_OrganisationDetails> GetOrganisation()
        {
            BaseService<tbl_OrganisationDetails> service = new BaseService<tbl_OrganisationDetails>();
            return new List<tbl_OrganisationDetails>(service.GetAll());
        }

        public List<tbl_Organisation_Projects> GetProjectsByOrganisationId(string OrganisationId)
        {
            BaseService<tbl_Organisation_Projects> service = new BaseService<tbl_Organisation_Projects>();
            return new List<tbl_Organisation_Projects>(service.GetAllById(OrganisationId, "OrganisationId"));
        }
    }
   
}
