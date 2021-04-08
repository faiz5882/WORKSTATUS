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

        public void InsertUserToDoList(tbl_ServerTodoDetails tbl_ToDoList)
        {
            BaseService<tbl_ServerTodoDetails> gg = new BaseService<tbl_ServerTodoDetails>();
            gg.Add(tbl_ToDoList);
        }
        public List<tbl_ServerTodoDetails> GetToDoListData(int CurrentProjectId)
        {
            BaseService<tbl_ServerTodoDetails> service = new BaseService<tbl_ServerTodoDetails>();
            return new List<tbl_ServerTodoDetails>(service.GetAllById(CurrentProjectId, "CurrentProjectId"));
        }
        public void AddTimeIntervalToDB(string StartTime, string EndTime)
        {
            try
            {


                tbl_KeyMouseTrack_Slot keyMouseTrack_Slot;
                DateTime oCurrentDate = DateTime.Now;
                keyMouseTrack_Slot = new tbl_KeyMouseTrack_Slot()
                {
                    IntervalStratTime = StartTime,
                    IntervalEndTime = EndTime,
                    Start = Common.Storage.SlotTimerStartTime,
                    End = "",
                    Hour = 0,
                    keyboardActivity = null,
                    MouseActivity = null,
                    AverageActivity = null,
                    Id = 0,
                    Latitude=null,
                    Longitude=null,
                    ScreenActivity=null,
                    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                };
                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
              Common.Storage.SlotID=  gg.Add(keyMouseTrack_Slot);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateTimeIntervalToDB(string IntervalEndTime, string CurrentDate)
        {
            try
            {

                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                gg.UpdateIntervalToDB(IntervalEndTime, Common.Storage.SlotID, CurrentDate);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public void SaveTimeIntbl_ProjectDetailsDB(tbl_ProjectDetails objProject)
        {
            BaseService<tbl_ProjectDetails> service2 = new BaseService<tbl_ProjectDetails>();            
            tbl_ProjectDetails p = new tbl_ProjectDetails();
             p = service2.Gettbl_ProjectDetailsByIDs(objProject.ProjectId, objProject.OrganizationId, objProject.CreatedDate);
            if (p != null)
            {
                if(p.ProjectId>0)
                {
                    service2.updatetbl_ProjectDetails(objProject.TotalWorkedHours, objProject.ProjectId, objProject.OrganizationId);
                }
                else
                {
                    service2.Add(objProject);
                }
              
            }
                  
        }
       
        public void SaveStartStopProjectTimeINLocalDB(tbl_Timer objtblTimer, bool boolVal)
        {
            try
            {

                BaseService<tbl_Timer> service = new BaseService<tbl_Timer>();
                if (boolVal)
                {

                   Common.Storage.tblTimerSno=  service.Add(objtblTimer);                   
                }
                else
                {

                    service.UpdateStartStopProjectTimeToDB(objtblTimer.Stop,Common.Storage.tblTimerSno);
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public void AddUpdateProjectTimeINLocalDBByToDoID(tbl_Timer objtblTimer, bool boolVal)
        {
            try
            {

                BaseService<tbl_Timer> service = new BaseService<tbl_Timer>();
                if (boolVal)
                {

                    Common.Storage.tblTimerSno = service.Add(objtblTimer);
                }
                else
                {

                    service.UpdateProjectTimeToDBByToDoID(objtblTimer.Stop, Common.Storage.tblTimerSno);
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
    }

}
