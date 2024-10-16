using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Events;
using Firebase.Auth;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using System;

public class FirebaseAuth_manager : MonoBehaviour
{
    [Header("Player Auth parameters")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private TMP_InputField inputPassword;

    public UnityEvent UserLoggedIn;

    [Header("Player info display")]
    [SerializeField] private TextMeshProUGUI usernameDisplay;
    [SerializeField] private TextMeshProUGUI highScoreDisplay;
    [SerializeField] private TextMeshProUGUI scoreDisplay;

    [Header("Leaderboard")]
    [SerializeField] private GameObject leaderboardUser_prefab;

    private string currentUserID;
    public int currentScore;
    public int downloadedScore;

    public UnityEvent startGame = new UnityEvent();

    private DatabaseReference dataBaseRef;

    #region UserAuth
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var app = Firebase.FirebaseApp.DefaultInstance;
            //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        });

        dataBaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        //FirebaseDatabase.DefaultInstance.GetReference("users").OrderByChild("score").LimitToLast(5).ValueChanged += HandleValueChanged;
    }

    private void Update()
    {
        scoreDisplay.text = currentScore.ToString();
    }

    private IEnumerator RegisterUser()
    {

        var auth = FirebaseAuth.DefaultInstance;
        var singupTask = auth.CreateUserWithEmailAndPasswordAsync(inputEmail.text, inputPassword.text);

        yield return new WaitUntil(() => singupTask.IsCompleted);

        if (singupTask.IsCanceled)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
        }
        else if (singupTask.IsFaulted)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + singupTask.Exception);
        }
        else
        {
            //Firebase fue creado
            Firebase.Auth.AuthResult result = singupTask.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            dataBaseRef.Child("users").Child(result.User.UserId).Child("username").SetValueAsync(inputUsername.text);
            dataBaseRef.Child("users").Child(result.User.UserId).Child("score").SetValueAsync(0);
            
            LogIn_Btn();
            currentUserID = result.User.UserId;
        }
    }


    public void RegisterUser_Btn()
    {
        StartCoroutine(RegisterUser());
    }

    public void LogIn_Btn()
    {
        LoginUser(inputEmail.text, inputPassword.text);
    }

    private void LoginUser(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;

            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.LogFormat("Firebase user logged in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });

        UserLoggedIn.Invoke();
    }

    public void GetUserInfo()
    {
        GetUsername();

        GetUserHighscore();
    }

    private void GetUsername()
    {
        FirebaseDatabase.DefaultInstance.GetReference($"users/{FirebaseAuth.DefaultInstance.CurrentUser.UserId}/username").GetValueAsync().ContinueWith(task =>
        {
            if(task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Username fetch encountered an error...");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;
                Debug.Log(dataSnapshot);

                usernameDisplay.text = dataSnapshot.Value.ToString();
            }
        });
    }

    private void GetUserHighscore()
    {
        FirebaseDatabase.DefaultInstance.GetReference($"users/{FirebaseAuth.DefaultInstance.CurrentUser.UserId}/score").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Highscore fetch encountered an error...");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;
                Debug.Log(dataSnapshot);

                highScoreDisplay.text = dataSnapshot.Value.ToString();
            }
        });
    }

    #endregion


    #region Leaderboard
    public void Event_SendData()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(uid).Child("score").SetValueAsync(currentScore);
    }

    public void ShowLeaderboard()
    {
        
    }

    private void HandleValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.Log(e.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = e.Snapshot;
        Debug.Log(snapshot);

        // Elimina los prefabs existentes
        foreach (var item in gameObject.GetComponentsInChildren<Leaderboard_User>())
        {
            Destroy(item.gameObject);
        }

        // Obtén una lista de usuarios ordenados por puntuación de mayor a menor
        var sortedUsers = ((Dictionary<string, object>)snapshot.Value)
            .OrderByDescending(u => Convert.ToInt32(((Dictionary<string, object>)u.Value)["score"]));

        // Instancia los prefabs en orden
        int i = 0;
        foreach (var userDoc in sortedUsers)
        {
            var userObject = (Dictionary<string, object>)userDoc.Value;
            var userEntry = Instantiate(leaderboardUser_prefab, transform);
            userEntry.transform.position = new Vector2(userEntry.transform.position.x, userEntry.transform.position.y - i * 40);
            userEntry.GetComponent<Leaderboard_User>().SetLabels(userObject["username"].ToString(), userObject["score"].ToString());
            i++;
        }
    }

    public void GetHighScore()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").OrderByChild("score").LimitToLast(3).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.Log(task.Exception);
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log(snapshot);

                // Obtén una lista de usuarios ordenados por puntuación de mayor a menor
                var sortedUsers = ((Dictionary<string, object>)snapshot.Value)
                    .OrderByDescending(u => Convert.ToInt32(((Dictionary<string, object>)u.Value)["score"]));

                int i = 0;

                foreach (var userDoc in (Dictionary<string, object>)snapshot.Value)
                {
                    Debug.Log("USER");
                    var userObject = (Dictionary<string, object>)userDoc.Value;
                    GameObject userEntry = Instantiate(leaderboardUser_prefab, transform, false);
                    userEntry.transform.position = new Vector2(userEntry.transform.position.x, userEntry.transform.position.y - i * 40);
                    userEntry.GetComponent<Leaderboard_User>().SetLabels(userObject["username"].ToString(), userObject["score"].ToString());
                    i++;
                }
            }
        });
    }
    #endregion
}

//[System.Serializable]
//public class AuthenticationData
//{
//    public string username;
//    public string password;
//    public UsersJson usuario;
//    public string token;
//    public UsersList[] usuarios;
//}

//[System.Serializable]
//public class UsersList
//{
//    public UsersJson[] usuarios;
//}

//[System.Serializable]
//public class UsersJson
//{
//    public string _id;
//    public string username;
//    public DataUser data;
//}

//[System.Serializable]
//public class DataUser
//{
//    public int score;
//}

