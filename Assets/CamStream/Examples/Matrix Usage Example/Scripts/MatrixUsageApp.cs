//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using UnityEngine;
using System;
using HoloLensCameraStream;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
using Windows.Perception.Spatial;
#endif

/// <summary>
/// This example uses the locatable camera attributes to project the camera image over the real world.
/// 
/// **Add Define Symbols:**
/// Open **File > Build Settings > Player Settings > Other Settings** and add the following to `Scripting Define Symbols` depending on the XR system used in your project;
/// - Legacy built-in XR: `BUILTIN_XR`';
/// - XR Plugin Management (Windows Mixed Reality): `XR_PLUGIN_WINDOWSMR`;
/// - XR Plugin Management (OpenXR):`XR_PLUGIN_OPENXR`.
/// </summary>
public class MatrixUsageApp : MonoBehaviour
{
    byte[] _latestImageBytes;
    HoloLensCameraStream.Resolution _resolution;

    //"Injected" objects.
    GameObject _videoPanelUI;
    Renderer _videoPanelUIRenderer;
    Texture2D _videoTexture;
    VideoCapture _videoCapture;
    IndicatorDisplay _targetIndicator;

    IntPtr _spatialCoordinateSystemPtr;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

    void Start()
    {

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

        _videoPanelUI = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _videoPanelUI.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        _videoPanelUI.name = "VideoPanelUI";
        _videoPanelUIRenderer = _videoPanelUI.GetComponent<Renderer>() as Renderer;
        _videoPanelUIRenderer.material = new Material(Shader.Find("AR/HolographicImageBlend"));

        _targetIndicator = GameObject.FindObjectOfType<IndicatorDisplay>();
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

        UnityEngine.WSA.Application.InvokeOnAppThread(() => { _videoTexture = new Texture2D(_resolution.width, _resolution.height, TextureFormat.BGRA32, false); }, false);

        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }

    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        Debug.Log("Video capture started.");
    }

    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
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

        sample.Dispose();

        // Right now we pass things across the pipe as a float array then convert them back into UnityEngine.Matrix using a utility method
        Matrix4x4 cameraToWorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(cameraToWorldMatrixAsFloat);
        Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionMatrixAsFloat);

        //This is where we actually use the image data
        //TODO: Create a class like VideoPanel for the next code
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            _videoTexture.LoadRawTextureData(_latestImageBytes);
            _videoTexture.wrapMode = TextureWrapMode.Clamp;
            _videoTexture.Apply();


            _videoPanelUIRenderer.sharedMaterial.SetTexture("_MainTex", _videoTexture);
            _videoPanelUIRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", cameraToWorldMatrix.inverse);
            _videoPanelUIRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
            _videoPanelUIRenderer.sharedMaterial.SetInt("_FlipY", 1);
            _videoPanelUIRenderer.sharedMaterial.SetFloat("_VignetteScale", 0.0f);


            Vector3 inverseNormal = -cameraToWorldMatrix.GetColumn(2);
            // Position the canvas object slightly in front of the real world web camera.
            Vector3 imagePosition = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2);

            _videoPanelUI.gameObject.transform.position = imagePosition;
            _videoPanelUI.gameObject.transform.rotation = Quaternion.LookRotation(inverseNormal, cameraToWorldMatrix.GetColumn(1));


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

            if (_targetIndicator != null)
            {
                _targetIndicator.SetText("cameraToWorldMatrix: \n" + cameraToWorldMatrix + "\n" + "projectionMatrix: \n" + projectionMatrix);
            }

        }, false);
    }
}
