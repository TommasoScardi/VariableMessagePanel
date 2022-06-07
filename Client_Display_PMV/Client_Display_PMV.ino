#include <WiFi.h>
#include <tinyxml2.h>
#include <MD_Parola.h>
#include <MD_MAX72xx.h>
#include <SPI.h>

#define BOOT_MSG "PMV v0.1 Scardi"
#define DEBUG 0

//-------MSG CAPACITY CONFIGURATION
#define MSG_STORED 5
#define BUF_SIZE 76 //75 Char + trailing /0
//-------END MSG CAPACITY CONFIGURATION

// Define the number of devices we have in the chain and the hardware interface
// NOTE: These pin numbers will probably not work with your hardware and may
// need to be adapted
/*
+Display MATRIX MD_MAX72xx
  -GPIO: 15 CS_PIN
  -GPIO 18 CLK_PIN
  -GPIO 23 DIN_PIN
*/

//-------MATRIX DISPLAY HW CONFIGURATION
#define HARDWARE_TYPE MD_MAX72XX::FC16_HW
#define MAX_DEVICES 4
#define CLK_PIN   18
#define DATA_PIN  23
#define CS_PIN    15
//-------END MATRIX DISPLAY HW CONFIGURATION

using namespace tinyxml2;

//-------WIFI CONFIGURATION
const char* ssid     = "AndroidAPI";
const char* password = "puffi301";
const char* sIp     = "192.168.43.124";
uint16_t sPort = 11000;
const char* req = "<?xml version=\"1.0\" encoding=\"utf-8\"?><action><type>GetMessagesToView</type></action><EOF>";
const unsigned long connTimeout = 5000;
//-------END WIFI CONFIGURATION

//-------MATRIX DISPLAY LIB CONFIGURATION
// HARDWARE SPI
MD_Parola P = MD_Parola(HARDWARE_TYPE, CS_PIN, MAX_DEVICES);
// SOFTWARE SPI
//MD_Parola P = MD_Parola(HARDWARE_TYPE, DATA_PIN, CLK_PIN, CS_PIN, MAX_DEVICES);

uint8_t scrollSpeed = 28;    // default frame delay value
textEffect_t scrollEffect = PA_SCROLL_LEFT;
textPosition_t scrollAlign = PA_LEFT;
uint16_t scrollPause = 0000; // in milliseconds 2000
//-------END MATRIX DISPLAY LIB CONFIGURATION

//-------VARIABLES
char curMessage[MSG_STORED][BUF_SIZE] = { BOOT_MSG, "", "", "", "" };
int IdMsg;
int tempIdMsg;
const unsigned long msgUpdateInterval = 5000;
unsigned long lastTimeElapsed;
bool parseText = true;
//-------END VARIABLES

void GetMessage(const char* serverIp, uint16_t serverPort)
{
#if DEBUG
  Serial.print("Connecting Server PMV ");
  Serial.print(serverIp);
  Serial.print(":");
  Serial.println(serverPort);
#endif

  WiFiClient client;
  if (!client.connect(serverIp, serverPort)) {
#if DEBUG
    Serial.println("Connection Failed");
#endif
    return;
  }

  client.write(req);
  
  unsigned long timeElapsed = millis();
  while (client.available() == 0) {
    if (millis() - timeElapsed > connTimeout) {
#if DEBUG
      Serial.println(">>> Client Timeout !");
#endif
      client.flush();
      client.stop();
      return;
    }
  }

  String strRec;
  while (client.available() > 0)
  {
    char charRec = client.read();
    strRec += charRec;

    if(strRec.indexOf(String("<EOF>")) > 0)
    {
      strRec.remove(strRec.indexOf(String("<EOF>")));
#if DEBUG
      Serial.println(strRec);
      Serial.println("Closing connection");
#endif
      client.flush();
      client.stop();

#if DEBUG
      Serial.println("Parsing Messages");
#endif
      ParseXmlMessage(strRec.c_str());
    }
  }
}

void ParseXmlMessage(const char* xmlText)
{
  XMLDocument xmlDoc;
  /*
   * <?xml version="1.0" encoding="utf-8"?>
   * <action>
   *  <type>Response</type>
   *  <data length="1">
   *    <message>
   *      <IDMessaggio>1</IDMessagio>
   *      <Testo>
   *        BLA BLA BLA
   *      </Testo>
   *    </message>
   *  </data>
   * </action>
  */

  if(xmlDoc.Parse(xmlText) != XML_SUCCESS){
#if DEBUG
    Serial.println("Error parsing XML");  
#endif
    return;
  }

  XMLElement* xmlAction = xmlDoc.RootElement(); //<action>
  XMLElement* xmlActData = xmlAction->FirstChildElement("data"); //<data>
#if DEBUG
  Serial.println("Parsed <action> and <data> fields");  
#endif
  
#if DEBUG
  Serial.println("Parsing message");  
#endif
  XMLElement* xmlMsg = xmlActData->FirstChildElement("message"); //<message>
  char* msgText;

  if (xmlMsg->FirstChildElement("IDMessaggio")->QueryIntText(&tempIdMsg) != XML_SUCCESS)
  {
#if DEBUG
    Serial.println(">>> ERROR parsing IDMessaggio");  
#endif
    tempIdMsg = -1;
  }
  
#if DEBUG
  Serial.print(tempIdMsg);
#endif
  
  if(parseText)
  {
      strcpy(curMessage, xmlMsg->FirstChildElement("Testo")->GetText());
    
#if DEBUG
    Serial.print(" - ");
    Serial.print(msgText);
#endif
  }
  
#if DEBUG
  Serial.println(";");
#endif
  
#if DEBUG
    Serial.println("Message Parsed Successfully");  
#endif
  return;
}

void setup()
{
#if DEBUG
  Serial.begin(115200);

  Serial.print("Connecting to ");
  Serial.println(ssid);
#endif
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
#if DEBUG
    Serial.print(".");
#endif
  }

#if DEBUG
  Serial.println("");
  Serial.println("WiFi connected!");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
  Serial.print("ESP Mac Address: ");
  Serial.println(WiFi.macAddress());
  Serial.print("Subnet Mask: ");
  Serial.println(WiFi.subnetMask());
#endif

  GetMessage(sIp, sPort);
  parseText = false;
  IdMsg = tempIdMsg;
  
  P.begin();
  P.displayText(curMessage, scrollAlign, scrollSpeed, scrollPause, scrollEffect, scrollEffect);
#if DEBUG
    Serial.println("Display Init OK");  
#endif
  
  lastTimeElapsed = millis();
#if DEBUG
    Serial.println("SETUP OK - STARTING LOOP");  
#endif
}

void loop()
{
  if (P.displayAnimate())
  {  
    if (millis() - lastTimeElapsed > msgUpdateInterval)
    { 
      GetMessage(sIp, sPort);
      if (tempIdMsg != -1 && tempIdMsg != IdMsg)
      {
#if DEBUG
        Serial.println("New Message -> Start Update Messages");
#endif
        parseText = true;
        GetMessage(sIp, sPort);
        parseText = false;
        IdMsg = tempIdMsg;
#if DEBUG
        Serial.println("Messages Updated");
#endif
      }
    
#if DEBUG
      Serial.print(IdMsg);
      Serial.print(" - ");
      Serial.print(curMessage);
      Serial.println(";");
#endif
      lastTimeElapsed = millis();
      
      P.displayReset();
    }
  }
}
