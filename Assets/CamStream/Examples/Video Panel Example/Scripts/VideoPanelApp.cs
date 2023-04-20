//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//
using UnityEngine;
using System;
using HoloLensCameraStream;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
using Windows.Perception.Spatial;
#endif

#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
using UnityEngine.Windows;
#endif
/// <summary>
/// This example gets the video frames at 30 fps and displays them on a Unity texture,
/// and displayed the debug information in front.
/// 
/// **Add Define Symbols:**
/// Open **File > Build Settings > Player Settings > Other Settings** and add the following to `Scripting Define Symbols` depending on the XR system used in your project;
/// - Legacy built-in XR: `BUILTIN_XR`';
/// - XR Plugin Management (Windows Mixed Reality): `XR_PLUGIN_WINDOWSMR`;
/// - XR Plugin Management (OpenXR):`XR_PLUGIN_OPENXR`.
/// </summary>
public class VideoPanelApp : MonoBehaviour
{

    byte[] _latestImageBytes;
    TCPClient tcpClient;
    TCPServer tcpServer;
    HoloLensCameraStream.Resolution _resolution;

    //"Injected" objects.
    public VideoPanel _videoPanelUI;
    VideoCapture _videoCapture;
    public TextMesh _displayText;

    IntPtr _spatialCoordinateSystemPtr;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif

    public void SendBytesToPythonAll()
    {
        StartCoroutine(SendAllFrames());
    }

    public void SendBytesToPythonSingle()
    {
        StartCoroutine(SendSingleFrame());
    }

    public void SendTxtBytesToPythonSingle()
    {
        StartCoroutine(SendSingleFrameTxt());
    }


    // done by Matthew Sielecki
    public IEnumerator SendAllFrames()
    {
        List<byte[]> savedImages = new List<byte[]>();

        // gets 30 frames of data 
        for(int i = 0; i < 10; i++) {
            savedImages.Add(_latestImageBytes);

            // waits 5 frames before adding to list again
            for(int x = 0; x < 5; x++){
                yield return null;
            }
        }
#if ENABLE_WINMD_SUPPORT
#if WINDOWS_UWP
        for(int i = 0; i < 10; i++){
            tcpClient.SendPVImageAsync(savedImages[i]);
            yield return new WaitForSeconds(0.7f);
        }
        yield return new WaitForSeconds(1.0f);
        tcpClient.MakeVid();
#endif  
#endif
    yield return null;
    }

    public IEnumerator SendSingleFrame()
    {
#if ENABLE_WINMD_SUPPORT
#if WINDOWS_UWP
        tcpClient.SendPVImageAsync(_latestImageBytes);
#endif  
#endif
    yield return null;
    }

    public IEnumerator SendSingleFrameTxt()
    {
#if ENABLE_WINMD_SUPPORT
#if WINDOWS_UWP
        tcpClient.SendPVTxtAsync(_latestImageBytes);
#endif  
#endif
    yield return null;
    }

    public void SendSingleFrameNoAsync()
    {
#if ENABLE_WINMD_SUPPORT
#if WINDOWS_UWP
        tcpClient.SendPVImageAsync(_latestImageBytes);
#endif  
#endif
    }

    public Vector3 [] AlignTextures(Vector3 [] org, float alignX, float alignY)
    {
#if ENABLE_WINMD_SUPPORT
        var frameTexture = researchMode.GetLongDepthMapTextureBuffer();
#endif
        // Calculation to get 1D from 2D y * width + x
        Vector3 [] placeholder = new Vector3 [30];
        DebugText.LOG(org.Length.ToString());

        for(int i = 0; i < org.Length; i++){
            float x = org[i].x + 30.0f;
            float y = org[i].y + 10.0f;

            // if(y < 0)
            // {
            //     y = 0;
            // }

            placeholder[i].z = 0.0f;

#if ENABLE_WINMD_SUPPORT
            int placement = (int)(y * 320 + x);
            float valueDepth = 0.0f;

            valueDepth = -float.Parse(frameTexture[placement].ToString())/80;
            valueDepth = findMaxRecursive(3, placement, valueDepth, frameTexture);


            DebugText.LOG("depthvalue: " + valueDepth.ToString());
            placeholder[i].z += valueDepth;
#endif
            placeholder[i].z += 9.0f;
            placeholder[i].x = alignX;
            placeholder[i].y = alignX;

            // DebugText.LOG(x + " " + y + " : " + placeholder[i].z);

            
        }
        return placeholder;
    }

