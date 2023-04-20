using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class JointManager : MonoBehaviour
{
    // List<Vector3> inputPoints = new List<Vector3>();
    public Vector3[] inputPoints = new Vector3[30];
    public Vector3[] origins = new Vector3[30];
    public static JointManager Instance;
    public TCPServer TCPServerInst;
    public VideoPanelApp VidPan;
    public Appendage leftShoulder, rightShoulder;
    public Appendage neck;
    public Appendage upLeftArm, upRightArm;
    public Appendage downLeftArm, downRightArm;
    public Appendage Chest;
    public Appendage leftThigh, rightThigh;
    public Appendage leftHip, rightHip;
    public Appendage leftCalf, rightCalf;
    public Vector3 offsetx = new Vector3(0, 0, 0);
    public Vector3 offsety = new Vector3(0, 0, 0);
    public Photo photo;
    bool moving = false;
    public Transform Alignment;

    public Joint Joint0, Joint1, Joint2, Joint3, Joint4, Joint5, Joint6, Joint7, Joint8, Joint9, 
    Joint10, Joint11, Joint12, Joint13, Joint14;
    string dataStr1 = "[[[203.11984     70.61233      0.85032684][201.18245     92.093475     0.7717505 ][179.7497      92.72288      0.71819216][162.8456      75.1874       0.67729414][168.03442     38.827717     0.7388757 ][225.19576     92.07085      0.68078023][242.1103      70.63848      0.7278591 ][237.5769      35.5829       0.73172563][199.86897    175.20737      0.592863  ][184.29965    174.56891      0.5853335 ][186.88551    230.3919       0.70634013][  0.           0.           0.        ][215.44148    175.21196      0.5477789 ][210.28612    231.06393      0.55859226][  0.           0.           0.        ][199.22615     65.47751      0.85433966][205.76064     65.45351      0.88293946][191.44511     68.67958      0.80049396][212.21378     66.74113      0.7348363 ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ]]]";
    // Start is called before the first frame update
    string dataStr2 = "[[[205.05992     60.275684     0.8442647 ][201.8461      90.79862      0.7895425 ][178.44495     90.74817      0.70575386][159.60767    127.14301      0.68829864][143.36208    159.63239      0.68663734][226.53299     91.426125     0.74782443][242.75282    136.8857       0.5977534 ][260.91974    171.94618      0.66528285][199.21667    172.59653      0.5727261 ][182.99342    170.64966      0.50732106][186.25975    225.19977      0.7617384 ][  0.           0.           0.        ][214.81424    173.2575       0.55140316][209.62798    225.86302      0.63303447][  0.           0.           0.        ][199.8878      55.062904     0.86086994][209.61972     55.08306      0.84150386][192.08124     58.300484     0.7856644 ][215.48468     58.941784     0.8354858 ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ]]]";
    string dataStr3 = "[[[220.64293     72.59975      0.8196673 ][219.9934      92.72144      0.72339684][200.53319     92.080444     0.65149885][194.01683     65.44989      0.5865198 ][199.88382     25.185583     0.65358794][237.55392     92.73597      0.6760509 ][246.64006     65.4404       0.4988146 ][244.05623     27.766516     0.6778809 ][214.81604    177.7971       0.54274994][199.86731    177.14473      0.5222914 ][199.88133    231.7085       0.56231403][  0.           0.           0.        ][229.11209    178.44904      0.5187891 ][221.31438    237.55731      0.48274985][  0.           0.           0.        ][215.47131     68.68609      0.8076409 ][225.19304     68.67941      0.7818011 ][208.96683     70.64154      0.770159  ][231.07092     70.01674      0.67402077][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ][  0.           0.           0.        ]]]";
    string dataStr4 = "[[[1.03118134e+02 6.99873962e+01 2.88619578e-01][1.03757507e+02 8.75215988e+01 7.35780954e-01][8.55773468e+01 9.07415543e+01 7.16933608e-01][6.34864426e+01 6.54687424e+01 7.07642257e-01][8.04082489e+01 4.20597572e+01 7.24343121e-01][1.21941612e+02 8.62053604e+01 7.26475894e-01][1.27820770e+02 6.41296692e+01 7.71827757e-01][1.08954285e+02 4.01150093e+01 7.46629119e-01][1.03761574e+02 1.38190353e+02 6.38162971e-01][9.07512207e+01 1.38192856e+02 6.32528305e-01][6.02510033e+01 1.58949036e+02 8.16145182e-01][6.47959747e+01 2.27809174e+02 5.01409650e-01][1.17400948e+02 1.38182007e+02 6.20658875e-01][1.37524857e+02 1.60911041e+02 7.70069838e-01][1.23897186e+02 2.15476379e+02 6.96041703e-01][9.92138443e+01 6.54317474e+01 3.01264435e-01][1.06361214e+02 6.54366455e+01 2.87769794e-01][9.07777481e+01 6.86885452e+01 3.72277319e-01][1.11543053e+02 6.93356628e+01 1.21816687e-01][1.42713959e+02 2.30408524e+02 6.66928828e-01][1.43365540e+02 2.27155731e+02 7.03941524e-01][1.18062080e+02 2.20677872e+02 5.86193681e-01][7.12915802e+01 2.32383850e+02 2.03017890e-01][6.41454773e+01 2.37560364e+02 2.09143758e-01][6.60891037e+01 2.32355682e+02 2.48061493e-01]]]";
    void Start()
    {
        Instance = this;
        getDataPoints(dataStr1);
        UpdatePosition();
    }

    public void BodyAlignment()
    {
        TCPServerInst.finishedSettingData = true;
    }

    void Update()
    {
        if(TCPServerInst.finishedSettingData)
        {
            inputPoints = TCPServerInst.inputPointsTCP;
            origins = inputPoints;
            UpdatePosition();
            TCPServerInst.finishedSettingData = false;
        }

        if(moving)
        {
            UpdatePosition();
            moving = false;
        }

        if (Input.GetMouseButtonDown(0))
            Down();

        if (Input.GetMouseButtonDown(1))
            Up();

        if (Input.GetMouseButtonDown(2))
            Left();
    }

    public void TakePhoto()
    {
        var random = new System.Random();
        bool holoOn = (random.Next(2) == 1);
        photo.Init(holoOn);
    }

    public void Up()
    {   
        Vector3 off = new Vector3(0, 0.2f, 0);
        Alignment.position += off;
        moving = true;
    }
    public void Down()
    {
        Vector3 off = new Vector3(0, -0.2f, 0);
        Alignment.position += off;
        moving = true;
    }
    public void Right()
    {
        Vector3 off = new Vector3(0.2f, 0, 0);
        Alignment.position += off;
        moving = true;
    }
    public void Left()
    {
        Vector3 off = new Vector3(-0.2f, 0, 0);
        Alignment.position += off;
        moving = true;
    }

    public void UpdatePosition()
    {   
        SetJoints();
        SetBodyFromPoints();
    }

    public void AlignData()
    {
        Debug.Log(" alignment block: " + Alignment.position);
        Debug.Log(" originblock: " + origins[1]);

        offsetx = origins[1] - Alignment.position;
        offsety = Alignment.position + origins[1];

        inputPoints[1].x = Alignment.position.x + 0.01f;
        inputPoints[1].y = -Alignment.position.y + 0.01f;
        inputPoints[1].z = Alignment.position.z;

        inputPoints = VidPan.AlignTextures(origins, inputPoints[1].x, inputPoints[1].y);

        for(int i = 0; i < inputPoints.Length; i++)
        {
            if(i != 1) {
                inputPoints[i].x = origins[i].x - offsetx.x;
                inputPoints[i].y = origins[i].y - offsety.y;
                //inputPoints[i].z = origins[i].z - offsetx.z;
            }
        }

    }

    void SetBodyFromPoints()
    {
        neck.SetPosition(inputPoints[1], inputPoints[0]);

        leftShoulder.SetPosition(inputPoints[2], inputPoints[1]);
        upLeftArm.SetPosition(inputPoints[3], inputPoints[2]);
        downLeftArm.SetPosition(inputPoints[4], inputPoints[3]);

        rightShoulder.SetPosition(inputPoints[5], inputPoints[1]);
        upRightArm.SetPosition(inputPoints[6], inputPoints[5]);
        downRightArm.SetPosition(inputPoints[7], inputPoints[6]);

        Chest.SetPosition(inputPoints[8], inputPoints[1]);

        rightHip.SetPosition(inputPoints[12], inputPoints[8]);
        leftHip.SetPosition(inputPoints[9], inputPoints[8]);

        rightThigh.SetPosition(inputPoints[13], inputPoints[12]);
        leftThigh.SetPosition(inputPoints[10], inputPoints[9]);

        rightCalf.SetPosition(inputPoints[14], inputPoints[13]);
        leftCalf.SetPosition(inputPoints[11], inputPoints[10]);
    }

    void SetJoints()
    {
        AlignData();
        Joint0.SetPointer(inputPoints[0]);
        Joint1.SetPointer(inputPoints[1]);
        Joint2.SetPointer(inputPoints[2]);
        Joint3.SetPointer(inputPoints[3]);
        Joint4.SetPointer(inputPoints[4]);
        Joint5.SetPointer(inputPoints[5]);
        Joint6.SetPointer(inputPoints[6]);
        Joint7.SetPointer(inputPoints[7]);
        Joint8.SetPointer(inputPoints[8]);
        Joint9.SetPointer(inputPoints[9]);
        Joint10.SetPointer(inputPoints[10]);
        Joint11.SetPointer(inputPoints[11]);
        Joint12.SetPointer(inputPoints[12]);
        Joint13.SetPointer(inputPoints[13]);
        Joint14.SetPointer(inputPoints[14]);
    }
    public bool getDataPoints(string input)
    {
        input = input.Substring(1);
        input = input.Substring(1); 
        input = input.Substring(0, input.Length - 1); 
        input = input.Substring(0, input.Length - 1); 

        string [] x = input.Split('[').ToArray();
        
        for(int i = 1; i < x.Length; i++){
            x[i] = x[i].Substring(0, (x[i]).Length - 1).Trim(); 
            Debug.Log(x[i]);
            x[i] = x[i].Replace(System.Environment.NewLine + "  ", "");
            while (x[i].Contains("  "))
            {
                x[i] = x[i].Replace("  ", " ");
            }
            x[i] = x[i].Substring(0, (x[i]).Length - 1).Trim(); 
            string [] chx = x[i].Split(' ').ToArray();
            float v0 = float.Parse(chx[0])/50;
            float v1 = float.Parse(chx[1])/50;
            float v2 = float.Parse(chx[2])/50;
            Vector3 vec = new Vector3(v0, v1, v2);
            inputPoints[i-1] = vec;
            origins[i-1] = vec;
        }
        return true;
    }
}
