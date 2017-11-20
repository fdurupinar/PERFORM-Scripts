// TODO:
// By default, screenshot files are placed next to the executable bundle -- we don't want this in a
// shipping game, as it will fail if the user doesn't have write access to the Applications folder.
// Instead we should place the screenshots on the user's desktop. However, the ~/ notation doesn't
// work, and Unity doesn't have a mechanism to return special paths. Therefore, the correct way to
// solve this is probably with a plug-in to return OS specific special paths.

// Mono/.NET has functions to get special paths... see discussion page. --Aarku

using UnityEngine;
using System.Collections;

public class Screenshot : MonoBehaviour
{    
    private int _screenshotCount = 0;
    private string _screenshotDir = "";
    private int _frameRate = 60;
    public bool IsRunning = false;

    public void CreateDir(string path) {
        _screenshotDir = path;
        _screenshotCount = 0;
        System.IO.Directory.CreateDirectory("SCREENSHOTS\\" + path);
        Time.captureFramerate = _frameRate;
        
    }
        
    // Check for screenshot at each frame
    void Update()
    {
        if (!IsRunning) return;
        // take screenshot on up->down transition of F9 key
    	
        string screenshotFilename;
        do
        {
            _screenshotCount++;

            screenshotFilename = "SCREENSHOTS\\" + _screenshotDir + "\\screenshot" + _screenshotCount + ".png";

        } while (System.IO.File.Exists(screenshotFilename));
        
        Application.CaptureScreenshot(screenshotFilename);

    }
}