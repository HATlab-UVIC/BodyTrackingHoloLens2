//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using UnityEngine;
using UnityEngine.UI;

public class VideoPanel : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    public void SetResolution(int width, int height)
    {
        transform.localScale = new Vector3(0.4f, 0.4f * height / width);

        var texture = new Texture2D(width, height, TextureFormat.BGRA32, false);

        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void SetBytes(byte[] image)
    {
        var texture = meshRenderer.sharedMaterial.mainTexture as Texture2D;
        texture.LoadRawTextureData(image); //TODO: Should be able to do this: texture.LoadRawTextureData(pointerToImage, 1280 * 720 * 4);
        texture.Apply();
    }
}
