using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{

    [SerializeField] private GameObject userPrefab;



    void Start()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").OrderByChild("score").LimitToLast(5).ValueChanged += HandleValueChanged;
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
            var userEntry = Instantiate(userPrefab, transform);
            userEntry.transform.position = new Vector2(userEntry.transform.position.x, userEntry.transform.position.y - i * 40);
            userEntry.GetComponent<Leaderboard_User>().SetLabels(userObject["username"].ToString(), userObject["score"].ToString());
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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


                int i = 0;

                foreach (var userDoc in (Dictionary<string, object>)snapshot.Value)
                {
                    Debug.Log("USER");
                    var userObject = (Dictionary<string,object>)userDoc.Value;
                    var userEntry = GameObject.Instantiate(userPrefab, transform);
                    userEntry.transform.position = new Vector2(userEntry.transform.position.x, (userEntry.transform.position.y - i * 40));
                    userEntry.GetComponent<Leaderboard_User>().SetLabels("" + userObject["username"], "" + userObject["score"]);
                    Debug.Log(i);
                    i++;

                }

            }
        });
    }




}
