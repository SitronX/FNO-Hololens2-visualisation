using QRTracking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorNotifier : MonoBehaviour
{
    [SerializeField] TMP_Text _errorText;
    [SerializeField] GameObject _notificatorObject;

    public static ErrorNotifier Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public void AddErrorMessageToUser(string message)
    {
        _notificatorObject.SetActive(true);
        _errorText.text += "<br>"+message; 
    }
    public void Dismiss()
    {
        _errorText.text = "";
        _notificatorObject.SetActive(false);
    }
}
