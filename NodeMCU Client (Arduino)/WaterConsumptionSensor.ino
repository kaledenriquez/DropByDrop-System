#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

#define WIFI_SSID "WifiSSID"
#define WIFI_PASSWORD "WifiPassword"

#define bitSerial D0  //Salida de bit serial
#define SRCLK     D1  //Salida de reloj para el bitSerial
#define RCLK      D2  //Salida de reloj para el registro de 8 bits
#define SRCLR     D3
#define b0        D4   //Salida de b0 del mutiplexor
#define b1        D5   //Salida de b1 del multiplexor

byte macAddr[6];  //Dirección física del NodeMCU ("Medium Access Control")
unsigned int mskDisplays[]={0x7F,0xBF,0xDF,0xEF,0xF7,0xFB};
unsigned int mskDigitos[]={0x7E,0x18,0xB6,0xBC,0xD8,0xEC,0xEE,0x38,0xFE,0xF8};

//Variables para registrar la cantidad de agua
int cantConAgua = 0;
int contadorGeneral = 0;

//Variables para la selección de un sensor
int s0,s1,s2,s3,senSelect;
//Variables para la identificación de cada dígito de un número máximo de 6 dígitos
int digito5, digito4, digito3, digito2, digito1, digito0;  

WiFiClient espClient;
String endpoint = "http://127.0.0.1:5000"; // API endpoint given by flask (the 127 one is local, the other one is external and is the one that must be used)
String userDB = "root";             // MySQL user
String passwordDB = "password";   // miMAC: MySQL password
String database = "dropByDropIOT";

