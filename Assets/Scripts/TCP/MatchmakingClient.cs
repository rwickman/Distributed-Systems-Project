using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MatchmakingClient : MonoBehaviour
{
    // State object for receiving data from remote device.  
    public class StateObject
    {  
        // Client socket.  
        public Socket workSocket = null;  
        // Size of receive buffer.  
        public const int MaxBufferSize = 512;
        // Receive buffer.  
        public byte[] buffer;
        // Received data string.  
        public StringBuilder sb = new StringBuilder();  
    } 

    public string userID;
    private const int port = 12001;
    //private const string hostName = "localhost";
    private const int headerLength = 8;

    // ManualResetEvent instances signal completion.  
    private ManualResetEvent connectDone = new ManualResetEvent(false);
    private ManualResetEvent sendDone = new ManualResetEvent(false);
    private ManualResetEvent receiveDone = new ManualResetEvent(false);

    public void StartClient(int gameType)
    { 
        try
        {
            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            Debug.Log("About to connect");
            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();


            Debug.Log("Sending FindGame Packet");
            SendFindGame(client, gameType);



        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            Debug.Log("Socket connected to " + client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void SendFindGame(Socket client, int gameType)
    {

        // Create FindGamePacket
        FindGamePacket findGamePacket = new FindGamePacket();
        findGamePacket.userID = userID;//userID.ToString();//SystemInfo.deviceUniqueIdentifier;
        findGamePacket.gameType = gameType;

        string findGameStr = JsonUtility.ToJson(findGamePacket);
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(findGameStr);
        byte[] dataSize = new byte[headerLength];
        System.Text.Encoding.ASCII.GetBytes(findGameStr.Length.ToString()).CopyTo(dataSize, 0);

        StateObject state = new StateObject();
        state.workSocket = client;
        state.buffer = byteData;

        // Begin sending the body size  
        client.BeginSend(dataSize, 0, headerLength, 0, new AsyncCallback(SendFindGameBody), state);
    }

    private void SendFindGameBody(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        state.workSocket.BeginSend(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(SendFindGameCallback), state.workSocket);
    }

    private void SendFindGameCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    /*
    private void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket   
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Get the rest of the data.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                // Signal that all bytes have been received.  
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    */

    /*
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

            // Read how many bytes the next packet will be
            dataSize = new Byte[headerLength];
            int bytesRead = stream.Read(dataSize, 0, headerLength);
            string rspDataLengthStr = System.Text.Encoding.ASCII.GetString(dataSize, 0, bytesRead);
            int rspDataLength = Int32.Parse(rspDataLengthStr);
            // Read next packet
            data = new Byte[rspDataLength];
            stream.Read(data, 0, rspDataLength);
            string rspStr = System.Text.Encoding.ASCII.GetString(data, 0, rspDataLength);

            // Decode string to JSON
            Packet rspJson = JsonUtility.FromJson<Packet>(rspStr);

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }*/
}
