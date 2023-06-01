using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;

public class LoginAuth : MonoBehaviour
{

    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text wariningLoginText;
    public TMP_Text confirmLoginText;

    //Função do botao de login.
    public void LoginButton()
    {
        StartLogin(emailInputField.text, passwordInputField.text);
    }

    //Funcao de autenticação do e-mail e senha.
    private void StartLogin(string email, string password)
    {
        //Autentica o loguin e senha no Firebase.
        Credential credential = EmailAuthProvider.GetCredential(email, password);
        // O termo ContinueWithOnMainThread é usado para procesar no mesmo thread que o unity (caso não use as funções internas pode não funcionar corretamnte).
        FireAuth.instance.auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(LoginTask =>
        {
            if (LoginTask.Exception != null)
            {
                HandleLoguinErrors(LoginTask.Exception); //Retorna o erro no Text escolhido.
            }

            LoginUser(LoginTask);

        });
    }

    //Funcao para tratar erro no login.
    void HandleLoguinErrors(System.AggregateException loginException)
    {
        Debug.LogWarning(message: $"Failed to login task with {loginException}"); // Retorna o erro no console.
        FirebaseException firebaseEx = loginException.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
        wariningLoginText.text = DefineLoginErrorMessage(errorCode);
    }

    //Funcao que identifica e retorna o erro.
    string DefineLoginErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Missing Email";
            case AuthError.MissingPassword:
                return "Missing Password";
            case AuthError.InvalidEmail:
                return "Invalid Email";
            case AuthError.UserNotFound:
                return "Account does not exist";
            default:
                return "login falhou";
        }
    }

    //Funcao de Login
    void LoginUser(System.Threading.Tasks.Task<FirebaseUser> loginTask)
    {
        FireAuth.instance.User = loginTask.Result;

        Debug.LogFormat("User Singer in sucessfuly: {0} ({1}) ", FireAuth.instance.User.DisplayName, FireAuth.instance.User.Email);

        wariningLoginText.text = "";
        confirmLoginText.text = FireAuth.instance.User.DisplayName + " Logued In";
    }

}
