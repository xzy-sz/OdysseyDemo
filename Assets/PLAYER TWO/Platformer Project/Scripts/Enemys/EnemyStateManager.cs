using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys
{
    [RequireComponent(typeof(Enemy))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy State Manager")]
    public class EnemyStateManager : EntityStateManager<Enemy>
    {
        [ClassTypeName(typeof(EnemyState))]
        public string[] states;

        // 重写基类方法，根据 states 字符串数组创建对应的 EnemyState 状态列表
        protected override List<EntityState<Enemy>> GetStateList()
        {
            return EnemyState.CreateListFromStringArray(states);
        }
    }
}
