using Assets.PLAYER_TWO.Platformer_Project.Scripts.Levels;
using System;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Games
{
    /// <summary>
    /// 表示一个游戏关卡的结结构数据
    /// 用于记录关卡的状态(是否解锁，收集金币数量，通关时间，星星收集情况等)
    /// 该类主要用于存储与读取关卡数据，以及格式化关卡时间显示
    /// </summary>
    [Serializable]      // 允许此类被序列化
    public class GameLevel
    {
        /// <summary>
        /// 是否锁定此关卡
        /// true 表示锁定（无法进入）
        /// </summary>
        public bool locked;

        /// <summary>
        /// 关卡所对应的场景名称（Unity Scence 名称）
        /// 用于场景切换时加载
        /// </summary>
        public string scene;

        /// <summary>
        /// 关卡的显示名称
        /// 例如：“第一关 - 森林冒险”
        /// </summary>
        public string name;

        /// <summary>
        /// 关卡描述信息
        /// 可用于 UI 中展示关卡的背景故事或提示
        /// </summary>
        public string description;

        /// <summary>
        /// 关卡预览图片
        /// 可用于 UI 中展示关卡缩略图
        /// </summary>
        public Sprite image;

        /// <summary>
        /// 已收集的金币数量
        /// 可通过属性进行读取或写入
        /// </summary>
        public int coins {  get; set; }

        /// <summary>
        /// 该关卡的通关时间
        /// 记录最快通关时间
        /// </summary>
        public float time { get; set; }

        /// <summary>
        /// 每关星星总数
        /// </summary>
        public static readonly int StarsPerLevel = 3;

        /// <summary>
        /// 星星收集状态数组
        /// </summary>
        public bool[] stars { get; set; } = new bool[StarsPerLevel];

        /// <summary>
        /// 从给定的 LevelData 加载此关卡的状态
        /// </summary>
        /// <param name="data"></param>
        public virtual void LoadState(LevelData data)
        {
            locked = data.locked;
            coins = data.coins;
            time = data.time;
            stars = data.stars;
        }

        /// <summary>
        /// 将当前 GameLevel 对象转化为 LevelData 对象。
        /// </summary>
        /// <returns></returns>
        public virtual LevelData ToData()
        {
            return new LevelData()
            {
                locked = this.locked,
                coins = this.coins,
                time = this.time,
                stars = this.stars
            };
        }

        /// <summary>
        /// 将给定的时间(秒)格式化未 00'00"00 的字符串格式
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormattedTime(float time)
        {
            // 计算分钟数
            var minutes = Mathf.FloorToInt(time / 60f);

            // 计算剩余的秒数
            var seconds = Mathf.FloorToInt(time % 60f);

            // 计算毫秒
            var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

            // 返回格式化后的字符串
            return minutes.ToString("0") + "'" + seconds.ToString("00") + "\"" + milliseconds.ToString("00");
        }

    }
}