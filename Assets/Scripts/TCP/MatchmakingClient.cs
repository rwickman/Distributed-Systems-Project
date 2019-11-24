using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class MatchmakingClient : MonoBehaviour
{
    public string userID;
    private const int portNum = 12001;
    private const string hostName = "localhost";
    private const int headerLength = 8;

    public void Connect(int gameType)
    {
        try
        {
            TcpClient client = new TcpClient(hostName, portNum);
            NetworkStream stream = client.GetStream();

            // Create FindGamePacket
            FindGamePacket findGamePacket = new FindGamePacket();
            findGamePacket.userID = userID;//userID.ToString();//SystemInfo.deviceUniqueIdentifier;
            findGamePacket.gameType = gameType;
            string findGameStr = JsonUtility.ToJson(findGamePacket);

            // Encode the json
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(findGameStr);
            Byte[] dataSize = new Byte[headerLength];
            System.Text.Encoding.ASCII.GetBytes(data.Length.ToString()).CopyTo(dataSize, 0);


            // Send the FindGamePacket
            stream.Write(dataSize, 0, headerLength);
            stream.Write(data, 0, data.Length);



        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }
}
