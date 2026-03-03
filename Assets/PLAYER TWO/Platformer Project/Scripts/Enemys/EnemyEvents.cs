using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys
{
    [Serializable]
    public class EnemyEvents
    {
        /// <summary>
        /// 当玩家金进入敌人视野的时候触发
        /// </summary>
        public UnityEvent OnPlayerSpotted;

        /// <summary>
        /// 当玩家离开敌人视野的时候触发
        /// </summary>
        public UnityEvent OnPlayerScaped;

        /// <summary>
        /// 当敌人与玩家接触的时候触发
        /// </summary>
        public UnityEvent OnPlayerContact;

        /// <summary>
        /// 当敌人受到伤害时触发
        /// </summary>
        public UnityEvent OnDamage;

        /// <summary>
        /// 当敌人死亡时触发
        /// </summary>
        public UnityEvent OnDie;

    }
}
