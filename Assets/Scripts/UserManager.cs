using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class UserManager : MonoBehaviour
{
    public TMP_Text puntajesText;
    string url = "https://sid-restapi.onrender.com";
    public string Token { get; private set; }
    public string Username { get; private set; }
    public GameObject panelAuth;

    void Start()
    {
        Token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("No hay Token");
            panelAuth.SetActive(true);
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            StartCoroutine(GetProfile());
            panelAuth.SetActive(false); // Oculta el panel si el token está presente
        }
    }

    public void enviarRegistro()
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine(Registro(JsonUtility.ToJson(new AuthenticationData { username = username, password = password })));
    }

    public void enviarLogin()
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine(Login(JsonUtility.ToJson(new AuthenticationData { username = username, password = password })));
    }

    public void CerrarSesion()
    {
        PlayerPrefs.DeleteKey("token");
        panelAuth.SetActive(true);
    }

    public void ActualizarPuntaje()
    {
        string _newScore = GameObject.Find("InputFieldPuntaje").GetComponent<TMP_InputField>().text;
        if (int.TryParse(_newScore, out int result)){
            int newScore = int.Parse(_newScore);
            StartCoroutine(UpdateScore(newScore));
        }
        else puntajesText.text = "El puntaje solo recibe números, corrijalo por favor";
    }

    public void VerPerfiles(){
        StartCoroutine(GetProfile());
    }

    IEnumerator Registro(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", json);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                Debug.Log("Registro Exitoso!");
                StartCoroutine(Login(json));
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }

    IEnumerator Login(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/auth/login", json);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                AuthenticationData data = JsonUtility.FromJson<AuthenticationData>(request.downloadHandler.text);
                Token = data.token;
                Username = data.username;
                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);

                Debug.Log(data.token);

            
                panelAuth.SetActive(false);
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }


    IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + Username);
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

                response.usuarios = response.usuarios.OrderByDescending(u => u.data.score).ToList();

                string puntajesInfo = "";

                int count = Mathf.Min(5, response.usuarios.Count); 

                for (int i = 0; i < count; i++)
                {
                    var usuario = response.usuarios[i];
                    puntajesInfo += "El usuario " + usuario.username + " tiene un puntaje de " + usuario.data.score + "\n";
                }

                puntajesText.text = puntajesInfo;
            }
            else
            {
                Debug.Log("El usuario no está autenticado");
            }
        }
    }


    IEnumerator UpdateScore(int newScore)
    {
        DataUser newData = new DataUser();
        newData.score = newScore;
        string json = JsonUtility.ToJson(newData);

        UnityWebRequest request = UnityWebRequest.Put(url + "/api/scores/" + Username, json);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                Debug.Log("Puntaje actualizado con éxito");
                StartCoroutine(GetProfile()); // Actualizar la lista de puntajes después de actualizar el puntaje del usuario
            }
            else
            {
                Debug.Log("Error al actualizar puntaje" + request.responseCode);
            }
        }
    }



    [System.Serializable]
    public class AuthResponse
    {
        public List<UsuarioJson> usuarios;
    }

    [System.Serializable]
    public class AuthenticationData
    {
        public string username;
        public string password;
        public UsuarioJson usuario;
        public string token;
        public bool estado;
        public DataUser data;
    }

    [System.Serializable]
    public class UsuarioJson
    {
        public string _id;
        public string username;
        public DataUser data;
    }

    [System.Serializable]
    public class DataUser
    {
        public int score;
    }
}

