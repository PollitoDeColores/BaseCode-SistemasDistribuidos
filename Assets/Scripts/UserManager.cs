using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using TMPro;

public class UserManager : MonoBehaviour
{
    public GameObject scorePrefab;
    public Transform content; 

    [SerializeField] GameObject _Login;
    [SerializeField] GameObject _Signin;
    [SerializeField] GameObject _Scores;

    [SerializeField] TextMeshProUGUI ErrorMSG_Signin;
    [SerializeField] TextMeshProUGUI ErrorMSG_Login;
    [SerializeField] TextMeshProUGUI ErrorMSG_Scores;

    private string registerUrl = "https://sid-restapi.onrender.com/register";
    private string loginUrl = "https://sid-restapi.onrender.com/login";
    private string scoreUrl = "https://sid-restapi.onrender.com/score";
    private string scoresUrl = "https://sid-restapi.onrender.com/scores";


    void Awake()
    {
        _Login.SetActive(false);
        _Scores.SetActive(false);
        _Signin.SetActive(true);
    }
    public void RegisterUser(string username, string password)
    {
        StartCoroutine(RegisterCoroutine(username, password));
    }

    private IEnumerator RegisterCoroutine(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(registerUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorMSG_Signin.text = "Error: " + www.error;
            }
            else
            {
                ErrorMSG_Signin.text = "User registered successfully";
                yield return new WaitForSeconds(3f);
                _Signin.SetActive(false);
                _Login.SetActive(true);
            }
        }
    }

    public void LoginUser(string username, string password)
    {
        StartCoroutine(LoginCoroutine(username, password));
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                // Process the response, e.g., save the token
                string jsonResponse = www.downloadHandler.text;
                // Assuming response contains a JSON object with a "token" field
                TokenResponse response = JsonUtility.FromJson<TokenResponse>(jsonResponse);
                string token = response.token;
                PlayerPrefs.SetString("AuthToken", token);
            }
        }
    }
    public bool IsUserAuthenticated()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString("AuthToken", null));
    }

    public void UpdateScore(string username, int score)
    {
        StartCoroutine(UpdateScoreCoroutine(username, score));
    }

    private IEnumerator UpdateScoreCoroutine(string username, int score)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(scoreUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                ErrorMSG_Scores.text = "Error: " + www.error;
            }
            else
            {
                ErrorMSG_Scores.text = "Score updated successfully";
            }
        }
    }
    public void GetScores()
    {
        StartCoroutine(GetScoresCoroutine());
    }

    private IEnumerator GetScoresCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(scoresUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                // Process the response
                string jsonResponse = www.downloadHandler.text;
                Score[] scores = JsonUtility.FromJson<ScoreList>(jsonResponse).scores;

                // Sort the scores
                System.Array.Sort(scores, (x, y) => y.score.CompareTo(x.score));

                DisplayScores(scores);
            }
        }
    }
    private void DisplayScores(Score[] scores)
    {
               // Limpiar el contenido actual
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Instanciar y agregar los elementos de puntaje
        foreach (Score score in scores)
        {
            GameObject scoreObject = Instantiate(scorePrefab, content);
            TextMeshProUGUI scoreText = scoreObject.GetComponent<TextMeshProUGUI>(); // Usa Text en lugar de TextMeshProUGUI si no usas TMP

            scoreText.text = $"{score.username}: {score.score}";
        }
    }
    }

    [System.Serializable]
    public class Score
    {
        public string username;
        public int score;
    }

    [System.Serializable]
    public class ScoreList
    {
        public Score[] scores;
    }
    [System.Serializable]
    public class TokenResponse
    {
        public string token;
    }


