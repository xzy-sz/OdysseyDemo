using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    /// <summary>
    ///  玩家状态管理器
    /// </summary>
    [RequireComponent(typeof(Player))] // 要求该脚本所在的物体必须挂有Player脚本
    public class PlayerStateManager : EntityStateManager<Player>
    {
        /// <summary>
        /// 玩家状态类的字符串数组
        /// </summary>
        [ClassTypeName(typeof(PlayerState))]
        public string[] states;

        /// <summary>
        /// 重写基类方法，获取玩家的状态列表
        /// 会将states中的字符串类名数组转化为真正的状态对象列表。
        /// </summary>
        /// <returns>返回一个包含所有状态的ListEntityState-Player-</returns>
        protected override List<EntityState<Player>> GetStateList()
        {
            return PlayerState.CreateListFromStringArray(states);
        }
    }
}
