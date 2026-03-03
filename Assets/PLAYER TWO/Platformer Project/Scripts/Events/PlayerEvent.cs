using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using System;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Events
{
    [Serializable]
    public class PlayerEvent : UnityEvent<Player>
    {
    }
}