    public float findMaxRecursive(int range, int initPlacement, float max, byte [] frameText)
    {
        float [] valueRange = new float[5];
        valueRange[1] = max;
        valueRange[1] = -float.Parse(frameText[initPlacement + range].ToString())/80;
        valueRange[2] = -float.Parse(frameText[initPlacement - range].ToString())/80;
        valueRange[3] = -float.Parse(frameText[initPlacement + range * 320].ToString())/80;
        valueRange[4] = -float.Parse(frameText[initPlacement - range * 320].ToString())/80;

        DebugText.LOG("depthvalue MAX: " + valueRange.Max().ToString());

        if(range > 1){
            return findMaxRecursive(range -1, initPlacement, valueRange.Max(), frameText);
        }else{
            return valueRange.Max();
        }
    } 


    public void SavePointCloudPLY()
    {
#if ENABLE_WINMD_SUPPORT
        var longpointCloud = researchMode.GetLongThrowPointCloudBuffer();
        var longpointMap = researchMode.GetLongDepthMapTextureBuffer();
        DebugText.LOG("length: " + longpointMap.Length);

        // DebugText.LOG((float.Parse(longpointMap[5040].ToString())/125).ToString());
#endif
    }

    Queue<Action> _mainThreadActions;
    void Start()
    {
        DebugText.LOG("Starting program");
#if ENABLE_WINMD_SUPPORT
        researchMode = new HL2ResearchMode();
        researchMode.InitializeLongDepthSensor();
#if WINDOWS_UWP && XR_PLUGIN_OPENXR
        researchMode.SetReferenceCoordinateSystem(_spatialCoordinateSystem);
#endif
        researchMode.SetPointCloudDepthOffset(0);
        researchMode.StartLongDepthSensorLoop(true);
#endif
        tcpClient = this.gameObject.GetComponent<TCPClient>();
        tcpServer = this.gameObject.GetComponent<TCPServer>();
        _mainThreadActions = new Queue<Action>();
        

        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping

#if WINDOWS_UWP

#if XR_PLUGIN_WINDOWSMR

        _spatialCoordinateSystemPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;

#elif XR_PLUGIN_OPENXR

        _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

#elif BUILTIN_XR

#if UNITY_2017_2_OR_NEWER
        _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
#else
        _spatialCoordinateSystemPtr = UnityEngine.VR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
#endif
#endif
#endif

        //Call this in Start() to ensure that the CameraStreamHelper is already "Awake".
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        //You could also do this "shortcut":
        //CameraStreamManager.Instance.GetVideoCaptureAsync(v => videoCapture = v);
        _videoPanelUI.meshRenderer.transform.localScale = new Vector3(1, -1, 1);
    }

    public IEnumerator reStart()
    {
         _mainThreadActions = new Queue<Action>();
        

        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping

#if WINDOWS_UWP

#if XR_PLUGIN_WINDOWSMR

        _spatialCoordinateSystemPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;

#elif XR_PLUGIN_OPENXR

        _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

#elif BUILTIN_XR

#if UNITY_2017_2_OR_NEWER
        _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
#else
        _spatialCoordinateSystemPtr = UnityEngine.VR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
#endif
#endif
#endif

        //Call this in Start() to ensure that the CameraStreamHelper is already "Awake".
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        //You could also do this "shortcut":
        //CameraStreamManager.Instance.GetVideoCaptureAsync(v => videoCapture = v);
        _videoPanelUI.meshRenderer.transform.localScale = new Vector3(1, -1, 1);
        yield return null;
    }

    private void Update()
    {
        lock (_mainThreadActions)
        {
            while (_mainThreadActions.Count > 0)
            {
                _mainThreadActions.Dequeue().Invoke();
            }
        }
    }

    public int counter = 0;
    private void FixedUpdate()
    {
        if(counter % 40 == 0){
            //SavePointCloudPLY();
        }
        counter++;
    }

    private void Enqueue(Action action)
    {
        lock (_mainThreadActions)
        {
            _mainThreadActions.Enqueue(action);
        }
    }

    private void OnDestroy()
    {
        if (_videoCapture != null)
        {
            _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            _videoCapture.Dispose();
        }
    }

