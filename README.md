# DropByDrop-System
This project is a collaborative effort where I was responsible for all the software development using Unity, Flask RestAPI, and Arduino code. The hardware components and diagrams were developed by other team members. The aim was to create an integrated application that facilitates interaction between physical (NodeMCU ESP8266) and digital (Android App) components. The application's primary function is to monitor water consumption, utilizing simulated values from a potentiometer on the NodeMCU and user-defined parameters within the app.

## System Components

**Android Application**
- Utilized Unity Software for creating an application that interfaces with the API to fetch and record user-generated data.
  - **Libraries Used in Unity:**
    - LitJson: Handling JSON data in C#
    - TMPro: Utilization of advanced typography in Unity
    - UnityEngine.Networking: Sending and downloading data to/from the API

**Flask-based RestAPI**
- Implemented to enable communication between the system's components: database, app, and NodeMCUs.
  - **Libraries Used in Python:**
    - flask: Framework for defining routes
    - hashlib: Library for password encryption
    - MySQL Connector: Connection between MySQL and Python

**MySQL Database**
- Employed for storing and managing data related to user accounts, water consumption, sensor values and system alerts.

**Main NodeMCU**
- Used to simulate water consumption via a potentiometer, enabling users to adjust analog values reflected within the application.

**Water Consumption NodeMCU**
- Implemented for simulating the alert system and irregularities using LED sensors. These sensors fetched data from the database to determine when to activate or deactivate alerts.

**Libraries Used in Arduino (NodeMCU):**
- ESP8266WiFi.h: WiFi connectivity library for NodeMCU
- ESP8266HTTPClient.h: HTTP client library for NodeMCU
- ArduinoJson.h: Library for handling JSON data in NodeMCU

## Use Cases

### Pressure
- Detects irregularities in water pressure.
- Default pressure: 250 kilopascals, user-adjustable within the range of 150 - 300 kilopascals.
- Alerts triggered when pressure falls outside the accepted range, reflected in LED activation.

### Humidity
- Alerts triggered when humidity exceeds 60% or falls below 30%.
- User-defined threshold within the application.
- Alerts reflected in the database, NodeMCU LED, and the app's alert menu.

### Temperature
- Monitors water temperature within the range of 30°C to 50°C.
- User-adjustable within the application.
- Alerts triggered if temperature surpasses set limits, reflected in NodeMCU LED.

### Consumption
- Monitors water consumption using a potentiometer.
- Alerts generated when consumption exceeds 150 liters.
- Alerts reflected in the application and NodeMCU via specific LED activation.

### Alerts Management
- Detects and manages active alerts.
- LEDs activated when specific sensor alerts are triggered.
- User can dismiss alerts in the app, turning off the LED.

## Hardware Functionality Description (NodeMCU Expansion PCB)

The NodeMCU Expansion PCB utilizes two shift registers and a multiplexer. These components effectively control the 6 7-segment displays, minimizing microcontroller pin usage by receiving data serially but presenting it in parallel. Multiplexing technique enables control of active displays and segments. Dynamic display updates simulate simultaneous activation by alternating display activity.

The multiplexer handles multiple inputs (4 in this case) and 2 selection bits, enabling reading from a single analog input among 4 options. Logical gates facilitate effective multiplexer operation, managing multiple inputs while directing a specific signal to the output. Additionally, a potentiometer simulating a sensor input is connected to the multiplexer using voltage, ground, and analog cables.

## Required NodeMCU Programs for Implementation Testing

For implementation testing, Arduino programs with libraries such as ESP8266WiFi.h, ESP8266HTTPClient.h, and ArduinoJson.h are required. These libraries manage internet connectivity, HTTP requests, and JSON data handling, respectively. Additionally, a functioning database for data storage, and the Flask-based API code written in Python are necessary.

## Utility of Source Codes and Database Components

The Arduino code initiates internet connection and interacts with the database and API. It sets constants for network credentials, variables for database interaction, and defines pins for display control. Functions for updating displays, handling water consumption data via HTTP POST requests, and managing sensor alerts are included.

The MySQL database manages user, consumption, alert, sensor data, etc., interconnected through foreign keys for efficient data querying. Stored Procedures optimize data operations and ensure efficient data handling, crucial for the API functionality. These tables and procedures facilitate data updates from the database to the application.

The Flask-based API written in Python includes routes for testing API availability, database creation/deletion, table creation/deletion, data insertion, retrieval, update, and specialized routes for specific data insertion and active user alert verification.