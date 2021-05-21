using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Common;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Views;

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
            List<tbl_OrganisationDetails> org = new List<tbl_OrganisationDetails>();
            try
            {
                BaseService<tbl_OrganisationDetails> service = new BaseService<tbl_OrganisationDetails>();
                org= new List<tbl_OrganisationDetails>(service.GetAll());
            }
            catch (Exception ex)
            {
                MyMessageBox.Show(new Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
                //throw new Exception(ex.StackTrace);
            }            
            return org;
            
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
                    keyboardActivity = Common.Storage.KeyBoradEventCount.ToStrVal(),
                    MouseActivity = Common.Storage.MouseEventCount.ToStrVal(),
                    AverageActivity = Common.Storage.AverageEventCount.ToStrVal(),
                    Id = 0,
                    Latitude=null,
                    Longitude=null,
                    ScreenActivity=Common.Storage.ScreenURl,
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

        public void UpdateTimeIntervalToDB(string IntervalEndTime,string EndTime, string CurrentDate)
        {
            try
            {

                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                gg.UpdateIntervalToDB(IntervalEndTime, EndTime, Common.Storage.SlotID, CurrentDate);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public void SaveTimeIntbl_ProjectDetailsDB(tbl_Temp_SyncTimer objProject,bool boolVal)
        {
            string strLogTime = "";

            BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
            tbl_Temp_SyncTimer p = new tbl_Temp_SyncTimer();

            //if (boolVal)
            //{
            //    Common.Storage.temp_SyncTimerSno = service2.Add(objProject);
            //}
            //else
            //{
            //    service2.updatetbl_ProjectDetails(objProject.TotalWorkedHours, Common.Storage.temp_SyncTimerSno);
            //}
           
            p = service2.Gettbl_ProjectDetailsByIDs(objProject.ProjectId, objProject.OrganizationId, objProject.CreatedDate);
            if (p != null)
            {
                if (p.ProjectId > 0)
                {
                    if (!boolVal)
                    {
                        string[] arryT = p.TotalWorkedHours.Split(':');//old
                        if (!string.IsNullOrEmpty(objProject.TotalWorkedHours))
                        {
                            string[] arryTlogTime = objProject.TotalWorkedHours.Split(':');
                            int x, y, z;
                            x = arryTlogTime[0].ToInt32();
                            y = arryTlogTime[1].ToInt32();
                            z = arryTlogTime[2].ToInt32();
                            int c = arryT[2].ToInt32();
                            z += c;
                            y += arryT[1].ToInt32();
                            x+= arryT[0].ToInt32();
                            if (z >= 60)
                            {
                                z = 0;
                                y += 1;

                            }
                            if (y == 60)
                            {
                                y = 0;
                                x += 1;
                            }
                            strLogTime = String.Format("{0}:{1}:{2}", x.ToString().PadLeft(2, '0'), y.ToString().PadLeft(2, '0'), z.ToString().PadLeft(2, '0'));
                        }
                        service2.updatetbl_ProjectDetails(strLogTime, objProject.ProjectId, objProject.OrganizationId);
                    }
                }
                else
                {
                    service2.Add(objProject);
                }

            }

        }

        public void Savetbl_TempSyncTimerTodoDetailsDB(tbl_TempSyncTimerTodoDetails objProject, bool boolVal)
        {
            string strLogTime = "";

            BaseService<tbl_TempSyncTimerTodoDetails> service2 = new BaseService<tbl_TempSyncTimerTodoDetails>();
            tbl_TempSyncTimerTodoDetails p = new tbl_TempSyncTimerTodoDetails();
           
            p = service2.tbl_TempSyncTimerTodoDetails(objProject.ProjectId, objProject.OrganizationId, objProject.CreatedDate, objProject.TodoId);
            if (p != null)
            {
                if (p.ProjectId > 0)
                {
                    if (!boolVal)
                    {
                        string[] arryT = p.TotalWorkedHours.Split(':');//old
                        if (!string.IsNullOrEmpty(objProject.TotalWorkedHours))
                        {
                            string[] arryTlogTime = objProject.TotalWorkedHours.Split(':');
                            int x, y, z;
                            x = arryTlogTime[0].ToInt32();
                            y = arryTlogTime[1].ToInt32();
                            z = arryTlogTime[2].ToInt32();
                            int c = arryT[2].ToInt32();
                            z += c;
                            y += arryT[1].ToInt32();
                            x += arryT[0].ToInt32();
                            if (z >= 60)
                            {
                                z = 0;
                                y += 1;

                            }
                            if (y == 60)
                            {
                                y = 0;
                                x += 1;
                            }
                            strLogTime = String.Format("{0}:{1}:{2}", x.ToString().PadLeft(2, '0'), y.ToString().PadLeft(2, '0'), z.ToString().PadLeft(2, '0'));
                        }
                        service2.Updatetbl_TempSyncTimerTodoDetails(strLogTime, objProject.ProjectId, objProject.OrganizationId, objProject.TodoId);
                    }
                }
                else
                {
                    service2.Add(objProject);
                }

            }

        }

        public void SaveTimeIntbl_Temp_SyncTimerDB(tbl_Temp_SyncTimer objProject)
        {
            string strLogTime = "";
            BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
            tbl_Temp_SyncTimer p = new tbl_Temp_SyncTimer();
            p = service2.Gettbl_Temp_SyncTimerByIDs(objProject.ProjectId, objProject.OrganizationId, objProject.CreatedDate, objProject.TodoId);
            if (p != null)
            {
                if (p.ProjectId > 0)
                {
                    service2.Updatetbl_Temp_SyncTimer(objProject.TotalWorkedHours, objProject.ProjectId, objProject.OrganizationId, objProject.TodoId);
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

        public long InsertNotetoDB(tbl_AddNotes tbl_AddNote)
        {
            BaseService<tbl_AddNotes> gg = new BaseService<tbl_AddNotes>();
            long data = gg.Add(tbl_AddNote);
            return data;

        }
        public void UpdateNotetoDB(long Id)
        {
            BaseService<tbl_AddNotes> gg = new BaseService<tbl_AddNotes>();
            gg.UpdateNotes(Id);
        }

        public List<tbl_AddNotes> GetNotesList()
        {
            BaseService<tbl_AddNotes> service = new BaseService<tbl_AddNotes>();
            return new List<tbl_AddNotes>(service.GetAllOfflineNotes());
        }
    }

}
