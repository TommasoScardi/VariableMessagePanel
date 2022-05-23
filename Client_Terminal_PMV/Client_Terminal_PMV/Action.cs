using System;
using System.Collections.Generic;
using System.Xml;
using System.Net;

namespace Client_Terminal_PMV
{
    public static class Action
    {

        public static ActionType Parse(string recXml)
        {
            if (recXml == null)
                return ActionType.NotAction;

            recXml = recXml.Replace("<EOF>", null);
            XmlDocument xmlReader = new XmlDocument();
            xmlReader.LoadXml(recXml);

            XmlNode actionElem = xmlReader.DocumentElement.SelectSingleNode("/action");
            if (actionElem == null)
            {
                return ActionType.NotAction;
            }
            XmlNode typeElem = actionElem.SelectSingleNode("type");
            ActionType actionType;

            if (!Enum.TryParse(typeElem.InnerText, true, out actionType))
            {
                return ActionType.NotAction;
            }
            else
            {
                return actionType;
            }
        }
        public static ActionType Parse(string recXml, out XmlNode data)
        {
            if(recXml == null)
            {
                data = null;
                return ActionType.NotAction;
            }

            recXml = recXml.Replace("<EOF>", null);
            XmlDocument xmlReader = new XmlDocument();
            xmlReader.LoadXml(recXml);

            XmlNode actionElem = xmlReader.DocumentElement.SelectSingleNode("/action");
            if (actionElem == null)
            {
                data = null;
                return ActionType.NotAction;
            }
            XmlNode typeElem = actionElem.SelectSingleNode("type");
            ActionType actionType;

            if (!Enum.TryParse(typeElem.InnerText, true, out actionType))
            {
                data = null;
                return ActionType.NotAction;
            }
            else
            {
                data = actionElem.SelectSingleNode("data");
                return actionType;
            }
        }

        public static List<Models.ModelMessaggio> GetMessages(IPAddress serverIp)
        {
            string received = Connection.SendReceiveFromServer(serverIp, "<?xml version=\"1.0\" encoding=\"utf - 8\" ?><action><type>GetMessages</type></action>");
            XmlNode data;
            if (Parse(received, out data) != ActionType.Response)
            {
                throw new Exception("GetMessages: Errore nel gestire la richiesta lato server");
            }

            List<Models.ModelMessaggio> msg = new List<Models.ModelMessaggio>();

            foreach (XmlNode xmlNode in data.ChildNodes)
            {
                msg.Add(new Models.ModelMessaggio()
                {
                    IDMessaggio = int.Parse(xmlNode.ChildNodes[0].InnerText),
                    Visualizza = bool.Parse(xmlNode.ChildNodes[1].InnerText),
                    Testo = xmlNode.ChildNodes[2].InnerText
                });
            }
            return msg;
        }

        public static Models.ModelMessaggio AddMessage(IPAddress serverIp, Models.ModelMessaggio nuovoMsg)
        {
            string received = Connection.SendReceiveFromServer(serverIp, "<?xml version=\"1.0\" encoding=\"utf - 8\" ?><action><type>AddMessage</type><data>"+
                nuovoMsg.ToString() +
            "</data></action>");
            XmlNode data;
            if (Parse(received, out data) != ActionType.Response)
            {
                throw new Exception("AddMessage: Errore nel gestire la richiesta lato server");
            }
            nuovoMsg.IDMessaggio = int.Parse(data.InnerText);
            return nuovoMsg;
        }

        public static void EditMessage(IPAddress serverIp, Models.ModelMessaggio editedMsg)
        {
            string received = Connection.SendReceiveFromServer(serverIp, "<?xml version=\"1.0\" encoding=\"utf - 8\" ?><action><type>EditMessage</type><data>" +
                editedMsg.ToString() +
            "</data></action>");
            XmlNode data;
            if (Parse(received, out data) != ActionType.Response)
            {
                throw new Exception("EditMessage: Errore nel gestire la richiesta lato server");
            }
        }

        public static void MakeMessageToView(IPAddress serverIp, Models.ModelMessaggio editedMsg)
        {
            string received = Connection.SendReceiveFromServer(serverIp, "<?xml version=\"1.0\" encoding=\"utf - 8\" ?><action><type>MakeMessageToView</type><data>" +
                editedMsg.ToString() +
            "</data></action>");
            XmlNode data;
            if (Parse(received, out data) != ActionType.Response)
            {
                throw new Exception("MakeMessageToView: Errore nel gestire la richiesta lato server");
            }
        }

        public static void DeleteMessage(IPAddress serverIp, Models.ModelMessaggio deleteMsg)
        {
            string received = Connection.SendReceiveFromServer(serverIp, "<?xml version=\"1.0\" encoding=\"utf - 8\" ?><action><type>DeleteMessage</type><data>" +
                deleteMsg.ToString() +
            "</data></action>");
            XmlNode data;
            if (Parse(received, out data) != ActionType.Response)
            {
                throw new Exception("DeleteMessage: Errore nel gestire la richiesta lato server");
            }
        }
    }

    public enum ActionType
    {
        NotAction = -1,
        TestConn = 1,
        Response = 5,

        GetMessages = 11,
        GetMessagesToView = 12,

        AddMessage = 21,
        EditMessage = 22,
        MakeMessageToView = 23,
        DeleteMessage = 24
    }
}