    void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            Debug.LogError("Did not find a video capture object. You may not be using the HoloLens.");
            Enqueue(() => SetText("Did not find a video capture object. You may not be using the HoloLens."));
            return;
        }

        this._videoCapture = videoCapture;

        //Request the spatial coordinate ptr if you want fetch the camera and set it if you need to 
#if WINDOWS_UWP

#if XR_PLUGIN_OPENXR
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystem(_spatialCoordinateSystem);
#elif XR_PLUGIN_WINDOWSMR || BUILTIN_XR
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(_spatialCoordinateSystemPtr);
#endif

#endif

        _resolution = CameraStreamHelper.Instance.GetLowestResolution();
        float frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(_resolution);
        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        //You don't need to set all of these params.
        //I'm just adding them to show you that they exist.
        CameraParameters cameraParams = new CameraParameters();
        cameraParams.cameraResolutionHeight = _resolution.height;
        cameraParams.cameraResolutionWidth = _resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
        cameraParams.rotateImage180Degrees = false;
        cameraParams.enableHolograms = false;

        Debug.Log("Configuring camera: " + _resolution.width + "x" + _resolution.height + "x" + cameraParams.frameRate + " | " + cameraParams.pixelFormat);
        Enqueue(() => SetText("Configuring camera: " + _resolution.width + "x" + _resolution.height + "x" + cameraParams.frameRate + " | " + cameraParams.pixelFormat));
        Enqueue(() => _videoPanelUI.SetResolution(_resolution.width, _resolution.height));
        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }

    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            Enqueue(() => SetText("Could not start video mode."));
            return;
        }

        Debug.Log("Video capture started.");
        Enqueue(() => SetText("Video capture started."));
    }

    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        lock (_mainThreadActions)
        {
            if (_mainThreadActions.Count > 2)
            {
                sample.Dispose();
                return;
            }
        }

        //When copying the bytes out of the buffer, you must supply a byte[] that is appropriately sized.
        //You can reuse this byte[] until you need to resize it (for whatever reason).
        if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
        {
            _latestImageBytes = new byte[sample.dataLength];
        }
        sample.CopyRawImageDataIntoBuffer(_latestImageBytes);


        //If you need to get the cameraToWorld matrix for purposes of compositing you can do it like this
        float[] cameraToWorldMatrixAsFloat;
        if (sample.TryGetCameraToWorldMatrix(out cameraToWorldMatrixAsFloat) == false)
        {
            //return;
        }

        //If you need to get the projection matrix for purposes of compositing you can do it like this
        float[] projectionMatrixAsFloat;
        if (sample.TryGetProjectionMatrix(out projectionMatrixAsFloat) == false)
        {
            //return;
        }

        // Right now we pass things across the pipe as a float array then convert them back into UnityEngine.Matrix using a utility method
        Matrix4x4 cameraToWorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(cameraToWorldMatrixAsFloat);
        Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionMatrixAsFloat);

        Enqueue(() =>
        {
            _videoPanelUI.SetBytes(_latestImageBytes);

#if XR_PLUGIN_WINDOWSMR || XR_PLUGIN_OPENXR
            // It appears that the Legacy built-in XR environment automatically applies the Holelens Head Pose to Unity camera transforms,
            // but not to the new XR system (XR plugin management) environment.
            // Here the cameraToWorldMatrix is applied to the camera transform as an alternative to Head Pose,
            // so the position of the displayed video panel is significantly misaligned. If you want to apply a more accurate Head Pose, use MRTK.

            Camera unityCamera = Camera.main;
            Matrix4x4 invertZScaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));
            Matrix4x4 localToWorldMatrix = cameraToWorldMatrix * invertZScaleMatrix;
            unityCamera.transform.localPosition = localToWorldMatrix.GetColumn(3);
            unityCamera.transform.localRotation = Quaternion.LookRotation(localToWorldMatrix.GetColumn(2), localToWorldMatrix.GetColumn(1));
#endif

            Debug.Log("Got frame: " + sample.FrameWidth + "x" + sample.FrameHeight + " | " + sample.pixelFormat + " | " + sample.dataLength);
            if (_displayText != null)
            {
                _displayText.text = "Got frame: " + sample.FrameWidth + "x" + sample.FrameHeight + " | " + sample.pixelFormat + " | " + sample.dataLength;
            }
        });

        sample.Dispose();
    }

    private void SetText(string text)
    {
        if (_displayText != null)
        {
            _displayText.text += text + "\n";
        }
    }
}