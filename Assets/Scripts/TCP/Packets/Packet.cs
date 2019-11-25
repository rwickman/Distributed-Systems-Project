﻿using UnityEngine;

namespace Matchmaking
{
    [System.Serializable]
    public class Packet
    {
        public int packetType;
        public string userID;
        public int gameType;
        public string PID;
        public string IP;
    }
}
