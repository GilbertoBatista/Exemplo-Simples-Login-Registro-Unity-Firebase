using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using Firebase.Extensions;

public class RegAuth : MonoBehaviour
{

    public TMP_InputField usernameRegistrerField;
    public TMP_InputField emailRegistrerField;
    public TMP_InputField passwordRegistrerField;
    public TMP_InputField verifyPasswordRegistrerField;
    public TMP_Text waringRegisterText;

   public void RegisterButton()
    {
        StartRegister(emailRegistrerField.text, passwordRegistrerField.text, usernameRegistrerField.text);
    }

    private void StartRegister(string email, string password, string username)
    {
        if (!CheckRegistrationFieldsAndReturnForErrors())
        {
            bool Reg = false;
            // O termo ContinueWithOnMainThread é usado para procesar no mesmo thread que o unity (caso não use as funções internas pode não funcionar corretamnte).
            FireAuth.instance.auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(RegisterTask =>
            {

                if (RegisterTask.Exception != null)
                {
                    HandleRegisterErrors(RegisterTask.Exception); //Retorna o erro no Text escolhido.
                }
               
                RegisterUser(username, email, password);

            });
        }
        
    }

    //Tratamento de erro para campos vazios e senhas diferentes.
    bool CheckRegistrationFieldsAndReturnForErrors()
    {
        if (usernameRegistrerField.text == "")
        {
            waringRegisterText.text = "Nome de Usuario vazio";
            return true;
        }
        else if (passwordRegistrerField.text != verifyPasswordRegistrerField.text)
        {
            waringRegisterText.text = "Os campos \"Senha\" e \"Verifica Senha \" são diferentes." ;
            return true;
        }
        else if (emailRegistrerField.text == "")
        {
            waringRegisterText.text = "Email vazio";
            return true;
        }
        else if (passwordRegistrerField.text == "")
        {
            waringRegisterText.text = "Password vazio";
            return true;
        }
        else
        {
            return false;
        }
    }

    //Tratamento de erros no cadastro de dados de autenticação (email e senha).
    void HandleRegisterErrors(System.AggregateException registrerException)
    {
        Debug.LogWarning(message: $"Failed to register task with: {registrerException}");
        FirebaseException firebaseEX = registrerException.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEX.ErrorCode;

        waringRegisterText.text = DefineRegisterErrorMessage(errorCode);
    }

    //Funcao que identifica e retorna o erro.
    string DefineRegisterErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Missing Email";
            case AuthError.MissingPassword:
                return "Missing Password";
            case AuthError.InvalidEmail:
                return "Invalid Email";
            case AuthError.WeakPassword:
                return "Weak Passord";
            case AuthError.EmailAlreadyInUse:
                return "Email Already In Use";
            default:
                return "Register Failed";
        }
    }

    private void RegisterUser(string displayName, string email, string password)
    {
        FirebaseUser user = FireAuth.instance.auth.CurrentUser;
        if (user != null)  
        {
            // Grava o Username no Firebase.
            UserProfile profile = new () { DisplayName = displayName };
            user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task => {
                if (task.Exception != null)
                {
                    HandleProfileCreatorErrors(task.Exception);
                    return;
                }
                //Troca de menu e realiza o Login.
                ChangeUI.instance.ChangeBetweenLoginAndRegister();
                FindObjectOfType<LoginAuth>().GetComponent<LoginAuth>().passwordInputField.text = password;
                FindObjectOfType<LoginAuth>().GetComponent<LoginAuth>().emailInputField.text = email;
                FindObjectOfType<LoginAuth>().GetComponent<LoginAuth>().LoginButton();

            });

        }

    }

    //Tratamento de erro ao gravar dados no Firebase.
    void HandleProfileCreatorErrors(System.AggregateException profileException)
    {
        Debug.LogWarning(message: $"Failed to register task with: {profileException}");

        FirebaseException firebaseEX = profileException.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEX.ErrorCode;
        waringRegisterText.text = "Username set failed";
    }

}
