using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using LitJson;
using TMPro;

public class AppManager : MonoBehaviour
{
    string webRequestString; JsonData dataJson;
    public string userDB = "root", passwordDB = "Password", databaseName = "dropByDropIOT", ip = "127.0.0.1";
    public string userHash; public string userName;
    public delegate void MetodoParametro();

    void Start(){
        setInitialValues();
    }

    private IEnumerator CreateTables(MetodoParametro funcion){
        string url = "http://" + ip + ":5000/queries/createTables";
        string jsonData = "{\"user\":\"" + userDB +
                          "\",\"password\":\"" + passwordDB +
                          "\",\"db_name\":\"" + databaseName + "\"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Descarga de datos
                webRequestString = webRequest.downloadHandler.text;
                Debug.Log("Tablas creadas exitosamente");
                funcion.Invoke();
            } else {
                // Descarga de datos
                webRequestString = webRequest.downloadHandler.text;
                print("Error al crear las tablas: " + webRequestString);
            }
        }
    }

    public void LogIn(){
        if(ip == "nodata"){
            UIElements.Instance.loginPrompt.text = "Error al ingresar, actualize el valor de la IP en la configuración"; 
        } else {
            StartCoroutine(CreateTables(LoginAux));
        }
    }

    void LoginAux(){
        StartCoroutine(StartLogIn());
    }

    private IEnumerator StartLogIn(){
        string url = "http://" + ip + ":5000/users/login";
        string jsonData = "{\"appUser\":\"" + UIElements.Instance.LoginUserName.text +
                          "\",\"appPassword\":\"" + UIElements.Instance.LoginPassword.text +
                          "\",\"user\":\"" + userDB +
                          "\",\"password\":\"" + passwordDB +
                          "\",\"db_name\":\"" + databaseName + "\"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Descarga de datos
                webRequestString = webRequest.downloadHandler.text;
                Debug.Log("JSON: " + webRequestString);
                webRequestString = webRequest.downloadHandler.text;
                dataJson = JsonMapper.ToObject(webRequestString);
                // Asignar valores a las variables userHash y userName
                userHash = dataJson["userHash"].ToString();
                userName = UIElements.Instance.LoginUserName.text;
                // Mostrar el hash en la interfaz
                UIElements.Instance.loginPrompt.text = "Sesión iniciada exitosamente";
                UIElements.Instance.DashboardTitle.text = "BIENVENIDO " + userName.ToUpper();
                UIElements.Instance.JsonResponse.text = "Hash: " + userHash;
                UIElements.Instance.settingsPrompt.text = "Configure el usuario y contraseña de su base de datos así como la dirección IP de la API";
                UIElements.Instance.createPrompt.text = "Ingrese los datos para crear su cuenta";
                // Empezar la secuencia de login
                UIElements.Instance.LogInMenu.SetActive(false);
                UIElements.Instance.AnimationMenu.SetActive(true);
                StartCoroutine(setActiveUser());
                Invoke("openDashboard", 5.9f);
            } else {
                // Descarga de datos
                webRequestString = webRequest.downloadHandler.text;
                print(webRequestString);
                dataJson = JsonMapper.ToObject(webRequestString);
                // Obtener el valor de la llave "error" del JSON y mostrarlo en la interfaz
                try{
                    string error = dataJson["error"].ToString();
                    if (error == "User or password incorrect"){
                        UIElements.Instance.loginPrompt.text = "Error al iniciar sesión, el usuario o la contraseña son incorrectos";
                    } else {
                        UIElements.Instance.loginPrompt.text = "Error de conexión, asegúrese que las credenciales de la base de datos sean correctas";
                    }
                } catch {
                    UIElements.Instance.loginPrompt.text = "Error de conexión con la API, asegúrese que la IP sea correcta";
                }
            }
        }
    }

    private IEnumerator StartCreateAccount(){
        string url = "http://" + ip + ":5000/users/createAccount";
        string jsonData = "{\"appUser\":\"" + UIElements.Instance.CreateUserName.text +
                          "\",\"appPassword\":\"" + UIElements.Instance.CreatePassword.text +
                          "\",\"user\":\"" + userDB +
                          "\",\"password\":\"" + passwordDB +
                          "\",\"db_name\":\"" + databaseName + "\"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                webRequestString = webRequest.downloadHandler.text;
                Debug.Log("JSON: " + webRequestString);
                UIElements.Instance.createPrompt.text = "Cuenta creada exitosamente";
            }
            else
            {
                webRequestString = webRequest.downloadHandler.text;
                print(webRequestString);
                // Convertir el JSON a un objeto JsonData
                dataJson = JsonMapper.ToObject(webRequestString);
                // Obtener el valor de la llave "error" del JSON
                try{
                    string error = dataJson["error"].ToString();
                    // Mostrar el error en la interfaz
                    if (error == "User already exists"){
                        UIElements.Instance.createPrompt.text = "El usuario ya existe, intente con otro nombre de usuario";
                    } else {
                        UIElements.Instance.createPrompt.text = "Error de conexión, asegúrese que las credenciales de la base de datos sean correctas";
                    }
                } catch {
                    UIElements.Instance.createPrompt.text = "Error de conexión con la API, asegúrese que la IP sea correcta";
                }

            }
        }
    }
 
    public void CreateAccount(){
        if(UIElements.Instance.CreateUserName.text == "" || UIElements.Instance.CreatePassword.text == "" || UIElements.Instance.CreateConfirmPassword.text == ""){
            UIElements.Instance.createPrompt.text = "Error al crear la cuenta, asegúrese de llenar todos los campos";
        } else if (UIElements.Instance.CreatePassword.text != UIElements.Instance.CreateConfirmPassword.text){
            UIElements.Instance.createPrompt.text = "Error al crear la cuenta, las contraseñas no coinciden";
        } else {
            StartCoroutine(CreateTables(CreateAccountAux));

        }
    }

    private void CreateAccountAux(){
        StartCoroutine(StartCreateAccount());
    }

    private IEnumerator getQueryAndExecute(string query, MetodoParametro funcion){
        string url = "http://" + ip + ":5000/queries/requestData";
        // Json data que contiene las variables user, password, db_name y query
        string jsonData = "{\"user\":\"" + userDB +
                          "\",\"password\":\"" + passwordDB +
                          "\",\"db_name\":\"" + databaseName +
                          "\",\"query\":\"" + query + "\"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData, "application/json")){
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success){
                webRequestString = webRequest.downloadHandler.text;
                dataJson = JsonMapper.ToObject(webRequestString);
                funcion.Invoke();
            } else {
                webRequestString = webRequest.downloadHandler.text;
                print(webRequestString);
                // Convertir el JSON a un objeto JsonData
                dataJson = JsonMapper.ToObject(webRequestString);
                // Obtener el valor de la llave "error" del JSON
                try{
                    string error = dataJson["error"].ToString();
                    // Mostrar el error en la interfaz
                    Debug.Log("Error al realizar la consulta: " + error);
                } catch {
                    Debug.Log("Error de conexión con la API, asegúrese que la IP sea correcta");
                }
            }
        }
    }

    private IEnumerator updateQueryAndExecute(string query, MetodoParametro funcion){
        string url = "http://" + ip + ":5000/queries/updateData";
        // Json data que contiene las variables user, password, db_name y query
        string jsonData = "{\"user\":\"" + userDB +
                          "\",\"password\":\"" + passwordDB +
                          "\",\"db_name\":\"" + databaseName +
                          "\",\"query\":\"" + query + "\"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData, "application/json")){
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success){
                webRequestString = webRequest.downloadHandler.text;
                dataJson = JsonMapper.ToObject(webRequestString);
                funcion.Invoke();
            } else {
                webRequestString = webRequest.downloadHandler.text;
                print(webRequestString);
                // Convertir el JSON a un objeto JsonData
                dataJson = JsonMapper.ToObject(webRequestString);
                // Obtener el valor de la llave "error" del JSON
                try{
                    string error = dataJson["error"].ToString();
                    // Mostrar el error en la interfaz
                    Debug.Log("Error al realizar la consulta: " + error);
                } catch {
                    Debug.Log("Error de conexión con la API, asegúrese que la IP sea correcta");
                }
            }
        }
    }

    private IEnumerator setActiveUser(){
        string url = "http://" + ip + ":5000/users/setActiveUser";
        // Json data que contiene las variables user, password, db_name y userHash
        string jsonData = "{\"user\":\"" + userDB +
                          "\",\"password\":\"" + passwordDB +
                          "\",\"db_name\":\"" + databaseName +
                          "\",\"userHash\":\"" + userHash + "\"}";
        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData, "application/json")){
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success){
                webRequestString = webRequest.downloadHandler.text;
                dataJson = JsonMapper.ToObject(webRequestString);
                Debug.Log(dataJson["response"].ToString());
            } else {
                webRequestString = webRequest.downloadHandler.text;
                print(webRequestString);
                // Convertir el JSON a un objeto JsonData
                dataJson = JsonMapper.ToObject(webRequestString);
                // Obtener el valor de la llave "error" del JSON
                try{
                    string error = dataJson["error"].ToString();
                    // Mostrar el error en la interfaz
                    Debug.Log("Error al realizar la consulta: " + error);
                } catch {
                    Debug.Log("Error de conexión con la API, asegúrese que la IP sea correcta");
                }
            }
        }
    }

    public void setSensor(string sensorID){
        
    }

    // Métodos para abrir los menús
    public void MainMenu(){
        // Desactivar todos los menús y activar el menú principal
        UIElements.Instance.LogInMenu.SetActive(true); UIElements.Instance.CreateAccountMenu.SetActive(false); UIElements.Instance.DashBoardMenu.SetActive(false); UIElements.Instance.SettingsMenu.SetActive(false);
        UIElements.Instance.settingsPrompt.text = "Configure el usuario y contraseña de su base de datos así como la dirección IP de la API";
        UIElements.Instance.createPrompt.text = "Ingrese los datos para crear su cuenta";
        UIElements.Instance.loginPrompt.text = "Ingrese sus datos para iniciar la sesión";
    }
    
    public void openDashboard(){
        UIElements.Instance.loginPrompt.text = "Ingrese sus datos para iniciar la sesión";
        UIElements.Instance.AnimationMenu.SetActive(false);
        UIElements.Instance.DashBoardMenu.SetActive(true);
    }
    
    public void ingresarMenuCrearCuenta(){
        UIElements.Instance.LogInMenu.SetActive(false);
        UIElements.Instance.CreateAccountMenu.SetActive(true);
    }
    
    public void ingresarMenuConfiguracion(){
        UIElements.Instance.LogInMenu.SetActive(false);
        UIElements.Instance.SettingsMenu.SetActive(true);
    }

    // Métodos para abrir los menús del dashboard

    public void ingresarMenuAlertas(){
        UIElements.Instance.menuAlertas.SetActive(true);
        UIElements.Instance.DashBoardMenu.SetActive(false);
        StartCoroutine(getQueryAndExecute("SELECT A.ID_Alerta, A.hora, A.tipoAlerta, S.tipo AS tipoSensor FROM Alerta AS A INNER JOIN Sensor AS S ON S.ID_Sensor = A.ID_Sensor WHERE UserHash = '"+ userHash + "'", generarTablaAlertas));
    }

    private void generarTablaAlertas(){
        Debug.Log("TablaJSON: " + webRequestString);
        UIElements.Instance.tablaAlertas.generateTable(webRequestString, new List<int>(){0,1,3,2});
    }

    public void limpiarAlertas(){
        StartCoroutine(updateQueryAndExecute("DELETE FROM Alerta WHERE UserHash = '"+ userHash + "'", limpiarAlertasPass));
    }

    void limpiarAlertasPass(){
        ingresarMenuAlertas();
        StartCoroutine(getQueryAndExecute("SELECT A.ID_Alerta, A.hora, A.tipoAlerta, S.tipo AS tipoSensor FROM Alerta AS A INNER JOIN Sensor AS S ON S.ID_Sensor = A.ID_Sensor WHERE UserHash = '"+ userHash + "'", printjson));
    }

    public void ingresarMenuConsumo(){
        UIElements.Instance.menuConsumo.SetActive(true);
        UIElements.Instance.DashBoardMenu.SetActive(false);
        StartCoroutine(getQueryAndExecute("SELECT C.ID_Consumo, C.consumoAgua, C.hora FROM consumoagua AS C WHERE UserHash = '"+ userHash + "'", generarTablaConsumo));
    }

    private void generarTablaConsumo(){
        Debug.Log("TablaJSON: " + webRequestString);
        UIElements.Instance.tablaConsumo.generateTable(webRequestString, new List<int>(){0,1,2});
    }

    public void limpiarConsumo(){
        StartCoroutine(updateQueryAndExecute("DELETE FROM consumoagua WHERE UserHash = '"+ userHash + "'", limpiarConsumoPass));
    }

    void limpiarConsumoPass(){
        ingresarMenuConsumo();
        StartCoroutine(getQueryAndExecute("SELECT C.ID_Consumo, C.consumoAgua, C.hora FROM consumoagua AS C WHERE UserHash = '"+ userHash + "'", generarTablaConsumo));
    }

    public void ingresarMenuConfiguraciones(){
        UIElements.Instance.menuConfiguraciones.SetActive(true);
        UIElements.Instance.DashBoardMenu.SetActive(false);
        StartCoroutine(getQueryAndExecute("SELECT C.ID_Sensor, C.valorMin, C.valorMax, S.tipo FROM Configuracion AS C INNER JOIN Sensor AS S ON S.ID_Sensor = C.ID_Sensor WHERE UserHash = '"+ userHash + "'", setInitialConfigurationValues));
    }

    public void ingresarMenuSimular(){
        UIElements.Instance.menuSimular.SetActive(true);
        UIElements.Instance.DashBoardMenu.SetActive(false);
    }

    public void ingresarMenuMonitorear(){
        UIElements.Instance.menuMonitorear.SetActive(true);
        UIElements.Instance.DashBoardMenu.SetActive(false);
        StartCoroutine(getQueryAndExecute("SELECT S.ID_Sensor, S.tipo, S.valor FROM Sensor AS S", generarTablaMonitorear));
    }

    private void generarTablaMonitorear(){
        Debug.Log("TablaJSON: " + webRequestString);
        UIElements.Instance.tablaMonitorear.generateTable(webRequestString, new List<int>(){0,1,2});
    }

    public void ingresarMenuOtros(){
        UIElements.Instance.menuOtros.SetActive(true);
        UIElements.Instance.DashBoardMenu.SetActive(false);
        StartCoroutine(getQueryAndExecute("SELECT nombreAsistente FROM AsistenteVirtual WHERE UserHash = '"+ userHash + "'", getAsistenteVirtual));
    }

    // Métodos para cerrar los menús del dashboard

    public void volverAlDashboard(){
        UIElements.Instance.menuAlertas.SetActive(false);
        UIElements.Instance.menuConsumo.SetActive(false);
        UIElements.Instance.menuConfiguraciones.SetActive(false);
        UIElements.Instance.menuSimular.SetActive(false);
        UIElements.Instance.menuMonitorear.SetActive(false);
        UIElements.Instance.menuOtros.SetActive(false);
        UIElements.Instance.DashBoardMenu.SetActive(true);
        UIElements.Instance.SimularPrompt.text = "Actualiza los valores de los sensores";
    }

    // Configuración inicial de valores de la base de datos y la IP al iniciar el programa
    private void setInitialValues(){
        userDB = PlayerPrefs.GetString("userDB");
        passwordDB = PlayerPrefs.GetString("passwordDB");
        databaseName = "dropByDropIOT";
        ip = PlayerPrefs.GetString("ip");
        if(userDB == ""){userDB = "root";}
        if(passwordDB == ""){passwordDB = "Password";}
        if(ip == ""){ip = "nodata";}
        UIElements.Instance.CurrentUserDB.text = "Usuario: " + userDB;
        UIElements.Instance.CurrentPasswordDB.text = "Contraseña: " + passwordDB;
        UIElements.Instance.CurrentIPAddress.text = "IP: " + ip;
    }
    
    // Método para actualizar las credenciales de la base de datos
    public void updateDBCredentials(){
        bool flag = false;
        if (UIElements.Instance.DBUser.text != ""){
            userDB = UIElements.Instance.DBUser.text;
            UIElements.Instance.CurrentUserDB.text = "Usuario: " + userDB;
            UIElements.Instance.DBUser.text = "";
            flag = true;
            PlayerPrefs.SetString("userDB", userDB);
        }

        if (UIElements.Instance.DBPassword.text != ""){
            passwordDB = UIElements.Instance.DBPassword.text;
            UIElements.Instance.CurrentPasswordDB.text = "Contraseña: " + passwordDB;
            UIElements.Instance.DBPassword.text = "";
            flag = true;
            PlayerPrefs.SetString("passwordDB", passwordDB);
        }

        if (UIElements.Instance.IPAddress.text != ""){
            ip = UIElements.Instance.IPAddress.text;
            UIElements.Instance.CurrentIPAddress.text = "IP: " + ip;
            UIElements.Instance.IPAddress.text = "";
            flag = true;
            PlayerPrefs.SetString("ip", ip);
        }

        if (flag){
            UIElements.Instance.settingsPrompt.text = "Datos actualizados exitosamente";
        } else {
            UIElements.Instance.settingsPrompt.text = "Error al actualizar los datos, asegúrese de ingresar al menos un dato";
        }

    }

    void printjson(){
        Debug.Log("JSON: " + webRequestString);
    }

    void simulateSensorPass(){
        UIElements.Instance.SimularPrompt.text = "Sensor actualizado exitosamente";
    }

    void virtualAssistantPass(){
        UIElements.Instance.OtrosPrompt.text = "Asistente virtual actualizado exitosamente";
        UIElements.Instance.OtrosPrompt2.text = "Asistente virtual actual: " + UIElements.Instance.asistenteVirtualInput.text;
    }

    public void setAsistenteVirtual(){
        try{
            string asistenteVirtual = UIElements.Instance.asistenteVirtualInput.text;
            StartCoroutine(updateQueryAndExecute("UPDATE AsistenteVirtual SET nombreAsistente = '" + asistenteVirtual + "' WHERE UserHash = '" + userHash + "'", virtualAssistantPass));
        } catch {
            Debug.Log("Error al actualizar el asistente virtual, asegúrese de ingresar un nombre");
            UIElements.Instance.OtrosPrompt.text = "Error al actualizar el asistente virtual, asegúrese de ingresar un dato";
        }
    }

    void getAsistenteVirtual(){
        try{
            printjson();
            Debug.Log(dataJson);
            string asistenteVirtual = dataJson["response"][0]["nombreAsistente"].ToString();
            UIElements.Instance.OtrosPrompt2.text = "Asistente virtual actual: " + asistenteVirtual;
        } // cacha la excepción y la imprime en un debug log que diga error: 
        catch(System.Exception e){
            Debug.Log("Error" + e);
        }
        
    }

    void configurateSensorPass(){
        UIElements.Instance.ConfiguracionesPrompt.text = "Valores actualizados exitosamente";
    }

    void setInitialConfigurationValues(){
        try{
            UIElements.Instance.sensor1Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text = dataJson["response"][0]["valorMin"].ToString();
            UIElements.Instance.sensor1Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text = dataJson["response"][0]["valorMax"].ToString();
            UIElements.Instance.sensor2Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text = dataJson["response"][1]["valorMin"].ToString();
            UIElements.Instance.sensor2Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text = dataJson["response"][1]["valorMax"].ToString();
            UIElements.Instance.sensor3Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text = dataJson["response"][2]["valorMin"].ToString();
            UIElements.Instance.sensor3Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text = dataJson["response"][2]["valorMax"].ToString();
            UIElements.Instance.sensor4Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text = dataJson["response"][3]["valorMin"].ToString();
            UIElements.Instance.sensor4Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text = dataJson["response"][3]["valorMax"].ToString();
        } catch {
            Debug.Log("Error al obtener los valores de configuración de los sensores");
        }
    }

    public void setNormalValues(int sensor){
        float valorMinimo;
        float valorMaximo;
        try{
            if(sensor == 1){
                Debug.Log(UIElements.Instance.sensor1Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text);
                Debug.Log(UIElements.Instance.sensor1Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text);
                valorMinimo = float.Parse(UIElements.Instance.sensor1Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text);
                valorMaximo = float.Parse(UIElements.Instance.sensor1Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text);
            } else if (sensor == 2){
                valorMinimo = float.Parse(UIElements.Instance.sensor2Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text);
                valorMaximo = float.Parse(UIElements.Instance.sensor2Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text);
            } else if (sensor == 3){
                valorMinimo = float.Parse(UIElements.Instance.sensor3Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text);
                valorMaximo = float.Parse(UIElements.Instance.sensor3Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text);
            } else {
                valorMinimo = float.Parse(UIElements.Instance.sensor4Config.transform.Find("Minimo").GetComponent<TMP_InputField>().text);
                valorMaximo = float.Parse(UIElements.Instance.sensor4Config.transform.Find("Maximo").GetComponent<TMP_InputField>().text);
            }
            Debug.Log("Valor minimo: " + valorMinimo + " Valor maximo: " + valorMaximo);
            //StartCoroutine(updateQueryAndExecute("UPDATE Configuracion SET valorMin = " + valorMinimo + " ", configurateSensorPass));
            // Actualizar valorMin y valorMax en la base de datos donde ID_Sensor = sensor y UserHash = userHash
            StartCoroutine(updateQueryAndExecute("UPDATE Configuracion SET valorMin = " + valorMinimo + ", valorMax = " + valorMaximo + " WHERE ID_Sensor = " + sensor + " AND UserHash = '" + userHash + "'", configurateSensorPass));
        } catch {
            Debug.Log("Error al actualizar los valores del sensor, asegúrese de ingresar valores numérico y no sea nulo");
            UIElements.Instance.ConfiguracionesPrompt.text = "Error, asegúrese de ingresar valores numérico de máximo dos digitos decimales que no sea nulo";
        }
    }

    public void simulateSensorValues(int sensor){
        float valor;
        try {
            if (sensor == 1){
                valor = float.Parse(UIElements.Instance.sensor1Sim.transform.Find("Input").GetComponent<TMP_InputField>().text);
            } else if (sensor == 2){
                valor = float.Parse(UIElements.Instance.sensor2Sim.transform.Find("Input").GetComponent<TMP_InputField>().text);
            } else if (sensor == 3){
                valor = float.Parse(UIElements.Instance.sensor3Sim.transform.Find("Input").GetComponent<TMP_InputField>().text);
            } else {
                valor = float.Parse(UIElements.Instance.sensor4Sim.transform.Find("Input").GetComponent<TMP_InputField>().text);
            }
            Debug.Log("Valor: " + valor);
            StartCoroutine(updateQueryAndExecute("UPDATE Sensor SET valor = " + valor + " WHERE ID_Sensor = " + sensor, simulateSensorPass));
        
        } catch {
            Debug.Log("Error al simular el sensor, asegúrese de ingresar un valor numérico y no sea nulo");
            UIElements.Instance.SimularPrompt.text = "Error, asegúrese de ingresar un valor numérico de máximo dos digitos decimales que no sea nulo";
        }
    }

    // Método para probar la conexión con la API
    private IEnumerator TestGetRequest(){
        string url = "http://" + ip + ":5000/queries/test";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                webRequestString = webRequest.downloadHandler.text;
                Debug.Log("JSON: " + webRequestString);
                UIElements.Instance.CreateUserName.text = "";
                UIElements.Instance.CreatePassword.text = "";
                UIElements.Instance.CreateConfirmPassword.text = "";
                UIElements.Instance.JsonResponse.text = webRequestString;
            }
            else
            {
                Debug.LogError("Failed to fetch JSON: " + webRequest.error);
                UIElements.Instance.loginPrompt.text = "Error al intentar ingresar, asegúrese que los datos sean correctos";
            }
        }
    }

}


