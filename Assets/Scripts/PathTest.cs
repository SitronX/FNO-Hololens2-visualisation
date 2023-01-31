using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class PathTest : MonoBehaviour
{
    [SerializeField] TMP_Text _debugDialogue;
    // Start is called before the first frame update
    void Start()
    {
        string path = "None";

#if ENABLE_WINMD_SUPPORT               
         path= Windows.Storage.KnownFolders.DocumentsLibrary.Path;
#endif
        _debugDialogue.text = path;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
