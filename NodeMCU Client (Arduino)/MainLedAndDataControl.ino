#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

#define WIFI_SSID "WifiSSID"
#define WIFI_PASSWORD "WifiPassword"

WiFiClient espClient;
String endpoint = "http://127.0.0.1:5000"; // API endpoint given by flask (the 127 one is local, the other one is external and is the one that must be used)
String userDB = "root";             // MySQL user
String passwordDB = "password";   // miMAC: MySQL password
String database = "dropByDropIOT";
float presionMin = 1, presionMax = 3;
float HumedadMin = 1, HumedadMax = 3;
float TemperaturaMin = 1, TemperaturaMax = 3;
float ConsumoAguaMin = 1, ConsumoAguaMax = 3;
float presionVal = 2;
float HumedadVal = 2;
float TemperaturaVal = 2;
float ConsumoAguaVal = 2;
bool flag1 = false;
bool flag2 = false;
bool flag3 = false;
bool flag4 = false;

void checkSensorValue(){

  if (presionVal < presionMin || presionVal > presionMax) {
    Serial.print("Presión ON  ");
    if(!flag1){
      enviarAlerta("Sensor presión fuera del umbral", 1);
      flag1 = true;
    }
    digitalWrite(D0, HIGH); // Encender LED
  } else {
    flag1 = false;
    Serial.print("Presión OFF  ");
    digitalWrite(D0, LOW); // Apagar LED después del delay
  }

  if (HumedadVal < HumedadMin || HumedadVal > HumedadMax) {
    Serial.print("Humedad ON  ");
    if(!flag2){
      enviarAlerta("Sensor humedad fuera del umbral", 2);
      flag2 = true;
    }
    digitalWrite(D1, HIGH); // Encender LED
  } else {
    flag2 = false;
    Serial.print("Humedad OFF  ");
    digitalWrite(D1, LOW); // Apagar LED después del delay
  }

  if (TemperaturaVal < TemperaturaMin || TemperaturaVal > TemperaturaMax) {
    Serial.print("Temperatura ON  ");
    if(!flag3){
      enviarAlerta("Sensor temperatura fuera del umbral", 3);
      flag3 = true;
    }
    digitalWrite(D2, HIGH); // Encender LED
  } else {
    flag3 = false;
    Serial.print("Temperatura OFF  ");
    digitalWrite(D2, LOW); // Apagar LED después del delay
  }

  if (ConsumoAguaVal < ConsumoAguaMin || ConsumoAguaVal > ConsumoAguaMax) {
    Serial.print("Consumo ON  ");
    if(!flag4){
      enviarAlerta("Sensor consumoAgua fuera del umbral", 4);
      flag4 = true;
    }
    digitalWrite(D3, HIGH); // Encender LED
  } else {
    flag4 = false;
    Serial.print("Consumo OFF  ");
    digitalWrite(D3, LOW); // Apagar LED después del delay
  }

  if(hayAlertas()){
    Serial.println("Alertas ON");
    digitalWrite(D5, HIGH);
  } else {
    Serial.println("Alertas OFF");
    digitalWrite(D5, LOW);
  }

}

void httpTest(){
  if(WiFi.status()== WL_CONNECTED){
  HTTPClient http;
  http.begin(espClient, endpoint + "/queries/test");
  http.addHeader("Content-Type", "application/json");
  int codigo_respuesta = http.GET();   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
  if(codigo_respuesta>0){
    Serial.println("Código HTTP ► " + String(codigo_respuesta));   //Print return code
    if(codigo_respuesta == 200){
      String cuerpo_respuesta = http.getString();
      Serial.println("El servidor respondió ▼ ");
      Serial.println(cuerpo_respuesta);
    }
  }else{
    Serial.print("Error enviando POST, código: ");
    Serial.println(codigo_respuesta);
    Serial.println(http.errorToString(codigo_respuesta).c_str());
  }
  http.end();  //libero recursos
  } else {
     Serial.println("Error en la conexión WIFI");
  }
}

bool hayAlertas(){
  DynamicJsonDocument doc(1024); // Objeto JSON
  if(WiFi.status()== WL_CONNECTED){   //Check WiFi connection status
    HTTPClient http;
    http.begin(espClient, endpoint + "/queries/activeUserHasAlerts");
    http.addHeader("Content-Type", "application/json");
    int codigo_respuesta = http.POST("{\"user\":\"" + userDB + "\",\"password\":\"" + passwordDB + "\",\"db_name\":\"" + database + "\"}");   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
    if(codigo_respuesta>0){
      String response = http.getString();
      DeserializationError error = deserializeJson(doc, response);
      if (error) {
        Serial.print(F("deserializeJson() failed: "));
        Serial.println(error.f_str());
      } else {
        String answer = doc["response"];
        if(answer == "true"){
          return true;
        } else{
          return false;
        }
      }
    }
    http.end();  //libero recursos
  }else{
     Serial.println("Error en la conexión WIFI");
  }
  return false;
}

DynamicJsonDocument readData(String query){
  DynamicJsonDocument doc(1024); // Objeto JSON
  if(WiFi.status()== WL_CONNECTED){   //Check WiFi connection status
    HTTPClient http;
    http.begin(espClient, endpoint + "/queries/requestData");
    http.addHeader("Content-Type", "application/json");
    int codigo_respuesta = http.POST("{\"user\":\"" + userDB + "\",\"password\":\"" + passwordDB + "\",\"db_name\":\"" + database + "\",\"query\":\"" + query + "\"}");   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
    if(codigo_respuesta>0){
      String response = http.getString();
      DeserializationError error = deserializeJson(doc, response);
      if (error) {
        Serial.print(F("deserializeJson() failed: "));
        Serial.println(error.f_str());
      } else {
        //String answer = doc["response"];
        //Serial.println(answer);
        // Serial.println("Se ha guardado la información exitosamente!");
      }
    }
    return doc;
    http.end();  //libero recursos
  }else{
     Serial.println("Error en la conexión WIFI");
  }
  return doc;
}

