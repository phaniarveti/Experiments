using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Reflection;
using umbraco.BusinessLogic.Utils;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Summary description for Log.
    /// </summary>
    public class Log
    {
        #region Statics
        private Interfaces.ILog _externalLogger;
        private bool _externalLoggerInitiated;

        internal Interfaces.ILog ExternalLogger
        {
            get
            {
                if (!_externalLoggerInitiated)
                {
                    _externalLoggerInitiated = true;
                    if (!string.IsNullOrEmpty(UmbracoSettings.ExternalLoggerAssembly) && !string.IsNullOrEmpty(UmbracoSettings.ExternalLoggerType))
                    {
                        try
                        {
                            var assemblyPath = IOHelper.MapPath(UmbracoSettings.ExternalLoggerAssembly);
                            _externalLogger = Assembly.LoadFrom(assemblyPath).CreateInstance(UmbracoSettings.ExternalLoggerType) as Interfaces.ILog;
                        }
                        catch (Exception ee)
                        {
							LogHelper.Error<Log>("Error loading external logger", ee);
                        }
                    }
                }

                return _externalLogger;
            }
        }


        #region Singleton

        public static Log Instance
        {
            get { return Singleton<Log>.Instance; }
        }

        #endregion

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Adds the specified log item to the log.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="user">The user adding the item.</param>
        /// <param name="nodeId">The affected node id.</param>
        /// <param name="comment">Comment.</param>
        public static void Add(LogTypes type, User user, int nodeId, string comment)
        {
            if (Instance.ExternalLogger != null)
            {
                Instance.ExternalLogger.Add(type, user, nodeId, comment);

                if (!UmbracoSettings.ExternalLoggerLogAuditTrail)
                {
                    AddLocally(type, user, nodeId, comment);
                }
            }
            else
            {
                if (!UmbracoSettings.EnableLogging) return;

                if (UmbracoSettings.DisabledLogTypes != null &&
                    UmbracoSettings.DisabledLogTypes.SelectSingleNode(String.Format("//logTypeAlias [. = '{0}']", type.ToString().ToLower())) == null)
                {
                    if (comment != null && comment.Length > 3999)
                        comment = comment.Substring(0, 3955) + "...";

                    if (UmbracoSettings.EnableAsyncLogging)
                    {
                        ThreadPool.QueueUserWorkItem(
                            delegate { AddSynced(type, user == null ? 0 : user.Id, nodeId, comment); });
                        return;
                    }

                    AddSynced(type, user == null ? 0 : user.Id, nodeId, comment);
                }
            }
        }

		[Obsolete("Use LogHelper to log exceptions/errors")]
        public void AddException(Exception ee)
        {
            if (ExternalLogger != null)
            {
                ExternalLogger.Add(ee);
            }
            else
            {
                var ex2 = ee;
                while (ex2 != null)
                {
                    ex2 = ex2.InnerException;
                }
				LogHelper.Error<Log>("An error occurred", ee);
            }
        }

        /// <summary>
        /// Adds the specified log item to the Umbraco log no matter if an external logger has been defined.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="user">The user adding the item.</param>
        /// <param name="nodeId">The affected node id.</param>
        /// <param name="comment">Comment.</param>
        public static void AddLocally(LogTypes type, User user, int nodeId, string comment)
        {
            if (comment.Length > 3999)
                comment = comment.Substring(0, 3955) + "...";

            if (UmbracoSettings.EnableAsyncLogging)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate { AddSynced(type, user == null ? 0 : user.Id, nodeId, comment); });
                return;
            }

            AddSynced(type, user == null ? 0 : user.Id, nodeId, comment);
        }

        /// <summary>
        /// Adds the specified log item to the log without any user information attached.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="nodeId">The affected node id.</param>
        /// <param name="comment">Comment.</param>
        public static void Add(LogTypes type, int nodeId, string comment)
        {
            Add(type, null, nodeId, comment);
        }

        /// <summary>
        /// Adds a log item to the log immidiately instead of Queuing it as a work item.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="comment">The comment.</param>
        public static void AddSynced(LogTypes type, int userId, int nodeId, string comment)
        {
            var logTypeIsAuditType = type.GetType().GetField(type.ToString()).GetCustomAttributes(typeof(AuditTrailLogItem), true).Length != 0;

            if (logTypeIsAuditType)
            {
                try
                {
                    SqlHelper.ExecuteNonQuery(
                        "insert into umbracoLog (userId, nodeId, logHeader, logComment) values (@userId, @nodeId, @logHeader, @comment)",
                        SqlHelper.CreateParameter("@userId", userId),
                        SqlHelper.CreateParameter("@nodeId", nodeId),
                        SqlHelper.CreateParameter("@logHeader", type.ToString()),
                        SqlHelper.CreateParameter("@comment", comment));
                }
                catch (Exception e)
                {
					LogHelper.Error<Log>("An error occurred adding an audit trail log to the umbracoLog table", e);
                }

				//Because 'Custom' log types are also Audit trail (for some wacky reason) but we also want these logged normally so we have to check for this:
				if (type != LogTypes.Custom)
				{
					return;
				}

            }

			//if we've made it this far it means that the log type is not an audit trail log or is a custom log.
			LogHelper.Info<Log>(
				"Redirected log call (please use Umbraco.Core.Logging.LogHelper instead of umbraco.BusinessLogic.Log) | Type: {0} | User: {1} | NodeId: {2} | Comment: {3}",
				() => type.ToString(), () => userId, () => nodeId.ToString(CultureInfo.InvariantCulture), () => comment);            
        }

        public List<LogItem> GetAuditLogItems(int NodeId)
        {
            if (UmbracoSettings.ExternalLoggerLogAuditTrail && ExternalLogger != null)
                return ExternalLogger.GetAuditLogReader(NodeId);
            
            return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, nodeId, logHeader, DateStamp, logComment from umbracoLog where nodeId = @id and logHeader not in ('open','system') order by DateStamp desc",
                SqlHelper.CreateParameter("@id", NodeId)));
        }

        public List<LogItem> GetLogItems(LogTypes type, DateTime sinceDate)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(type, sinceDate);
            
            return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", type),
                SqlHelper.CreateParameter("@dateStamp", sinceDate)));
        }

        public List<LogItem> GetLogItems(int nodeId)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(nodeId);
            
            return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where id = @id order by dateStamp desc",
                SqlHelper.CreateParameter("@id", nodeId)));
        }

        public List<LogItem> GetLogItems(User user, DateTime sinceDate)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(user, sinceDate);
            
            return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", sinceDate)));
        }

        public List<LogItem> GetLogItems(User user, LogTypes type, DateTime sinceDate)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(user, type, sinceDate);
            
            return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", type),
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", sinceDate)));
        }

        public static void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            if (Instance.ExternalLogger != null)
                Instance.ExternalLogger.CleanLogs(maximumAgeOfLogsInMinutes);
            else
            {
                try
                {
                    DateTime oldestPermittedLogEntry = DateTime.Now.Subtract(new TimeSpan(0, maximumAgeOfLogsInMinutes, 0));
                    var formattedDate = oldestPermittedLogEntry.ToString("yyyy-MM-dd HH:mm:ss");

                    SqlHelper.ExecuteNonQuery("delete from umbracoLog where datestamp < @oldestPermittedLogEntry and logHeader in ('open','system')",
                        SqlHelper.CreateParameter("@oldestPermittedLogEntry", formattedDate));

                    LogHelper.Info<Log>(string.Format("Log scrubbed.  Removed all items older than {0}", formattedDate));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString(), "Error");
                    Trace.WriteLine(e.ToString());
                }
            }
        }


        #region New GetLog methods - DataLayer layer compatible
        /// <summary>
        /// Gets a reader for the audit log.
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>A reader for the audit log.</returns>
        [Obsolete("Use the Instance.GetAuditLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetAuditLogReader(int NodeId)
        {
            return SqlHelper.ExecuteReader(
                "select u.userName as [User], logHeader as Action, DateStamp as Date, logComment as Comment from umbracoLog inner join umbracoUser u on u.id = userId where nodeId = @id and logHeader not in ('open','system') order by DateStamp desc",
                SqlHelper.CreateParameter("@id", NodeId));
        }

        /// <summary>
        /// Gets a reader for the log for the specified types.
        /// </summary>
        /// <param name="Type">The type of log message.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(LogTypes Type, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        }

        /// <summary>
        /// Gets a reader for the log of the specified node.
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(int NodeId)
        {
            return SqlHelper.ExecuteReader(
                "select u.userName, DateStamp, logHeader, logComment from umbracoLog inner join umbracoUser u on u.id = userId where nodeId = @id",
                SqlHelper.CreateParameter("@id", NodeId));
        }

        /// <summary>
        /// Gets a reader for the log for the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(User user, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        }

        /// <summary>
        /// Gets a reader of specific for the log for specific types and a specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="Type">The type of log message.</param>
        /// <param name="SinceDate">The since date.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(User user, LogTypes Type, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        }

        /// <summary>
        /// Gets a reader of specific for the log for specific types and a specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="type">The type of log message.</param>
        /// <param name="sinceDate">The since date.</param>
        /// <param name="numberOfResults">Number of rows returned</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        internal static IRecordsReader GetLogReader(User user, LogTypes type, DateTime sinceDate, int numberOfResults)
        {
            if (SqlHelper is umbraco.DataLayer.SqlHelpers.MySql.MySqlHelper)
            {
                return SqlHelper.ExecuteReader(
                    "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc limit " + numberOfResults,
                    SqlHelper.CreateParameter("@logHeader", type.ToString()),
                    SqlHelper.CreateParameter("@user", user.Id),
                    SqlHelper.CreateParameter("@dateStamp", sinceDate));
            }
            else
            {
                return SqlHelper.ExecuteReader(
                    "select top " + numberOfResults + " userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                    SqlHelper.CreateParameter("@logHeader", type.ToString()),
                    SqlHelper.CreateParameter("@user", user.Id),
                    SqlHelper.CreateParameter("@dateStamp", sinceDate));
            }
        }
        #endregion

        #endregion
    }

	public class LogItem
    {
        public int UserId { get; set; }
        public int NodeId { get; set; }
        public DateTime Timestamp { get; set; }
        public LogTypes LogType { get; set; }
        public string Comment { get; set; }

        public LogItem()
        {

        }

        public LogItem(int userId, int nodeId, DateTime timestamp, LogTypes logType, string comment)
        {
            UserId = userId;
            NodeId = nodeId;
            Timestamp = timestamp;
            LogType = logType;
            Comment = comment;
        }

        public static List<LogItem> ConvertIRecordsReader(IRecordsReader reader)
        {
            var items = new List<LogItem>();
            while (reader.Read())
            {
                items.Add(new LogItem(
                    reader.GetInt("userId"),
                    reader.GetInt("nodeId"),
                    reader.GetDateTime("DateStamp"),
                    ConvertLogHeader(reader.GetString("logHeader")),
                    reader.GetString("logComment")));
            }

            return items;

        }

        private static LogTypes ConvertLogHeader(string logHeader)
        {
            try
            {
                return (LogTypes)Enum.Parse(typeof(LogTypes), logHeader, true);
            }
            catch
            {
                return LogTypes.Custom;
            }
        }
    }

    public class AuditTrailLogItem : Attribute
    {
    }
}