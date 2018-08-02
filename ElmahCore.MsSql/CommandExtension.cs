using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ElmahCore.Sql
{
	internal static class CommandExtension
	{
		internal const int MaxAppNameLength = 60;

		public static SqlCommand LogError(
			Guid id,
			string appName,
			string hostName,
			string typeName,
			string source,
			string message,
			string user,
			int statusCode,
			DateTime time,
			string xml)
		{
			var command = new SqlCommand("ELMAH_LogError") {CommandType = CommandType.StoredProcedure};

			var parameters = command.Parameters;

			parameters.Add("@ErrorId", SqlDbType.UniqueIdentifier).Value = id;
			parameters.Add("@Application", SqlDbType.NVarChar, MaxAppNameLength).Value = appName;
			parameters.Add("@Host", SqlDbType.NVarChar, 30).Value = hostName;
			parameters.Add("@Type", SqlDbType.NVarChar, 100).Value = typeName;
			parameters.Add("@Source", SqlDbType.NVarChar, 60).Value = source;
			parameters.Add("@Message", SqlDbType.NVarChar, 500).Value = message;
			parameters.Add("@User", SqlDbType.NVarChar, 50).Value = user;
			parameters.Add("@AllXml", SqlDbType.NText).Value = xml;
			parameters.Add("@StatusCode", SqlDbType.Int).Value = statusCode;
			parameters.Add("@TimeUtc", SqlDbType.DateTime).Value = time;

			return command;
		}

		public static SqlCommand GetErrorXml(string appName, Guid id)
		{
			var command = new SqlCommand("ELMAH_GetErrorXml") {CommandType = CommandType.StoredProcedure};

			var parameters = command.Parameters;
			parameters.Add("@Application", SqlDbType.NVarChar, MaxAppNameLength).Value = appName;
			parameters.Add("@ErrorId", SqlDbType.UniqueIdentifier).Value = id;

			return command;
		}

		public static SqlCommand GetErrorsXml(string appName, int pageIndex, int pageSize)
		{
			var command = new SqlCommand("ELMAH_GetErrorsXml") {CommandType = CommandType.StoredProcedure};

			var parameters = command.Parameters;

			parameters.Add("@Application", SqlDbType.NVarChar, MaxAppNameLength).Value = appName;
			parameters.Add("@PageIndex", SqlDbType.Int).Value = pageIndex;
			parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
			parameters.Add("@TotalCount", SqlDbType.Int).Direction = ParameterDirection.Output;

			return command;
		}

		public static void GetErrorsXmlOutputs(SqlCommand command, out int totalCount)
		{
			Debug.Assert(command != null);

			totalCount = (int) command.Parameters["@TotalCount"].Value;
		}
	}
}