using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Views;

namespace WorkStatus.Utility
{
    /// <summary>
    /// Author : Indra Singh Manral    
    /// Date : 04/03/2021
    /// Description : Entity/model classes as T 
    /// </summary>
    /// <typeparam name="T"> Class and newable type</typeparam>   


    #region Attributes
    /// <summary>
    /// Use this attribute to decorate the properties on your model class.
    /// Only those properties that are having exactly the same column name of a DB table.
    /// </summary>


    public class DbColumnAttribute : Attribute
    {
        /// <summary>
        /// Set true if implicit conversion is required.
        /// </summary>
        public bool Convert { get; set; }
        /// <summary>
        /// Set true if the property is primary key in the table
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Denotes if the field is an identity type or not.
        /// </summary>
        public bool IsIdentity { get; set; }
    }
    #endregion
    #region BaseService
    /// <summary>
    /// Use with Entity/Model class only
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseService<T> where T : class, new()
    {
        #region Constructor
        public BaseService()
        {

        }
        /// <summary>
        /// Pass the connection string in constructor
        /// </summary>
        /// <param name="connectionString"></param>

        public BaseService(string connectionString)
        {
            Context.ConnectionString = connectionString;
        }

        #endregion

        #region Public methods
        /// <summary>
        /// To get SQLite connection object
        /// </summary>
        /// <returns>SQLiteConnection object</returns>
        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(Configuration.Configurations.GetConnectionString());
        }
        /// <summary>
        /// Inserts the single record into table
        /// </summary>
        /// <param name="entity"></param>


