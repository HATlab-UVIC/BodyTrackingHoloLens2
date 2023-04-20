using UnityEngine;
using System.Collections;
using UnityEngine.Windows.WebCam;
using System.Linq;

#if WINDOWS_UWP
using Windows.Storage;
#endif

using System;
using System.IO;

public class Photo : MonoBehaviour {

    PhotoCapture photoCaptureObject = null;    
    
    public VideoPanelApp panel;
    string folderPath = "";
    // Use this for initialization
    public void Init(bool holo)
    {
#if WINDOWS_UWP
        folderPath = Application.persistentDataPath;
        PhotoCapture.CreateAsync(holo, OnPhotoCaptureCreated);
#endif
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 1f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"\CapturedImage{0}_n.jpg", Time.time);
            string filePath = folderPath + filename;
            Debug.Log("Saving photo to " + filePath);

            try
            {
                photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("System.ArgumentException:\n" + e.Message);
            }
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            panel.reStart();
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }
}