using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResetPassword : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_Text messageText;

    public void RecoverPassword()
    {
        string email = emailInput.text;

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
            {
                messageText.text = "An email has been sent in order to reset your password.";
            }
            else
            {
                messageText.text = "There has been an error while sending the email, plese try again in a moment.";
            }
        });
    }
}
