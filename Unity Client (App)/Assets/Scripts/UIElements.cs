using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIElements : MonoBehaviour
{
    public static UIElements Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public GameObject LogInMenu;
    public GameObject DashBoardMenu;
    public GameObject CreateAccountMenu;
    public GameObject SettingsMenu;
    public GameObject AnimationMenu;
    public TextMeshProUGUI loginPrompt;
    public TextMeshProUGUI createPrompt;
    public TextMeshProUGUI settingsPrompt;
    public TMP_InputField LoginUserName;
    public TMP_InputField LoginPassword;
    public TMP_InputField CreateUserName;
    public TMP_InputField CreatePassword;
    public TMP_InputField CreateConfirmPassword;
    public TMP_InputField DBUser;
    public TMP_InputField DBPassword;
    public TMP_InputField IPAddress;
    public TextMeshProUGUI CurrentUserDB;
    public TextMeshProUGUI CurrentPasswordDB;
    public TextMeshProUGUI CurrentIPAddress;
    public TextMeshProUGUI JsonResponse;
    public TextMeshProUGUI DashboardTitle;
    public GameObject menuAlertas;
    public GameObject menuConsumo;
    public GameObject menuConfiguraciones;
    public GameObject menuSimular;
    public GameObject menuMonitorear;
    public GameObject menuOtros;
    public dynamicTable tablaAlertas;
    public dynamicTable tablaConsumo;
    public dynamicTable tablaMonitorear;
    public GameObject sensor1Config;
    public GameObject sensor2Config;
    public GameObject sensor3Config;
    public GameObject sensor4Config;
    public GameObject sensor1Sim;
    public GameObject sensor2Sim;
    public GameObject sensor3Sim;
    public GameObject sensor4Sim;
    public TextMeshProUGUI ConfiguracionesPrompt;
    public TextMeshProUGUI SimularPrompt;
    public TextMeshProUGUI OtrosPrompt;
    public TextMeshProUGUI OtrosPrompt2;
    public TMP_InputField asistenteVirtualInput;
    
}
