using System;
using System.Collections.Generic;
using System.Xml;

namespace Server_PMV
{
    public class Action
    {
        const int TEXT_CHAR_MAX_LEN = 75;

        public static ActionType Parse(string recXml, out XmlNode data)
        {
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

        public static string GetMessages(bool soloDaVisualizzare = false) //GetMessagesToView
        {
            List<Models.ModelMessaggio> messages = DBManager.GetMessaggi(soloDaVisualizzare);
            string data = $"<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>Response</type> <data length=\"{messages.Count}\">";
            foreach (Models.ModelMessaggio message in messages)
            {
                data += !soloDaVisualizzare ? message.ToString() : message.ToTinyString();
            }
            data += "</data></action>";
            return data;
        }

        public static string AddMessage(XmlNode datiMsg)
        {
            XmlNode msg = datiMsg.SelectSingleNode("message");
            if (msg.SelectSingleNode("Testo").InnerText.Length > TEXT_CHAR_MAX_LEN)
            {
                throw new ArgumentException("Lunghezza campo testo maggiore di 75 caratteri");
            }
            Models.ModelMessaggio newMsg = new Models.ModelMessaggio()
            {
                Visualizza = bool.Parse(msg.SelectSingleNode("Visualizza").InnerText),
                Testo = msg.SelectSingleNode("Testo").InnerText
            };
            DBManager.AddMessaggio(ref newMsg);

            return "<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>Response</type> <data>" +
                /*ATTENZIONE AL FORMATO PER I CLIENT*/$"<IDMessaggio>{newMsg.IDMessaggio}</IDMessaggio>" +
                "</data></action>";
        }

        public static string EditMessage(XmlNode datiMsg)
        {
            XmlNode msg = datiMsg.SelectSingleNode("message");
            if (msg.SelectSingleNode("Testo").InnerText.Length > TEXT_CHAR_MAX_LEN)
            {
                throw new ArgumentException("Lunghezza campo testo maggiore di 75 caratteri");
            }
            Models.ModelMessaggio newMsg = new Models.ModelMessaggio()
            {
                IDMessaggio = int.Parse(msg.SelectSingleNode("IDMessaggio").InnerText),
                Visualizza = bool.Parse(msg.SelectSingleNode("Visualizza").InnerText),
                Testo = msg.SelectSingleNode("Testo").InnerText
            };
            DBManager.EditMessage(newMsg);

            return $"<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>Response</type> <data>OK</data></action>";
        }

        public static string MakeMessageToView(XmlNode datiMsg)
        {
            XmlNode msg = datiMsg.SelectSingleNode("message");
            Models.ModelMessaggio newMsg = new Models.ModelMessaggio()
            {
                IDMessaggio = int.Parse(msg.SelectSingleNode("IDMessaggio").InnerText),
                Visualizza = bool.Parse(msg.SelectSingleNode("Visualizza").InnerText),
            };
            DBManager.EditMessage(newMsg, true);

            return $"<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>Response</type> <data>OK</data></action>";
        }

        public static string DeleteMessage(XmlNode datiMsg)
        {
            XmlNode msg = datiMsg.SelectSingleNode("message");
            Models.ModelMessaggio newMsg = new Models.ModelMessaggio()
            {
                IDMessaggio = int.Parse(msg.SelectSingleNode("IDMessaggio").InnerText)
            };
            DBManager.DeleteMessage(newMsg);

            return $"<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>Response</type> <data>OK</data></action>";
        }

        public static string Send(string xmlResponse)
        {
            return xmlResponse + "<EOF>";
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
