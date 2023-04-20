//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using UnityEngine;

public class IndicatorDisplay : MonoBehaviour
{
    public TextMesh DisplayText;

    public void SetText(string label)
    {
        DisplayText.text = label;
    }

    public void SetPosition(Vector3 pos)
    {
        gameObject.transform.position = pos;
    }
}