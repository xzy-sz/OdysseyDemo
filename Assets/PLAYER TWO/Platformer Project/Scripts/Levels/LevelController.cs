using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Levels
{
    /// <summary>
    /// 关卡控制器
    /// 该类负责统一管理关卡内的常用操作，例如：
    /// - 结束关卡 / 退出关卡
    /// - 玩家重生 / 关卡重启
    /// - 增加金币 / 收集星星 / 结算分数
    /// - 暂停与恢复游戏
    /// 
    /// 它通过放置关卡功能类单例(LevelFinisher,LevelRespawner,LevelScore,LevelPauser)
    /// 来完成对应的操作，方便在其他地方统一调用，而不必直接操作这些类
    /// </summary>
    [AddComponentMenu("PLAYER TWO/Platformer Project/Levels/level Controller")]
    public class LevelController : MonoBehaviour
    {
        /// <summary>
        /// 关卡结束处理器
        /// 用于处理通关，退出关卡等操作。
        /// </summary>
        protected LevelFinisher m_finisher => LevelFinisher.Instance;

        /// <summary>
        /// 关卡计分系统
        /// </summary>
        protected LevelScore m_score => LevelScore.Instance;

        /// <summary>
        /// 关卡暂停控制器
        /// 同于控制游戏暂停和恢复
        /// </summary>
        protected LevelPauser m_pauser => LevelPauser.Instance;

        /// <summary>
        /// 关卡重生处理器
        /// 用于处理玩家复活，重新开始关卡等操作
        /// </summary>
        protected LevelRespawner m_respawner => LevelRespawner.Instance;

        // ============== 计分系统 ================

        /// <summary>
        /// 增加金币数量
        /// </summary>
        /// <param name="amount">增加的金币数量</param>
        public virtual void AddCoins(int amount)
        {
             m_score.coins += amount;
        }

        // =================== 重生与重启 ====================== //
        /// <summary>
        /// 让玩家复活
        /// </summary>
        /// <param name="consumeRetries"></param>
        public virtual void Respawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);


        // =================== 暂停与恢复 ===================== //

        /// <summary>
        /// 暂停或恢复游戏
        /// </summary>
        /// <param name="value">true为暂停，false 为恢复</param>
        public virtual void Pause(bool value) => m_pauser.Pause(value);

        /// <summary>
        /// 重新开始关卡
        /// </summary>
        public virtual void Restart() => m_respawner.Restart();

        /// <summary>
        /// 完成关卡
        /// </summary>
        public virtual void Finish() => m_finisher.Finish();

        /// <summary>
        /// 退出关卡(调用 LevelFinisher.Exit)
        /// </summary>
        public virtual void Exit() => m_finisher.Exit();
    }
}
