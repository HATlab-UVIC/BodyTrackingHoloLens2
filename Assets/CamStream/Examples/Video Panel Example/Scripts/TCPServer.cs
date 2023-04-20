using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//using HUX.Interaction;
//using HUX.Receivers;
using UnityEngine.UI;

//<JEM>Ignore unity editor and run this code in the hololens instead</JEM>
#if !UNITY_EDITOR
using System.Threading;

using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;

#endif

//Able to act as a reciever 
public class TCPServer : MonoBehaviour
{
    public GameObject debugLogText;
    Stream streamIn;
    Renderer rend;
    Texture2D texture;
    int counter = 0;
    byte[] byArray;
    bool logInputSize = false;
    bool socketClosed = false;
    bool writeStringToFile = false;
    bool loadTexture = false;
    bool logSize = false;
    bool recievedData = false;

    bool logRealInput = false;
    string realInput;
    public bool finishedSettingData = false;

    int sizeOfBuffer;
    string error;
    public Vector3[] inputPointsTCP;
    public Vector3 offset = new Vector3(0, 0, 0);
    public Transform Alignment;
    
    uint imageSize;
#if !UNITY_EDITOR
        StreamSocket socket;
        StreamSocketListener listener;
        String port;
        String message;
    
#endif
    // Use this for initialization
    void Start()
    {
#if !UNITY_EDITOR
        offset = Alignment.position;
        rend = this.GetComponent<Renderer>();
        listener = new StreamSocketListener();
        port = "8080";

        listener.ConnectionReceived += _receiver_socket_ConnectionReceived;

        listener.Control.KeepAlive = false;

        Listener_Start();
#endif
    inputPointsTCP = new Vector3[30];
    }

#if !UNITY_EDITOR
    private async void Listener_Start()
    {
        Debug.Log("Listener started");
        try
        {
            LOG("Listener started");
            await listener.BindServiceNameAsync(port);
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

       
    }

    private async void _receiver_socket_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        
        try
        {
            while(true)
            {
                using (var dr = new DataReader(args.Socket.InputStream))
                {
                    dr.InputStreamOptions = InputStreamOptions.Partial;
                    await dr.LoadAsync(60000); //loading the buffer
                    //Read the buffer but I dont want to read the whole 15000 
                    
                    var inputSize = dr.ReadString(5); //reading the buffer, there may be a b in it so we remove that if found
                    if(inputSize.EndsWith("b"))
                    {
                        inputSize = inputSize.Substring(0, (inputSize.Length - 1)); //Remove the b from the size string if the size is 4 bytes
                    }
                    imageSize = Convert.ToUInt32(inputSize);
                    logInputSize = true;
                    recievedData = true;
                    var input = dr.ReadString(imageSize);

                    input = input.Substring(1);
                    input = input.Substring(0, input.Length - 1);
                    input = input + "==";

                    byte[] byteArray = Convert.FromBase64String(input);
                    string fullInput = System.Text.Encoding.UTF8.GetString(byteArray);
                    finishedSettingData = ChangeStuff(fullInput);
                    // bool finished = JointManager.Instance.getDataPoints(fullInput);
                    //JointManager.Instance.UpdatePosition(fullInput);
                    
                    // //trim off the b'' part of the base64 encoding, if 4bytes remove 1 if 5 bytes remove 2
                    // if(inputSize.Length == 4)
                    // {
                    //     input = input.Substring(1, input.Length - 1);
                    // }
                    // else
                    // {
                    //     input = input.Substring(2, input.Length - 2);
                    // }
                    // //input = input.Substring(0, (input.Length - 1));

                    // realInput = input.Substring(0, 10) + "\n" + input.Substring(input.Length - 3); //display realInput start and end bytes
                    // logRealInput = true;
                    // while(input.Length % 4 != 0)
                    // {
                    //     input += "=";
                    // }
                    // byte[] byteArray = Convert.FromBase64String(input);
                    // byArray = byteArray;

                    // loadTexture = true;
                    // //writeToFile(input);


                }
            }
        }
        catch (Exception e)
        {
            error = e.Message;
            socketClosed = true;
        }
    }

