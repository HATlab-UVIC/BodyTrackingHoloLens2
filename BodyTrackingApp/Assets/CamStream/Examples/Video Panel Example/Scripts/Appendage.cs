using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appendage : MonoBehaviour
{

    Vector3 appOffsetx = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 appOffsety = new Vector3(0.0f, 0.0f, 0.0f);
    // Update is called once per frame
    void Update()
    {
        //SetAll(cy1);
    }

    public void SetPosition(Vector3 v1, Vector3 v2)
    {
        appOffsetx = -JointManager.Instance.offsetx;
        appOffsety = -JointManager.Instance.offsety;

        if(((v1.x == appOffsetx.x) && (v1.y == appOffsety.y)) || ((v2.x == appOffsetx.x) && (v2.y == appOffsety.y))){
            this.gameObject.SetActive(false);
        }else {
            this.gameObject.SetActive(true);
        }

        v1.y = v1.y * -1;
        v2.y = v2.y * -1;
        if(v2.y < v1.y)
        {
            Vector3 temp = v2;
            v2 = v1;
            v1 = temp;
        }
        // Debug.Log(appOffset);

        Debug.Log("z value" + v2.z);

        Vector3 between = v2 - v1;
        float scalez =  Mathf.Sqrt(Mathf.Pow(between.y, 2.0f) + Mathf.Pow(between.x, 2.0f) + Mathf.Pow(between.z, 2.0f));

        Vector3 distance = between/between.magnitude;
        this.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, scalez);
        this.gameObject.transform.position = v1 + (between / 2.0f);
        this.gameObject.transform.LookAt(v2);
    }
}
