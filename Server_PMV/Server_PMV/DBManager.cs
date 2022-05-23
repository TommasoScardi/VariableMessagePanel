using System;
using System.Collections.Generic;

using System.Configuration;

using System.Data;
using System.Data.OleDb;

using Dapper;

using Server_PMV.Models;

namespace Server_PMV
{
    public static class DBManager
    {
        private static IDbConnection GetConnection()
        {
            DBMSType selDBMS;

            Enum.TryParse<DBMSType>(ConfigurationManager.AppSettings["DBMS"], out selDBMS);
            switch (selDBMS)
            {
                case DBMSType.Access:
                    return new OleDbConnection(ConfigurationManager.ConnectionStrings[DBMSType.Access.ToString()].ConnectionString);
                case DBMSType.SQLite:
                    return null;
                default:
                    return new OleDbConnection(ConfigurationManager.ConnectionStrings[DBMSType.Access.ToString()].ConnectionString);
            }
        }

        public static List<ModelMessaggio> GetMessaggi(bool soloDaVisualizzare = false)
        {
            string dbQuery = !soloDaVisualizzare ?
                "SELECT * FROM Messaggi;" :
                "SELECT IDMessaggio, Testo FROM Messaggi WHERE Visualizza = true;";
            return GetConnection().Query<ModelMessaggio>(dbQuery).AsList();
        }

        public static void AddMessaggio(ref ModelMessaggio newMsg)
        {
            GetConnection().Execute("INSERT INTO Messaggi(Visualizza, Testo) VALUES(@Visualizza, @Testo);",
                new { newMsg.Visualizza, newMsg.Testo });
            newMsg.IDMessaggio = GetConnection().QuerySingle<int>("SELECT TOP 1 IDMessaggio FROM Messaggi ORDER BY IDMessaggio DESC");
        }

        public static void EditMessage(ModelMessaggio newMsg, bool modificaVisualizza = false)
        {
            string dbQuery = !modificaVisualizza ?
                "UPDATE Messaggi SET Visualizza = @Visualizza, Testo = @Testo WHERE IDMessaggio = @IDMessaggio;" :
                "UPDATE Messaggi SET Visualizza = @Visualizza WHERE IDMessaggio = @IDMessaggio;";
            GetConnection().Execute(dbQuery,
                !modificaVisualizza ?
                    new { newMsg.Visualizza, newMsg.Testo, newMsg.IDMessaggio } :
                    new { newMsg.Visualizza, newMsg.IDMessaggio });
        }

        public static void DeleteMessage(ModelMessaggio newMsg)
        {
            GetConnection().Execute("DELETE FROM Messaggi WHERE IDMessaggio = @IDMessaggio;",
                new { newMsg.IDMessaggio });
        }
    }

    public enum DBMSType
    {
        Access,
        SQLite
    }
}
