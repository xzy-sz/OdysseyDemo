using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using System;
using System.Linq;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Levels
{
    [Serializable]
    public class LevelData
    {
        /// <summary>
        /// 关卡是否被锁定
        /// true 表示该关卡尚未解锁
        /// </summary>
        public bool locked;

        /// <summary>
        /// 在该关卡中收集到的金币数量
        /// </summary>
        public int coins;

        /// <summary>
        /// 通关耗时
        /// </summary>
        public float time;

        /// <summary>
        /// 星星收集状态数组
        /// </summary>
        public bool[] stars = new bool[GameLevel.StarsPerLevel];

        /// <summary>
        /// 获取已收集的星星数量
        /// </summary>
        /// <returns></returns>
        public int CollectedStars()
        {
            // 统计 stars 数组值为 true 的个数
            return stars.Where((star) => star).Count();
        }
    }
}
