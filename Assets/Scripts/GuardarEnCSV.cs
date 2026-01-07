using UnityEngine;
using TMPro;
using System.IO;

public class GuardarEnCSV : MonoBehaviour
{
    [Header("Campos del formulario")]
    public TMP_InputField campoNombre;
    public TMP_InputField campoCorreo;
    public TMP_InputField campoTelefono;
    public TMP_InputField campoEmpresa;
    public TMP_InputField campoCargo;

    [Header("Configuraci�n del archivo")]
    public string nombreArchivo = "datos.csv";

    public void GuardarDatos()
    {
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo);

        if (!File.Exists(ruta))
        {
            string encabezado = "Nombre,Correo,Telefono,Empresa,Cargo";
            File.WriteAllText(ruta, encabezado + "\n");
        }

        string linea = $"{campoNombre.text},{campoCorreo.text},{campoTelefono.text},{campoEmpresa.text},{campoCargo.text}";

        File.AppendAllText(ruta, linea + "\n");

        Debug.Log($"Datos guardados en: {ruta}");
    }
}
