using System;
using MySql.Data.MySqlClient;

namespace ElmahCore.MySql;

public static class CommandExtension
{
    public static MySqlCommand CheckTable(string databaseName)
    {
        var command = new MySqlCommand();
        command.CommandText =
            @"
            SELECT EXISTS (
               SELECT 1
               FROM   information_schema.tables 
               WHERE  table_schema = @DatabaseName
               AND    table_name = 'ELMAH_Error'
               );
            ";

        command.Parameters.Add(new MySqlParameter("@DatabaseName", databaseName));

        return command;
    }

    public static MySqlCommand CreateTable()
    {
        var command = new MySqlCommand();
        command.CommandText =
            @"
            CREATE TABLE ELMAH_Error
            (
                ErrorId		VARCHAR(64) NOT NULL,
                Application	VARCHAR(60) NOT NULL,
                Host 		VARCHAR(50) NOT NULL,
                Type		VARCHAR(100) NOT NULL,
                Source		VARCHAR(60)  NOT NULL,
                Message		VARCHAR(500) NOT NULL,
                User		VARCHAR(50)  NOT NULL,
                StatusCode	INT NOT NULL,
                TimeUtc		TIMESTAMP NOT NULL,
                Sequence	INT NOT NULL AUTO_INCREMENT,
                AllXml		TEXT NOT NULL,
                KEY(Sequence)
            );

            ALTER TABLE ELMAH_Error ADD CONSTRAINT PK_ELMAH_Error PRIMARY KEY (ErrorId);

            CREATE INDEX IX_ELMAH_Error_App_Time_Seq ON ELMAH_Error 
            (
                Application   ASC,
                TimeUtc       DESC,
                Sequence      DESC
            ) USING BTREE;
            ";

        return command;
    }

    public static MySqlCommand LogError(
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
        var command = new MySqlCommand();
        command.CommandText =
            @"
            /* elmah */
            INSERT INTO ELMAH_Error (ErrorId, Application, Host, Type, Source, Message, User, StatusCode, TimeUtc, AllXml)
            VALUES (@ErrorId, @Application, @Host, @Type, @Source, @Message, @User, @StatusCode, @TimeUtc, @AllXml)
            ";

        command.Parameters.Add(new MySqlParameter("ErrorId", id));
        command.Parameters.Add(new MySqlParameter("Application", appName));
        command.Parameters.Add(new MySqlParameter("Host", hostName));
        command.Parameters.Add(new MySqlParameter("Type", typeName));
        command.Parameters.Add(new MySqlParameter("Source", source));
        command.Parameters.Add(new MySqlParameter("Message", message));
        command.Parameters.Add(new MySqlParameter("User", user));
        command.Parameters.Add(new MySqlParameter("StatusCode", statusCode));
        command.Parameters.Add(new MySqlParameter("TimeUtc", time.ToUniversalTime()));
        command.Parameters.Add(new MySqlParameter("AllXml", xml));

        return command;
    }

    public static MySqlCommand GetErrorXml(string appName, Guid id)
    {
        var command = new MySqlCommand();
        command.CommandText =
            @"
            SELECT AllXml FROM ELMAH_Error 
            WHERE 
                Application = @Application 
                AND ErrorId = @ErrorId
            ";

        command.Parameters.Add(new MySqlParameter("Application", appName));
        command.Parameters.Add(new MySqlParameter("ErrorId", id));

        return command;
    }

    public static MySqlCommand GetErrorsXml(string appName, int errorIndex, int pageSize)
    {
        var command = new MySqlCommand();
        command.CommandText =
            @"
            SELECT ErrorId, AllXml FROM ELMAH_Error
            WHERE
                Application = @Application
            ORDER BY Sequence DESC
                LIMIT @limit
                OFFSET @offset
            ";

        var offset = errorIndex;
        command.Parameters.Add("@Application", MySqlDbType.String).Value = appName;
        command.Parameters.Add("@offset", MySqlDbType.Int32).Value = offset;
        command.Parameters.Add("@limit", MySqlDbType.Int32).Value = pageSize;
        return command;
    }

    public static MySqlCommand GetTotalErrorsXml(string appName)
    {
        var command = new MySqlCommand();
        command.CommandText = "SELECT COUNT(*) FROM ELMAH_Error WHERE Application = @Application";
        command.Parameters.Add(new MySqlParameter("@Application", appName));
        return command;
    }
}