        public void Delete(T entity)
        {
            T entityDelete = new T();
            ExecuteGet(string.Format("DELETE FROM [{0}]", entityDelete.GetType().Name));
        }
        public void DeleteSlot(long id)
        {
            string qry = "DELETE FROM tbl_KeyMouseTrack_Slot where Id=" +  id;
            ExecuteGet(qry.ToString());
        }
        public void DeleteSlotFromtbl_Timer(long Sno)
        {
            string qry = "DELETE FROM tbl_Timer where Sno=" + Sno;
            ExecuteGet(qry.ToString());
        }
        public void DeleteToDoAttachments()
        {
            try
            {
                string qry = "DELETE FROM tbl_ToDoAttachments";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateToDoListTable(int todoId)
        {
            try
            {

                string qry = "update tbl_ServerTodoDetails set  IsCompleted = 1  where Id =" + "" + todoId;
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

            }
        }
        public void DeleteSelectedToDoData(int todoId)
        {
            try
            {
                string qry = "DELETE FROM tbl_ServerTodoDetails where Id =" + "" + todoId;
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateNotes(long Id)
        {
            try
            {

                string qry = "update tbl_AddNotes set  IsOffline = 1  where Id =" + "" + Id;
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

            }
        }
        public IList<T> GetAllOfflineNotes()
        {
            T entity = new T();
            return ExecuteGet(string.Format("SELECT * FROM tbl_AddNotes where IsOffline = 0"));
        }
        public long Add(T entity)
        {

            long identity = 0;
            bool hasIdentity = false;

            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            IList<PropertyInfo> propertyInfos = GetPropertyInfoList(entity);

            foreach (PropertyInfo i in propertyInfos)
            {
                var ca = i.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;

                if (ca != null)
                {
                    if (!ca.IsIdentity)
                    {
                        columns.Append(string.Format("[{0}],", i.Name));
                        values.Append(string.Format("{0},",
                               i.GetValue(entity) == null ? "NULL" : string.Format("'{0}'", i.GetValue(entity))));
                    }
                    else
                    {
                        hasIdentity = true;
                    }
                }
            }

            if (columns.ToString() != string.Empty)
            {

                columns.Remove(columns.Length - 1, 1); // Remove additional comma(',')
                values.Remove(values.Length - 1, 1); // Remove additional comma(',')

                StringBuilder qry = new StringBuilder();
                qry.Append(string.Format("INSERT INTO [{0}] ( {1} ) VALUES ( {2} ); SELECT last_insert_rowid();"
                    , entity.GetType().Name, columns, values));


                identity = hasIdentity ? Execute(qry.ToString(), true) : Execute(qry.ToString());
            }

            return identity;
        }
        /// <summary>
        /// Inserts multiple records into a table
        /// </summary>
        /// <param name="entities"></param>
        public void AddRange(IList<T> entities)
        {
            StringBuilder qry = new StringBuilder();
            foreach (T entity in entities)
            {
                StringBuilder columns = new StringBuilder();
                StringBuilder values = new StringBuilder();

                IList<PropertyInfo> propertyInfos = GetPropertyInfoList(entity);

                foreach (PropertyInfo i in propertyInfos)
                {
                    var ca = i.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;

                    if (ca != null)
                    {
                        if (!ca.IsIdentity)
                        {
                            columns.Append(string.Format("[{0}],", i.Name));

                            values.Append(string.Format("{0},",
                                i.GetValue(entity) == null ? "NULL" : string.Format("'{0}'", i.GetValue(entity))));
                        }
                    }
                }

                if (columns.ToString() != string.Empty)
                {

                    columns.Remove(columns.Length - 1, 1); // Remove additional comma(',')
                    values.Remove(values.Length - 1, 1); // Remove additional comma(',')


                    qry.AppendLine(string.Format("INSERT INTO [{0}] ( {1} ) VALUES ( {2} );"
                        , entity.GetType().Name, columns, values));
                }
            }

            try
            {
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// Updates single entity
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            StringBuilder columns = new StringBuilder();
            StringBuilder clause = new StringBuilder();

            IList<PropertyInfo> propertyInfos = GetPropertyInfoList(entity);
            foreach (PropertyInfo i in propertyInfos)
            {
                var ca = i.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;

                if (ca != null)
                {
                    if (!ca.IsPrimary)
                    {
                        columns.Append(string.Format("[{0}] = {1},", i.Name,
                            i.GetValue(entity) == null ? "NULL" : string.Format("'{0}'", i.GetValue(entity))));
                    }
                    else
                    {
                        clause.Append(string.Format("[{0}] = '{1}'", i.Name, i.GetValue(entity)));
                    }
                }
            }

            if (columns.ToString() != string.Empty)
            {
                columns.Remove(columns.Length - 1, 1);
                StringBuilder qry = new StringBuilder();
                qry.Append(string.Format("UPDATE [{0}] SET {1} WHERE {2};"
                    , entity.GetType().Name, columns, clause));


                Execute(qry.ToString());
            }
        }

        public void UpdateIntervalToDB(string IntervalEndTime,string EndTime, long slotId, string CurrentDate)
        {
            try
            {

                string qry = "update tbl_KeyMouseTrack_Slot set  IntervalEndTime=" + "'" + IntervalEndTime + "'" + ", End=" + "'" + EndTime + "'" + " where Id=" + "" + slotId + "" + " and CreatedDate=" + "'" + CurrentDate + "'" + " ";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {


            }
        }

        
        public T CheckSlotExistNotFromDb(string Start,string CurrentDate)
        {
            T entity = new T();
            IList<T> entities = new List<T>();
            string qry = "Select * from tbl_KeyMouseTrack_Slot where Start=" + "'" + Start + "'" + " and CreatedDate=" + "" + CurrentDate + "" + "";
            var _entities = ExecuteGet(qry.ToString());
            if (_entities != null && _entities.Count > 0)
                entity = _entities[0];
            return entity;
        }
        public void UpdateToDoSyncTimeToLocalDB(string ToDoTimeConsumed, long ToDoId,string ProjectId)
        {
            try
            {

                string qry = "update tbl_ServerTodoDetails set IsOffline=true, ToDoTimeConsumed=" + "'" + ToDoTimeConsumed + "'" + " where Id=" + "" + ToDoId + " and CurrentProjectId=" + "'" + ProjectId + "'" + "";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {


            }
        }

        public void UpdateProjectSyncTimeToLocalDB(string ProjectTimeConsumed, long ProjectId)
        {
            try
            {
                string qry = "update tbl_Organisation_Projects set IsOffline=1, ProjectTimeConsumed=" + "'" + ProjectTimeConsumed + "'" + " where ProjectId=" + "'" + ProjectId + "'";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateStartStopProjectTimeToDB(string StopTime, long sno)
        {
            try
            {
                //TimeSpan diff = DateTime.Parse(StopTime) - DateTime.Parse(oldTime);
                //var minutes = diff.Seconds;
                //string qry = "update tbl_Timer set  Stop=" + "'" + StopTime + "'" + ", IntervalTime=" + "'" + minutes + "'" +" where ProjectId=" + "'" + ProjectId + "'" + " and OrgId=" + "'" + OrgId + "'" + " and Start=" + "'" + oldTime + "'" + "";
                //string qry = "update tbl_Timer set  Stop=" + "'" + StopTime + "'" +" where Sno=" + "'" + ProjectId + "'" + " and OrgId=" + "'" + OrgId + "'" + " and Start=" + "'" + oldTime + "'" + "";
                string qry = "update tbl_Timer set  Stop=" + "'" + StopTime + "'" + " where Sno=" + "" + sno + "";

                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public void UpdateProjectIdandToDoIdToDB(string StartTime, string IdleProjectId, int IdleToDoId, long sno)
        {
            try
            {
                string qry = "update tbl_Timer set Start=" + "'" + StartTime + "'" + ", ProjectId =" + "'" + IdleProjectId + "'" + ", ToDoId=" + "'" + IdleToDoId + "'" + " where Sno=" + "" + sno + "";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public void UpdateSlot_IdleStartTimeToDB(string StartTime, int sno)
        {
            try
            {
                string qry = "update tbl_KeyMouseTrack_Slot_Idle set Start=" + "'" + StartTime + "'" + " where Id=" + "" + sno + "";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public void updatetbl_ProjectDetails(string TotalWorkedHours, int ProjectId, int OrganizationId)
        {
           // string qry = "update tbl_Temp_SyncTimer set  TotalWorkedHours=" + "'" + TotalWorkedHours + "'" + " where SNo=" + "" + sNo + "";
            string qry = "update tbl_Temp_SyncTimer set  TotalWorkedHours=" + "'" + TotalWorkedHours + "'" + " where ProjectId=" + "" + ProjectId + " and OrganizationId=" + "" + OrganizationId + "";

            Execute(qry.ToString());
        }
        public T Gettbl_ProjectDetailsByIDs(int ProjectId, int OrganizationId, string CurrentDate)
        {
            T entity = new T();
            IList<T> entities = new List<T>();
            string qry = "Select * from tbl_Temp_SyncTimer where ProjectId=" + "" + ProjectId + " and OrganizationId=" + "" + OrganizationId + "" + " and CreatedDate=" + "'" + CurrentDate + "'" + "";
            var _entities = ExecuteGet(qry.ToString());
            if (_entities != null && _entities.Count > 0)
                entity = _entities[0];
            return entity;
        }

        public void UpdateProjectDetails(string ProjectId, string OrganizationId)
        {
                

            string qry = "update tbl_Organisation_Projects set IsOffline=0  where ProjectId=" + "'" + ProjectId + "'" + "and OrganisationId=" + "'" + OrganizationId + "'" + "";            
            Execute(qry.ToString());
        }

        public void UpdateTODODetails(string ProjectId, string OrganizationId, int todoId)
        {


            string qry = "update tbl_ServerTodoDetails set IsOffline=false  where CurrentProjectId=" + "'" + ProjectId + "'" + "and CurrentOrganisationId=" + "'" + OrganizationId + "'" + " and Id=" + "" + todoId + "" + "";
            Execute(qry.ToString());
        }
        public T Gettbl_Temp_SyncTimerByIDs(int ProjectId, int OrganizationId, string CurrentDate,int todoId)
        {
            T entity = new T();
            IList<T> entities = new List<T>();
            string qry = "Select * from tbl_Temp_SyncTimer where ProjectId=" + "" + ProjectId + " and OrganizationId=" + "" + OrganizationId + "" + " and CreatedDate=" + "'" + CurrentDate + "'" + " and TodoId=" + "" + todoId + "" + "";
            var _entities = ExecuteGet(qry.ToString());
            if (_entities != null && _entities.Count > 0)
                entity = _entities[0];
            return entity;
        }

        public T tbl_TempSyncTimerTodoDetails(int ProjectId, int OrganizationId, string CurrentDate, int todoId)
        {
            T entity = new T();
            IList<T> entities = new List<T>();
            string qry = "Select * from tbl_TempSyncTimerTodoDetails where ProjectId=" + "" + ProjectId + " and OrganizationId=" + "" + OrganizationId + "" + " and CreatedDate=" + "'" + CurrentDate + "'" + " and TodoId=" + "" + todoId + "" + "";
            var _entities = ExecuteGet(qry.ToString());
            if (_entities != null && _entities.Count > 0)
                entity = _entities[0];
            return entity;
        }

        public void Updatetbl_TempSyncTimerTodoDetails(string StopTime, int ProjectId, int OrganizationId, int todoId)
        {
            string qry = "update tbl_TempSyncTimerTodoDetails set  TotalWorkedHours=" + "'" + StopTime + "'" + " where ProjectId=" + "" + ProjectId + " and OrganizationId=" + OrganizationId + " and TodoId=" + todoId + "";

            Execute(qry.ToString());
        }
        public void Updatetbl_Temp_SyncTimer(string StopTime, long ProjectId, long OrganizationId,int todoId)
        {
            string qry = "update tbl_Temp_SyncTimer set  TotalWorkedHours=" + "'" + StopTime + "'" + " where ProjectId=" + "" + ProjectId + " and OrganizationId=" + OrganizationId + " and TodoId=" + todoId + "";

            Execute(qry.ToString());
        }
        public void UpdateProjectTimeToDBByToDoID(string StopTime, long sno)
        {
            try
            {
               
                string qry = "update tbl_Timer set  Stop=" + "'" + StopTime + "'" + " where Sno=" + "" + sno + "";
                Execute(qry.ToString());
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public void DeleteAppAndURL(long id)
        {
            string qry = "DELETE FROM tbl_AppAndUrl where Id=" + id;
            ExecuteGet(qry.ToString());
        }
        /// <summary>
        /// Updates mutiple entities in single query
        /// </summary>
        /// <param name="entities"></param>
        public void UpdateRange(IList<T> entities)
        {
            StringBuilder qry = new StringBuilder();
            foreach (T entity in entities)
            {
                StringBuilder columns = new StringBuilder();
                StringBuilder clause = new StringBuilder();


                #region MyRegion
                IList<PropertyInfo> propertyInfos = GetPropertyInfoList(entity);
                foreach (PropertyInfo i in propertyInfos)
                {
                    var ca = i.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;

                    if (ca != null)
                    {
                        if (!ca.IsPrimary)
                        {
                            columns.Append(string.Format("[{0}] = {1},", i.Name,
                                i.GetValue(entity) == null ? "NULL" : string.Format("'{0}'", i.GetValue(entity))));
                        }
                        else
                        {
                            clause.Append(string.Format("[{0}] = '{1}'", i.Name, i.GetValue(entity)));
                        }
                    }
                }

                if (columns.ToString() != string.Empty)
                {
                    columns.Remove(columns.Length - 1, 1);

                    qry.AppendLine(string.Format("UPDATE [{0}] SET {1} WHERE {2};"
                        , entity.GetType().Name, columns, clause));
                }
                #endregion
            }

            Execute(qry.ToString());
        }

        /// <summary>
        /// Find single item
        /// </summary>
        /// <param name="cmdText"></param>
        /// 
        public IList<T> SearchToDoByString(string sText, int projectid, int organization_id)
        {
            IList<T> entities = new List<T>();

            StringBuilder qry = new StringBuilder();
            qry.Append(string.Format("Select * from tbl_ServerTodoDetails where ToDoName like '%" + sText + "%' and CurrentProjectId = " + projectid + " and CurrentOrganisationId = " + organization_id + ""));
            entities = ExecuteGet(qry.ToString());
            return entities;
        }
        public IList<T> SearchProjectByString(string sText, int projectid, int organization_id)
        {
            IList<T> entities = new List<T>();

            StringBuilder qry = new StringBuilder();
            qry.Append(string.Format("Select * from tbl_Organisation_Projects where ProjectName like '%" + sText + "%' and OrganisationId = " + organization_id + ""));
            entities = ExecuteGet(qry.ToString());
            return entities;
        }
        public T GetById(object id, string DbColumnRequest = "")
        {
            T entity = new T();
            StringBuilder clause = new StringBuilder();

            IList<PropertyInfo> pInfos = GetPropertyInfoList(entity);

            foreach (var pi in pInfos)
            {
                if (!string.IsNullOrEmpty(DbColumnRequest))
                {
                    clause.Append(string.Format("[{0}]='{1}'", DbColumnRequest, id));
                    break;
                }
                else
                {
                    var pk = pi.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;
                    if (pk != null && pk.IsPrimary)
                    {
                        clause.Append(string.Format("[{0}]='{1}'", pi.Name, id));
                        break;
                    }
                }
            }

            if (clause.ToString() != string.Empty)
            {
                StringBuilder qry = new StringBuilder();
                qry.Append(string.Format("SELECT * FROM [{0}] WHERE {1}", entity.GetType().Name, clause));
                var _entities = ExecuteGet(qry.ToString());
                if (_entities != null && _entities.Count > 0)
                    entity = _entities[0];
            }


            return entity;
        }
        public IList<T> GetAllById(object id, string DbColumnRequest = "")
        {
            IList<T> entities = new List<T>();
            T entity = new T();
            StringBuilder clause = new StringBuilder();

            IList<PropertyInfo> pInfos = GetPropertyInfoList(entity);

            foreach (var pi in pInfos)
            {
                if (!string.IsNullOrEmpty(DbColumnRequest))
                {
                    clause.Append(string.Format("[{0}]='{1}'", DbColumnRequest, id));
                    break;
                }
                else
                {
                    var pk = pi.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;
                    if (pk != null && pk.IsPrimary)
                    {
                        clause.Append(string.Format("[{0}]='{1}'", pi.Name, id));
                        break;
                    }
                }

            }

            if (clause.ToString() != string.Empty)
            {
                StringBuilder qry = new StringBuilder();
                qry.Append(string.Format("SELECT * FROM [{0}] WHERE {1}", entity.GetType().Name, clause));
                entities = ExecuteGet(qry.ToString());
                //if (_entities != null && _entities.Count > 0)
                //    entity = _entities[0];
            }


            return entities;
        }
        public IList<T> Find(IEnumerable<object> ids)
        {
            IList<T> entities = new List<T>();
            StringBuilder clause = new StringBuilder();

            var entity = new T();
            IList<PropertyInfo> pInfos = GetPropertyInfoList(entity);

            foreach (var pi in pInfos)
            {
                var pk = pi.GetCustomAttribute(typeof(DbColumnAttribute)) as DbColumnAttribute;
                if (pk != null && pk.IsPrimary)
                {
                    string _ids = string.Empty;
                    foreach (var id in ids)
                    {
                        if (_ids != string.Empty)
                            _ids = _ids + ",";

                        _ids = _ids + id.ToString();
                    }

                    clause.Append(string.Format("[{0}] IN ({1})", pi.Name, _ids));
                    break;
                }
            }

            if (clause.ToString() != string.Empty)
            {
                StringBuilder qry = new StringBuilder();
                qry.Append(string.Format("SELECT * FROM [{0}] WHERE {1}", entity.GetType().Name, clause));
                entities = ExecuteGet(qry.ToString());
            }

            return entities;
        }
        public IList<T> Find(IFilter<T> filter)
        {
            T entity = new T();
            return ExecuteGet<T>(filter);
        }
        /// <summary>
        /// Get all records
        /// </summary>
        /// <param name="cmdText"></param>
        public IList<T> GetAll()
        {           
            T entity = new T();
            return ExecuteGet(string.Format("SELECT * FROM [{0}]", entity.GetType().Name));
        }
        /// <summary>
        /// Pass comman text to get values
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        public IList<T> GetAll(string commandText)
        {
            return ExecuteGet(commandText);
        }
        public IList<TEntity> GetAll<TEntity>(string commandText)
            where TEntity : class, new()
        {
            return ExecuteGet<TEntity>(commandText);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Execute Only ; No return
        /// </summary>
        /// <param name="cmdText"></param>
        //private void Execute(string cmdText)
        //{
        //    using (var connection = GetConnection())
        //    {
        //        connection.Open();
        //        SQLiteCommand cmd = new SQLiteCommand(cmdText, connection);
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        public void CheckConnectionState(SqliteConnection conn)
        {
            if (conn.State.HasFlag(System.Data.ConnectionState.Open))
            {
                conn.Close();
                conn.Open();
            }
            else
            {
                conn.Close();
                conn.Open();
            }
        }
        private long Execute(string cmdText, bool returnIdentity = false)
        {
            //using (var connection = new SqliteConnection(GetConnectionString()))
            //{
            //    connection.Open();
            //    using (var transaction = connection.BeginTransaction())
            //    {
            //        var insertcmd = connection.CreateCommand();
            //        insertcmd.CommandText = cmdText;
            //        insertcmd.ExecuteNonQuery();
            //        transaction.Commit();
            //    }
            //    return 1;
            //}
            using (var connection = GetConnection())
            {
                // connection.Open();
                CheckConnectionState(connection);
                 SqliteCommand cmd = new SqliteCommand(cmdText, connection);

                if (returnIdentity)
                {
                    return (long)cmd.ExecuteScalar();
                }
                else
                {
                    cmd.ExecuteNonQuery();
                    return 0;
                }
            }
        }
        /// <summary>
        /// Execute and get records. of the native type
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        private IList<T> ExecuteGet(string cmdText)
        {
           
            using (var connection = GetConnection())
            {
               // connection.Open();
                CheckConnectionState(connection);

                SqliteCommand cmd = new SqliteCommand(cmdText, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    return new EntityMapper().Map<T>(reader);
                }
            }
        }
        /// <summary>
        /// Get list of items by specifying the type
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        private IList<TEntity> ExecuteGet<TEntity>(string cmdText)
            where TEntity : class, new()
        {
            using (var connection = GetConnection())
            {
                // connection.Open();
                CheckConnectionState(connection);

                SqliteCommand cmd = new SqliteCommand(cmdText, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    return new EntityMapper().Map<TEntity>(reader);
                }
            }
        }
        /// <summary>
        /// Pass filter to get records in entity format
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        private IList<TEntity> ExecuteGet<TEntity>(IFilter<TEntity> filter)
            where TEntity : class, new()
        {
            using (var connection = GetConnection())
            {
                // connection.Open();
                CheckConnectionState(connection);

                SqliteCommand cmd = new SqliteCommand(filter.Query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    return new EntityMapper().Map<TEntity>(reader);
                }
            }
        }
        /// <summary>
        /// Pass SQLite reader to get the specified entity type
        /// when you are reading dataset or multiple records
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private IList<TEntity> ExecuteGet<TEntity>(SqliteDataReader reader)
            where TEntity : class, new()
        {
            return new EntityMapper().Map<TEntity>(reader);
        }
        private IList<PropertyInfo> GetPropertyInfoList(T entity)
        {
            return entity.GetType().GetProperties()
                .Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(DbColumnAttribute)) != null).ToList();
        }
        private IList<PropertyInfo> GetPropertyInfoList<TEntity>(TEntity entity)
        {
            return entity.GetType().GetProperties()
                .Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(DbColumnAttribute)) != null).ToList();
        }
        #endregion              
    }
    #endregion

    #region Cache/Storage
    public static class Context
    {
        public static string ConnectionString { get; set; }
    }
    #endregion
    #region Entity Mapper

    public class EntityMapper
    {
        // Complete
        public static T ChangeType<T>(object value)
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default(T);
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(value, t);
        }
        public IList<T> Map<T>(SqliteDataReader reader)
            where T : class, new()
        {
            IList<T> collection = new List<T>();
            while (reader.Read())
            {
                T obj = new T();
                foreach (PropertyInfo i in obj.GetType().GetProperties()
                    .Where(p => p.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(DbColumnAttribute)) != null).ToList())
                {

                    try
                    {
                        var ca = i.GetCustomAttribute(typeof(DbColumnAttribute));

                        if (ca != null)
                        {
                            if (((DbColumnAttribute)ca).Convert == true)
                            {
                                if (reader[i.Name] != DBNull.Value)
                                    i.SetValue(obj, Convert.ChangeType(reader[i.Name], i.PropertyType));
                            }
                            else
                            {
                                if (reader[i.Name] != DBNull.Value)
                                    i.SetValue(obj, Convert.ChangeType(reader[i.Name], i.PropertyType));
                                //    if(reader[i.Name].ToString()=="ToDoId")
                                //    {
                                //        i.SetValue(obj, Convert.ChangeType(reader[i.Name], i.PropertyType));
                                //    }
                                //else
                                //    {
                                //        i.SetValue(obj, Convert.ChangeType(reader[i.Name], i.PropertyType));
                                //    }

                                // i.SetValue(obj, reader[i.Name]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                       
                        Console.WriteLine(ex.Message);
                        Console.ReadLine();
#endif

#if !DEBUG
                        throw ex;
#endif
                    }
                }

                collection.Add(obj);
            }

            return collection;
        }

    }

    #endregion

    #region Interfaces

    public interface IFilter<T> where T : class, new()
    {
        string EntityName { get; }
        string Query { get; }

        void Add(Expression<Func<T, object>> memberExpression, object memberValue);
    }

    #endregion

    #region Filter
    public class Filter<T> : IFilter<T> where T : class, new()
    {

        public Filter()
        {
            _Query = new StringBuilder();
            EntityName = typeof(T).Name;
        }

        public void Add(Expression<Func<T, object>> memberExpression, object memberValue)
        {

            if (_Query.ToString() != string.Empty)
                _Query.Append(" AND ");

            _Query.Append(string.Format(" [{0}] = {1}", NameOf(memberExpression), memberValue == null ? "NULL" : string.Format("'{0}'", memberValue)));
        }

        public string EntityName { get; private set; }

        private readonly StringBuilder _Query;

        /// <summary>
        /// Returns SELECT statement with WHERE clause based on the expression passed; This is CommandText
        /// </summary>
        public string Query
        {
            get
            {
                return string.Format("SELECT * FROM [{0}] {1} {2};"
                    , EntityName
                    , _Query.ToString() == string.Empty ? string.Empty : "WHERE"
                    , _Query.ToString());
            }
        }

        private string NameOf(Expression<Func<T, object>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }

    #endregion
}
