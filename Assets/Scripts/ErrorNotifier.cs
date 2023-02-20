using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorNotifier : MonoBehaviour
{
    [SerializeField] TMP_Text _errorText;
    [SerializeField] GameObject _notificatorObject;

    public void ShowErrorMessageToUser(string message)
    {
        _notificatorObject.SetActive(true);
        _errorText.text = message; 
    }
    public void Dismiss()
    {
        _notificatorObject.SetActive(false);
    }
}
