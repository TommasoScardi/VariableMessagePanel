# Variable Message Panel or PMV
> A small project where a server handle messages stored on Access or SqLite DBMS and a Win cilent WPF application who performs the CRUD operation and decides which messages should be displayed and how many, all completed by a Arduino ESP32 client who displays the messages.

The server provide basic commands to perform the CRUD operation on the DB with XML format.
All three codes have a model of Message to fill with data who came from server, and the win client have a quite normal GUI to perform operation on the Messages Table.

The Arduino Sketch asks for new message every 5 sec and parse the incoming XML with [tinyxml2](https://github.com/leethomason/tinyxml2), a small C++ lib who does the job.
If the Arduino client, comparing current message and income message see that the received is newer overwrite it on the older and display in on the `MAX7219` display thanks to the `MD_Parola` lib for those displays.

*Disclamer*:for use it with Access DBMS you need the `Microsoft.ACE.OLEDB.12.0` driver; you can switch between Access and SqLite in the `.config` file in the server installation path.

### The `Client_Display_PMV_OLD` is the sketch for Arduino 
### The `Client_Terminal_PMV` is the WPF Win App

## [Download It](https://github.com/TommasoScardi/variable-message-panel/releases/tag/v0.1-alpha)
