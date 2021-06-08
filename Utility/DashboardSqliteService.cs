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
            try
            {


                BaseService<tbl_OrganisationDetails> gg = new BaseService<tbl_OrganisationDetails>();
                gg.Add(tbl_Organisation);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        public void InsertUserOrganisationList(List<tbl_OrganisationDetails> tbl_Organisation)
        {
            try
            {
                BaseService<tbl_OrganisationDetails> gg = new BaseService<tbl_OrganisationDetails>();
                gg.AddRange(tbl_Organisation);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void InsertUserProjectsByOrganisationID(tbl_Organisation_Projects tbl_OrganisationProjects)
        {
            try
            {

                BaseService<tbl_Organisation_Projects> gg = new BaseService<tbl_Organisation_Projects>();
                gg.Add(tbl_OrganisationProjects);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void InsertUserProjectsListByOrganisationID(List<tbl_Organisation_Projects> tbl_OrganisationProjects)
        {
            try
            {


                BaseService<tbl_Organisation_Projects> gg = new BaseService<tbl_Organisation_Projects>();
                gg.AddRange(tbl_OrganisationProjects);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public List<tbl_OrganisationDetails> GetOrganisation()
        {
            List<tbl_OrganisationDetails> org = new List<tbl_OrganisationDetails>();
            try
            {
                BaseService<tbl_OrganisationDetails> service = new BaseService<tbl_OrganisationDetails>();
                org = new List<tbl_OrganisationDetails>(service.GetAll());
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //MyMessageBox.Show(new Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
                //throw new Exception(ex.StackTrace);
            }
            return org;

        }

        public List<tbl_Organisation_Projects> GetProjectsByOrganisationId(string OrganisationId)
        {
            List<tbl_Organisation_Projects> lstorg = new List<tbl_Organisation_Projects>();
            try
            {


                BaseService<tbl_Organisation_Projects> service = new BaseService<tbl_Organisation_Projects>();
                lstorg= new List<tbl_Organisation_Projects>(service.GetAllById(OrganisationId, "OrganisationId"));
                return lstorg;
                // return new List<tbl_Organisation_Projects>(service.GetAllById(OrganisationId, "OrganisationId"));
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
            return lstorg;

        }

        public void InsertUserToDoList(tbl_ServerTodoDetails tbl_ToDoList)
        {
            try
            {

                BaseService<tbl_ServerTodoDetails> gg = new BaseService<tbl_ServerTodoDetails>();
                gg.Add(tbl_ToDoList);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public List<tbl_ServerTodoDetails> GetToDoListData(int CurrentProjectId)
        {
            List<tbl_ServerTodoDetails> tbl_ServerTodos = new List<tbl_ServerTodoDetails>();
            try
            {
                BaseService<tbl_ServerTodoDetails> service = new BaseService<tbl_ServerTodoDetails>();
                tbl_ServerTodos= new List<tbl_ServerTodoDetails>(service.GetAllById(CurrentProjectId, "CurrentProjectId"));
                return tbl_ServerTodos;
                //return new List<tbl_ServerTodoDetails>(service.GetAllById(CurrentProjectId, "CurrentProjectId"));
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return tbl_ServerTodos;
        }
        public void AddTimeIntervalToDB(string StartTime, string EndTime)
        {
            try
            {


                tbl_KeyMouseTrack_Slot keyMouseTrack_Slot;
                DateTime oCurrentDate = DateTime.Now;

                var keyboardActivity = Common.Storage.KeyBoradEventCount.ToStrVal();
                var MouseActivity = Common.Storage.MouseEventCount.ToStrVal();
                var AverageActivity = Common.Storage.AverageEventCount.ToStrVal();

                Common.Storage.AverageEventCount = 0;
                Common.Storage.KeyBoradEventCount = 0;
                Common.Storage.MouseEventCount = 0;

                keyMouseTrack_Slot = new tbl_KeyMouseTrack_Slot()
                {
                    IntervalStratTime = StartTime,
                    IntervalEndTime = EndTime,
                    Start = Common.Storage.SlotTimerStartTime,
                    End = "",
                    Hour = 0,
                    keyboardActivity = keyboardActivity,
                    MouseActivity = MouseActivity,
                    AverageActivity = AverageActivity,
                    Id = 0,
                    Latitude = null,
                    Longitude = null,
                    ScreenActivity = Common.Storage.ScreenURl,
                    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                };
                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                Common.Storage.SlotID = gg.Add(keyMouseTrack_Slot);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }

        public void UpdateTimeIntervalToDB(string IntervalEndTime, string EndTime, string CurrentDate)
        {
            try
            {

                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                gg.UpdateIntervalToDB(IntervalEndTime, EndTime, Common.Storage.SlotID, CurrentDate);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }

        public void SaveTimeIntbl_ProjectDetailsDB(tbl_Temp_SyncTimer objProject, bool boolVal)
        {
            string strLogTime = "";
            try
            {


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
                            service2.updatetbl_ProjectDetails(strLogTime, objProject.ProjectId, objProject.OrganizationId);
                        }
                    }
                    else
                    {
                        service2.Add(objProject);
                    }

                }
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        public void Savetbl_TempSyncTimerTodoDetailsDB(tbl_TempSyncTimerTodoDetails objProject, bool boolVal)
        {
            string strLogTime = "";
            try
            {


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
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        public void SaveTimeIntbl_Temp_SyncTimerDB(tbl_Temp_SyncTimer objProject)
        {
            string strLogTime = "";
            try
            {

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
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void SaveStartStopProjectTimeINLocalDB(tbl_Timer objtblTimer, bool boolVal)
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

                    service.UpdateStartStopProjectTimeToDB(objtblTimer.Stop, Common.Storage.tblTimerSno);
                }

            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
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

                LogFile.ErrorLog(ex);
            }
        }

        public long InsertNotetoDB(tbl_AddNotes tbl_AddNote)
        {
            long data = 0;
            try
            {
                BaseService<tbl_AddNotes> gg = new BaseService<tbl_AddNotes>();
                data = gg.Add(tbl_AddNote);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
            return data;

        }
        public void UpdateNotetoDB(long Id)
        {
            try
            {
                BaseService<tbl_AddNotes> gg = new BaseService<tbl_AddNotes>();
                gg.UpdateNotes(Id);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        public List<tbl_AddNotes> GetNotesList()
        {
            List<tbl_AddNotes> listnote = new List<tbl_AddNotes>();
            try
            {

                BaseService<tbl_AddNotes> service = new BaseService<tbl_AddNotes>();
                listnote =new List<tbl_AddNotes>(service.GetAllOfflineNotes());
                //return new List<tbl_AddNotes>(service.GetAllOfflineNotes());
                return listnote;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return listnote;
        }
    }

}
