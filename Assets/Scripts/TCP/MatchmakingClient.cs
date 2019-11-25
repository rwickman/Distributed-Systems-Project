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
            // Receive buffer.  
            public byte[] buffer;
        }

        public string userID;
        public string hostPort = "15012";

        private const int port = 12001;
        //private const string hostName = "localhost";
        private const int headerLength = 8;
        private Packet recvPacket;



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
                // Create FindGamePacket
                FindGamePacket findGamePacket = new FindGamePacket();
                findGamePacket.userID = userID;//SystemInfo.deviceUniqueIdentifier;
                findGamePacket.gameType = gameType;
                string findGameStr = JsonUtility.ToJson(findGamePacket);

                Send(client, findGameStr);
                sendDone.WaitOne();

                ReceiveHostOrJoin(client);
                receiveDone.WaitOne();

                if (recvPacket.packetType == 2)
                {
                    Debug.Log("Joining: " + recvPacket.IP + ":" + recvPacket.PID);
                }
                else if (recvPacket.packetType == 3)
                {
                    JoinPacket joinPacket = new JoinPacket();
                    joinPacket.PID = hostPort;
                    Debug.Log("IP: " + GetLocalIPAddress());
                    joinPacket.IP = GetLocalIPAddress();
                    string joinPacketStr = JsonUtility.ToJson(joinPacket);
                    Send(client, joinPacketStr);
                }
                else
                {
                    throw new System.InvalidOperationException("Invalid Packet Type!");
                }

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

                // Complete the connection.  
                client.EndConnect(ar);

                Debug.Log("Socket connected to " + client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void Send(Socket client, string data)
        {
            // Create byte array to send to Matchmaking server
            byte[] byteData = new byte[headerLength + Encoding.ASCII.GetByteCount(data)];
            Encoding.ASCII.GetBytes(data.Length.ToString()).CopyTo(byteData, 0);
            Encoding.ASCII.GetBytes(data).CopyTo(byteData, headerLength);

            // Begin sending the body size  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Debug.Log("Sent " + bytesSent + " bytes to server.");

                // Signal that all bytes have been sent.  
                sendDone.Set();
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
                    new AsyncCallback(ReceiveHostOrJoinCallback), state);
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

                // Signal that all bytes have been received.  
                receiveDone.Set();


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
