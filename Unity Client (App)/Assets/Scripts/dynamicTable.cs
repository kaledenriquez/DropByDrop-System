using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using TMPro;

public class dynamicTable : MonoBehaviour
{
    [SerializeField] GameObject columnPrefab;
    [SerializeField] GameObject elementPrefab;

    // void Start(){
    // //     // cargar el archivo json
    // //     string json = System.IO.File.ReadAllText(Application.dataPath + "/hola.json");
    // //     // generar la tabla
    // //     
    // //     generateTable(json, new List<int>(){1,2,0});
    // }
    public void generateTable(string json, List<int> orden){
        destroyChildren();
        JsonData data = JsonMapper.ToObject(json);
        int rows = data["response"].Count;
        int columns = data["response"][0].Count;
        Debug.Log("Rows: " + rows + " Columns: " + columns);
        List<string> columnNames = new List<string>();
        JsonData element = data["response"][0];

        IDictionary elementDictionary = element as IDictionary;
        foreach(var key in elementDictionary.Keys){
            columnNames.Add(key.ToString());
            Debug.Log(key.ToString());
        }

        for(int i = 0; i < columns; i++){
            GameObject currentColumn = Instantiate(columnPrefab, transform);
            currentColumn.GetComponentInChildren<TextMeshProUGUI>().text = columnNames[i];
            for(int j = 0; j < rows; j++){
                // Se instancia dentro de la columna
                GameObject currentElement = Instantiate(elementPrefab, currentColumn.transform);
                // Se le asigna el valor
                currentElement.GetComponentInChildren<TextMeshProUGUI>().text = data["response"][j][columnNames[i]].ToString();
            }
        }
        // reordena las columnas
        // foreach(int i in orden){
        //     transform.GetChild(i).SetSiblingIndex(orden.IndexOf(i));
        // }

// Crear un diccionario para almacenar los hijos según su índice en la lista de orden
        Dictionary<int, Transform> childrenDict = new Dictionary<int, Transform>();

        // Obtener todos los objetos hijos y asignarlos al diccionario
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            childrenDict.Add(i, child);
        }

        // Ordenar los objetos según la lista de órdenes
        foreach (int i in orden)
        {
            if (childrenDict.ContainsKey(i))
            {
                childrenDict[i].SetAsLastSibling(); // Mover el objeto al final de la lista de hijos
            }
        }

        transform.localPosition = new Vector3(2000, 0, 0);
    }

    public void destroyChildren(){
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void resetPosition(){
        transform.localPosition = Vector3.zero;
        // Sumarle la mitad de su altura y la mitad de su longitud
        transform.localPosition -= new Vector3(0, transform.GetComponent<RectTransform>().rect.height/2, 0);
        transform.localPosition += new Vector3(transform.GetComponent<RectTransform>().rect.width/2, 0, 0);
    }

}
