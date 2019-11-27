using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Matchmaking
{
    public class MatchmakingClient : MonoBehaviour
    {
        // State object for receiving data from remote device.  
        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int MaxBufferSize = 512;
            public int gameType;
            // Receive buffer.  
            public byte[] buffer;
        }

        public string userID;
        public string hostPort = "15012";

        private const int port = 12001;
        //private const string hostName = "localhost";
        private const int headerLength = 8;

        private int findGameType;

        public void StartClient(int gameType)
        {
            try
            {
                findGameType = gameType;

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
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }


        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                Debug.Log("Socket connected to " + client.RemoteEndPoint.ToString());
                SendFindGame(client); 
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void SendFindGame(Socket client)
        {
            try
            {
                Debug.Log("Sending FindGame Packet");
                // Create FindGamePacket
                FindGamePacket findGamePacket = new FindGamePacket();
                findGamePacket.userID = userID;//SystemInfo.deviceUniqueIdentifier;
                findGamePacket.gameType = findGameType;
                string findGameStr = JsonUtility.ToJson(findGamePacket);

                // Create byte array to send to Matchmaking server
                byte[] byteData = new byte[headerLength + Encoding.ASCII.GetByteCount(findGameStr)];
                Encoding.ASCII.GetBytes(findGameStr.Length.ToString()).CopyTo(byteData, 0);
                Encoding.ASCII.GetBytes(findGameStr).CopyTo(byteData, headerLength);
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendFindGameCallback), client);

            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        private void SendFindGameCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Debug.Log("Sent " + bytesSent + " bytes to server.");
                ReceiveHostOrJoin(client);

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }



        private void ReceiveHostOrJoin(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;
                state.buffer = new byte[headerLength];

                // Begin receiving the header or the packet from the matchmaking server 
                client.BeginReceive(state.buffer, 0, headerLength, 0,
                    new AsyncCallback(ReceiveHostOrJoinBody), state);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void ReceiveHostOrJoinBody(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;

                // End previous receive
                state.workSocket.EndReceive(ar);

                int packetLength = int.Parse(Encoding.ASCII.GetString(state.buffer));
                state.buffer = new byte[StateObject.MaxBufferSize];

                state.workSocket.BeginReceive(state.buffer, 0, packetLength, 0,
                    new AsyncCallback(ReceiveHostOrJoinCallback), state);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        private void ReceiveHostOrJoinCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                client.EndReceive(ar);

                string recvPacketStr = Encoding.ASCII.GetString(state.buffer);
                Packet recvPacket = JsonUtility.FromJson<Packet>(recvPacketStr);
                Debug.Log(recvPacket.packetType);
                if (recvPacket.packetType == 2)
                {
                    Debug.Log("Joining: " + recvPacket.IP + ":" + recvPacket.PID);
                }
                else if (recvPacket.packetType == 3)
                {

                    SendJoin(client);
                }
                else
                {
                    Debug.Log("USERID: " + userID);
                    throw new System.InvalidOperationException("Invalid Packet Type!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void SendJoin(Socket client)
        {
            try
            {
                JoinPacket joinPacket = new JoinPacket();
                joinPacket.PID = hostPort;
                Debug.Log("IP: " + GetLocalIPAddress());
                joinPacket.IP = GetLocalIPAddress();
                string joinPacketStr = JsonUtility.ToJson(joinPacket);

                // Create byte array to send to Matchmaking server
                byte[] byteData = new byte[headerLength + Encoding.ASCII.GetByteCount(joinPacketStr)];
                Encoding.ASCII.GetBytes(joinPacketStr.Length.ToString()).CopyTo(byteData, 0);
                Encoding.ASCII.GetBytes(joinPacketStr).CopyTo(byteData, headerLength);
                client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendJoinCallback), client);

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        private void SendJoinCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Debug.Log("Sent " + bytesSent + " bytes to server.");

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