void enviarAlerta(String tipoAlerta, int idSensor){
  if(WiFi.status()== WL_CONNECTED){   //Check WiFi connection status
    HTTPClient http;
    http.begin(espClient, endpoint + "/queries/crearAlerta");
    http.addHeader("Content-Type", "application/json");
    int codigo_respuesta = http.POST("{\"user\":\"" + userDB + "\",\"password\":\"" + passwordDB + "\",\"db_name\":\"" + database + "\",\"tipoAlerta\":\"" + tipoAlerta + "\",\"ID_Sensor\":\"" + String(idSensor) + "\"}");   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
    if(codigo_respuesta>0){
      String response = http.getString();
      DynamicJsonDocument doc(1024); // Objeto JSON
      DeserializationError error = deserializeJson(doc, response);
      if (error) {
        Serial.print(F("deserializeJson() failed: "));
        Serial.println(error.f_str());
      } else {
        String answer = doc["response"];
        Serial.println(answer);
      }
    }
    http.end();  //libero recursos
  }else{
     Serial.println("Error en la conexión WIFI");
  }
}



void actualizarVariablesBaseDeDatos(){
  DynamicJsonDocument variablesSensor(1024);
  variablesSensor = readData("SELECT * FROM sensor");
  presionVal = atof(variablesSensor["response"][0]["valor"]);
  HumedadVal = atof(variablesSensor["response"][1]["valor"]);
  TemperaturaVal = atof(variablesSensor["response"][2]["valor"]);
  ConsumoAguaVal = atof(variablesSensor["response"][3]["valor"]);
}

void actualizarParametrosDesdeBaseDeDatos(){
  DynamicJsonDocument doc(1024); // Objeto JSON
    if(WiFi.status()== WL_CONNECTED){   //Check WiFi connection status
      HTTPClient http;
      http.begin(espClient, endpoint + "/users/getVariablesFromActiveUser");
      http.addHeader("Content-Type", "application/json");
      int codigo_respuesta = http.POST("{\"user\":\"" + userDB + "\",\"password\":\"" + passwordDB + "\",\"db_name\":\"" + database + "\"}");   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
      if(codigo_respuesta>0){
        String response = http.getString();
        DeserializationError error = deserializeJson(doc, response);
        if (error) {
          Serial.print(F("deserializeJson() failed: "));
          Serial.println(error.f_str());
        } else {
          String answer = doc["response"];
          presionMin = atof(doc["response"][0]["valorMin"]);
          presionMax = atof(doc["response"][0]["valorMax"]);
          HumedadMin = atof(doc["response"][1]["valorMin"]);
          HumedadMax = atof(doc["response"][1]["valorMax"]);
          TemperaturaMin = atof(doc["response"][2]["valorMin"]);
          TemperaturaMax = atof(doc["response"][2]["valorMax"]);
          ConsumoAguaMin = atof(doc["response"][3]["valorMin"]);
          ConsumoAguaMax = atof(doc["response"][3]["valorMax"]);
        }
      }      http.end();  //libero recursos
    }else{
      Serial.println("Error en la conexión WIFI");
    }
}

void writeData(String query){
  if(WiFi.status()== WL_CONNECTED){   //Check WiFi connection status
    HTTPClient http;
    http.begin(espClient, endpoint + "/queries/updateData");
    http.addHeader("Content-Type", "application/json");
    int codigo_respuesta = http.POST("{\"user\":\"" + userDB + "\",\"password\":\"" + passwordDB + "\",\"db_name\":\"" + database + "\",\"query\":\"" + query + "\"}");   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
    if(codigo_respuesta>0){
      String response = http.getString();
      DynamicJsonDocument doc(1024); // Objeto JSON
      DeserializationError error = deserializeJson(doc, response);
      if (error) {
        Serial.print(F("deserializeJson() failed: "));
        Serial.println(error.f_str());
      } else {
        String answer = doc["response"];
        Serial.println(answer);
      }
    }
    http.end();  //libero recursos
  }else{
     Serial.println("Error en la conexión WIFI");
  }
}

void setup_wifi() {
    delay(100);
    Serial.println();
    Serial.print("macAddress: ");
    Serial.println(WiFi.macAddress());  //mac:="Medium Access Control Address"
    //Iniciar por conectar con la red WiFi
    Serial.print("Conectando WiFi --> ");
    Serial.println(WIFI_SSID);
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        // Serial.print(".");
        Serial.println(WiFi.status());

    }
    randomSeed(micros());
    Serial.println();
    Serial.println("WiFi conectado!");
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
} // End setup_wifi()
 

void setup() {
  // put your setup code here, to run once:
  pinMode(D0,OUTPUT);
  pinMode(D1,OUTPUT);
  pinMode(D2,OUTPUT);
  pinMode(D3,OUTPUT);
  pinMode(D5,OUTPUT);
  Serial.begin(9600);
  setup_wifi();
  httpTest();
  actualizarParametrosDesdeBaseDeDatos();
  actualizarVariablesBaseDeDatos();
}

void loop() {
  actualizarParametrosDesdeBaseDeDatos();
  actualizarVariablesBaseDeDatos();
  checkSensorValue();
  delay(1000);
}