    public bool ChangeStuff(string input)
    {
        inputPointsTCP = new Vector3[30];
        input = input.Substring(1);
        input = input.Substring(1); 
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
            // realInput = x[i];
            // logRealInput = true;
            string [] chx = x[i].Split(' ').ToArray();
            // realInput = chx[0] + " " + chx[1] + " " + chx[2];
            // logRealInput = true;
            float v0 = float.Parse(chx[0])/35;
            float v1 = float.Parse(chx[1])/35;
            float v2 = float.Parse(chx[2])/35;
            v2 = 0;
            // realInput = "floats: " + v0.ToString() + " " + v1.ToString() + " " + v2.ToString();
            Vector3 vec = new Vector3(v0, v1, v2);
            inputPointsTCP[i-1] = vec;
            realInput = "vectors: " + inputPointsTCP[i-1].x + " " + inputPointsTCP[i-1].y + " " + inputPointsTCP[i-1].z;
            logRealInput = true;
        }
        return true;
    }

    //private async Task readTCPDataAsync(DataReader reader)
    //{
    //    reader.InputStreamOptions = InputStreamOptions.None;

    //    // Read the length of the payload that will be received.
    //    byte[] payloadSize = new byte[(uint)BitConverter.GetBytes(0).Length];
    //    await reader.LoadAsync((uint)payloadSize.Length);
    //    reader.ReadBytes(payloadSize);


    //    // Read the payload.
    //    int size = BitConverter.ToInt32(payloadSize, 0);
    //    sizeOfBuffer = size;
    //    logSize = true;
    
    //    byte[] payload = new byte[size];
    //    await reader.LoadAsync((uint)size);
    //    reader.ReadBytes(payload);

    //    string data = Encoding.ASCII.GetString(payload);

    //    //write the data to file to see if yoya and image is recieved for sure
    //    writeToFile(payload);
    //    writeToFile(data);

    //    //set the public variable byArray to the payload image to be set in the main update routine
    //    byArray = payload;

    //    loadTexture = true;
    //    writeStringToFile = true;
       
    //}

    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    //private async Task readTCPDataAsync(DataReader reader)
    //{

    //    reader.InputStreamOptions = InputStreamOptions.Partial;

    //    uint numFileBytes = await reader.LoadAsync(reader.UnconsumedBufferLength);

    //    byArray = new byte[numFileBytes];

    //    reader.ReadBytes(byArray);
    //    texture.LoadImage(byArray);

    //}
#endif

    public bool Offset()
    {
        offset = inputPointsTCP[0] - Alignment.position;
        inputPointsTCP[0] = Alignment.position;

        for(int i = 1; i < inputPointsTCP.Length; i++)
        {
            inputPointsTCP[i] = inputPointsTCP[i] + offset;
        } 
        return true;
    }
    
    void writeToFile(byte[] bytes)
    {
        string path = Path.Combine(Application.persistentDataPath, "MyFile.txt");
        using (TextWriter writer = File.CreateText(path))
        {
            writer.Write("hey there frend this is working now whoooo!");
            writer.Write(bytes);
        }
    }
    void writeToFile(String str)
    {
        string path = Path.Combine(Application.persistentDataPath, "MyFile.txt");
        using (TextWriter writer = File.CreateText(path))
        {
            writer.Write("Hey there frend");
        }
    }
    public void LOG(string msg)
    {
        debugLogText.GetComponent<TextMesh>().text += "\n " + msg;
    }

   
    void Update()
    {
        if(logRealInput)
        {
            LOG("INPUT IS: " + realInput);
            logRealInput = false;
        }
        if(logInputSize)
        {
            LOG("IMAGESIZE IS : " + imageSize);
            logInputSize = false;
        }
        if(logSize)
        {
            LOG("SIZE IS : " + sizeOfBuffer);
            logSize = false;
        }
        if(socketClosed)
        {
            LOG(error);
            LOG("OOPS SOCKET CLOSED ");
            socketClosed = false;
        }
        if(writeStringToFile)
        {
            LOG("WRITTEN TO FILE");
            writeStringToFile = false;
        }
        if(loadTexture)
        {
            LOG("LOADING IMAGE CURRENTLY");
            LOG("LOADED IMAGE");
            loadTexture = false;
        }
        if(recievedData)
        {
            LOG("recieved data!");
            recievedData = false;
        }
  
    }

}