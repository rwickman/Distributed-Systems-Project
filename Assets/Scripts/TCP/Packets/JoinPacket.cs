using UnityEngine;
namespace Matchmaking
{
    [System.Serializable]
    public class JoinPacket
    {
        public int packetType;
        public string PID;
        public string IP;
    }
}
