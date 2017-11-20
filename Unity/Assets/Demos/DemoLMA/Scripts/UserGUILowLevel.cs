using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class UserGUILowLevel : MonoBehaviour {

    static string _nameStr = "";
    static string _errorStr = "";
    private bool _isPosted = false;
    
    
    void Start() {
        _isPosted = false;
        
        
    }
    
    void OnGUI () {
      
     GUILayout.BeginArea (new Rect (Screen.width*0.3f, Screen.height*0.3f, Screen.width*0.5f, Screen.height*0.9f));
     GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        
         GUILayout.BeginHorizontal ();	
         GUILayout.Label ("User Name:  ", style);   
         _nameStr = GUILayout.TextField(_nameStr, 180);        
         GUILayout.EndHorizontal ();

        
         if(GUILayout.Button("Start")){            
             if (_nameStr == "")
                 _errorStr = "Please enter a unique user name";
             else {
                     _errorStr = "Loading";
                     UserInfo.UserId = _nameStr;
                     this.StartCoroutine(PostUserInfo());
                    
               

     
             }
         }
        
         if (_isPosted)
             Application.LoadLevel("DemoDrives");
         
      
        style.fontSize = 15;
        
        style.normal.textColor = Color.red;  
        GUILayout.Label (_errorStr);
         
 
        GUILayout.EndArea();

    }

    
     IEnumerator PostUserInfo() {
         string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/v2/putUserInfo.php";

     // Create a form object for sending high score data to the server
        var form = new WWWForm();        
        form.AddField( "userId", UserInfo.UserId);        
         

         // Create a download object
        var download = new WWW( resultURL, form );



         
        // Wait until the download is done
        yield return download;

        _isPosted = true;
         _errorStr = download.error;


     }

    
        
}