void actualizarConsumo(int cantidadAgua){
  if(WiFi.status()== WL_CONNECTED){   //Check WiFi connection status
    HTTPClient http;
    http.begin(espClient, endpoint + "/queries/insertarConsumo");
    http.addHeader("Content-Type", "application/json");
    int codigo_respuesta = http.POST("{\"user\":\"" + userDB + "\",\"password\":\"" + passwordDB + "\",\"db_name\":\"" + database + "\",\"consumoAgua\":\"" + String(cantidadAgua) + "\"}");   //Enviamos el post pasándole, los datos que queremos enviar. (esta función nos devuelve un código que guardamos en un int)
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

//Se va actualizando el valor del agua hacia la api
void uploadSensor4(int cantidadAgua){
  writeData("UPDATE Sensor SET valor =  " + String(cantidadAgua) + " WHERE ID_Sensor = 4");
  actualizarConsumo(cantidadAgua);
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

 //Función para mostrar el número de 1 dígitos en los 1 displays
void actualizaDisplays1() {
  int i;
  int dato;
  static int displayIndx=0;

  digitalWrite(RCLK,LOW);
  //-X-Xb2b3b4b5b6b7
  dato = mskDisplays[displayIndx];
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;
  }
  //abcdefgh
  //5432167x
  switch(displayIndx) {
      case 0: dato=mskDigitos[digito0];
            break;
  }
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;    
  }
  digitalWrite(RCLK,HIGH);

  if (++displayIndx>=6) {
    displayIndx=0;
  }
}
//Función para mostrar el número de 2 dígitos en los 2 displays
void actualizaDisplays2() {
  int i;
  int dato;
  static int displayIndx=0;

  digitalWrite(RCLK,LOW);
  //-X-Xb2b3b4b5b6b7
  dato = mskDisplays[displayIndx];
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;
  }
  //abcdefgh
  //5432167x
  switch(displayIndx) {
      case 0: dato=mskDigitos[digito0];
            break;
      case 1: dato=mskDigitos[digito1];
            break;
  }
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;    
  }
  digitalWrite(RCLK,HIGH);

  if (++displayIndx>=6) {
    displayIndx=0;
  }
}
//Función para mostrar el número de 3 dígitos en los 3 displays
void actualizaDisplays3() {
  int i;
  int dato;
  static int displayIndx=0;

  digitalWrite(RCLK,LOW);
  //-X-Xb2b3b4b5b6b7
  dato = mskDisplays[displayIndx];
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;
  }
  //abcdefgh
  //5432167x
  switch(displayIndx) {
      case 0: dato=mskDigitos[digito0];
            break;
      case 1: dato=mskDigitos[digito1];
            break;
      case 2: dato=mskDigitos[digito2];
            break;
  }
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;    
  }
  digitalWrite(RCLK,HIGH);

  if (++displayIndx>=6) {
    displayIndx=0;
  }
}
//Función para mostrar el número de 4 dígitos en los 4 displays
void actualizaDisplays4() {
  int i;
  int dato;
  static int displayIndx=0;

  digitalWrite(RCLK,LOW);
  //-X-Xb2b3b4b5b6b7
  dato = mskDisplays[displayIndx];
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;
  }
  //abcdefgh
  //5432167x
  switch(displayIndx) {
      case 0: dato=mskDigitos[digito0];
            break;
      case 1: dato=mskDigitos[digito1];
            break;
      case 2: dato=mskDigitos[digito2];
            break;
      case 3: dato=mskDigitos[digito3];
            break;
  }
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;    
  }
  digitalWrite(RCLK,HIGH);

  if (++displayIndx>=6) {
    displayIndx=0;
  }
}
//Función para mostrar el número de 5 dígitos en los 5 displays
void actualizaDisplays5() {
  int i;
  int dato;
  static int displayIndx=0;

  digitalWrite(RCLK,LOW);
  //-X-Xb2b3b4b5b6b7
  dato = mskDisplays[displayIndx];
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;
  }
  //abcdefgh
  //5432167x
  switch(displayIndx) {
      case 0: dato=mskDigitos[digito0];
            break;
      case 1: dato=mskDigitos[digito1];
            break;
      case 2: dato=mskDigitos[digito2];
            break;
      case 3: dato=mskDigitos[digito3];
            break;
      case 4: dato=mskDigitos[digito4];
            break;
  }
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;    
  }
  digitalWrite(RCLK,HIGH);

  if (++displayIndx>=6) {
    displayIndx=0;
  }
}
//Función para mostrar el número de 6 dígitos en los 6 displays
void actualizaDisplays6() {
  int i;
  int dato;
  static int displayIndx=0;

  digitalWrite(RCLK,LOW);
  //-X-Xb2b3b4b5b6b7
  dato = mskDisplays[displayIndx];
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;
  }
  //abcdefgh
  //5432167x
  switch(displayIndx) {
      case 0: dato=mskDigitos[digito0];
            break;
      case 1: dato=mskDigitos[digito1];
            break;
      case 2: dato=mskDigitos[digito2];
            break;
      case 3: dato=mskDigitos[digito3];
            break;
      case 4: dato=mskDigitos[digito4];
            break;
      case 5: dato=mskDigitos[digito5];
            break;
  }
  for (i=0;i<8;i++) {
    digitalWrite(SRCLK,LOW);
    if (dato&0x01) {
      digitalWrite(bitSerial,HIGH);
    } else {
      digitalWrite(bitSerial,LOW);
    }
    digitalWrite(SRCLK,HIGH);
    dato=dato>>1;    
  }
  digitalWrite(RCLK,HIGH);

  if (++displayIndx>=6) {
    displayIndx=0;
  }
}
//Proceso inicial y una sola vez
void setup() {
  Serial.begin(9600);
  pinMode(bitSerial,OUTPUT);
  pinMode(SRCLK,OUTPUT);
  pinMode(RCLK,OUTPUT);
  pinMode(SRCLR,OUTPUT);
  pinMode(b1,OUTPUT);
  pinMode(b0,OUTPUT);
  digitalWrite(SRCLR,LOW);
  digitalWrite(SRCLR,HIGH);
  digitalWrite(b1,LOW);
  digitalWrite(b0,LOW);
  setup_wifi();

  delay(10);
}
//Ciclo repetitivo
void loop() {
  static int cont=0;
  static int tiempoAnterior=0;
  int tiempoActual;
  if (++cont==1000) {
    digitalWrite(b1,LOW);
    digitalWrite(b0,LOW);
    Serial.print("Sensor 0 = ");
    s0 = analogRead(A0);
    Serial.println(s0);
    cantConAgua = map(s0,0,1024,0,300);
    Serial.print("Consumo de agua = ");
    uploadSensor4(cantConAgua);
    Serial.println(cantConAgua);
    digito5 = cantConAgua / 100000;
    digito4 = (cantConAgua % 100000) /10000;
    digito3 = (cantConAgua % 10000) / 1000;
    digito2 = (cantConAgua % 1000) / 100;
    digito1 = (cantConAgua % 100) / 10;
    digito0 = cantConAgua % 10;
    
    cont=0;
  }
  //Selección de los displays a utilizar
  if(cantConAgua < 10)
  {
    actualizaDisplays1();  
  }
  else if((cantConAgua>9) && (cantConAgua<100))
  {
    actualizaDisplays2();
  }
  else if((cantConAgua>99) && (cantConAgua<1000))
  {
    actualizaDisplays3();
  }
  else if((cantConAgua>999) && (cantConAgua<10000))
  {
    actualizaDisplays4();
  }
  else if((cantConAgua>9999) && (cantConAgua<100000))
  {
    actualizaDisplays5();
  }
  else if((cantConAgua>99999) && (cantConAgua<1000000))
  {
    actualizaDisplays6();
  }

  //Se realiza el ciclo cada 3 segundos
  while(millis()-tiempoAnterior < 3) ;
  tiempoAnterior= millis();
}