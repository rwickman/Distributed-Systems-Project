using UnityEngine;

namespace Matchmaking
{
    [System.Serializable]
    public class FindGamePacket
    {
        public int packetType = 1;
        public string userID;
        public int gameType;
    }
}
