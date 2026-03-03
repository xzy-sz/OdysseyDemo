using Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Games
{
    /// <summary>
    /// 游戏存档管理器(单例模式)。
    /// 负责将游戏数据保存到不同的存储介质。
    /// 并支持读取，删除和批量获取存档。
    /// 支持三种存档方式：
    /// - Binary(二进制文件存储，体积小，无法直接读取)
    /// - JSON （文本可读，方便调试）.
    /// - PlayerPrefs （存储在注册表或本地配置文件中）。
    /// </summary>
    [AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Saver")]
    public class GameSaver : Singleton<GameSaver>
    {
        /// <summary>
        /// 存档模式
        /// </summary>
        public enum Mode 
        {
            Binary,
            JSON,
            PlayerPrefs
        }

        /// <summary>
        /// 当前使用的存档模式
        /// </summary>
        public Mode mode = Mode.Binary;

        /// <summary>
        /// 存档文件的基础文件名
        /// </summary>
        public string fileName = "save";

        /// <summary>
        /// 二进制文件的扩展名
        /// </summary>
        public string binaryFileExtension = "data";

        /// <summary>
        /// 存档槽位总数
        /// 游戏可同时存在的最大存档数
        /// </summary>
        protected static readonly int TotalSlots = 5;

        /// <summary>
        /// 读取所有存档槽位的游戏数据
        /// 返回一个长度为 TotalSlots 的数组。
        /// </summary>
        /// <returns></returns>
        public virtual GameData[] LoadList()
        {
            var list = new GameData[TotalSlots];

            for(int i = 0;i < TotalSlots; i++)
            {
                var data = Load(i);

                if(data != null)
                {
                    list[i] = data;
                }
            }
            return list;
        }

        public virtual GameData Load(int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                    return LoadBinary(index);
                case Mode.JSON:
                    return LoadJSON(index);
                case Mode.PlayerPrefs:
                    return LoadPlayerPrefs(index);
            }
        }

        /// <summary>
        /// 保存游戏数据到指定的存档槽位
        /// 根据当前 mode 选择保存方式
        /// </summary>
        /// <param name="data">要保存的游戏数据对象</param>
        /// <param name="index">存档槽位编号</param>
        public virtual void Save(GameData data,int index)
        {
            switch(mode)
            {
                default:
                case Mode.Binary:
                    SaveBinary(data,index);
                    break;
                case Mode.JSON:
                    SaveJSON(data,index);
                    break;
                case Mode.PlayerPrefs:
                    SavePlayerPrefs(data,index);
                    break;
            }
        }

        /// <summary>
        /// 删除指定存档槽位的数据
        /// </summary>
        /// <param name="index">存档槽位编号</param>
        public virtual void Delete(int index)
        {
            switch (mode)
            {
                default :
                case Mode.Binary:
                case Mode.JSON:
                    DeleteFile(index);
                    break;
                case Mode.PlayerPrefs:
                    DeletePlayerPrefs(index);
                    break;
            }
        }

        #region

        /// <summary>
        /// 删除指定槽位的存档文件
        /// </summary>
        /// <param name="index"></param>
        protected virtual void DeleteFile(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        #endregion

        #region 文件路径

        /// <summary>
        /// 获取指定存储槽位的文件路径
        /// 根据当前 mode 自动选择扩展名.
        /// 存储路径使用 Application.persistentDataPath(持久化目录)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual string GetFilePath(int index)
        {
            var extension = mode == Mode.JSON ? "json" : binaryFileExtension;
            return Application.persistentDataPath + $"/{fileName}_{index}.{extension}";
        }

        #endregion

        #region Binary 存档方式

        /// <summary>
        /// 读取二进制数据文件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual GameData LoadBinary(int index)
        {
            var path = GetFilePath(index);
            if (File.Exists(path))
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(path, FileMode.Open);
                var data = formatter.Deserialize(stream);
                stream.Close();
                return data as GameData;
            }

            return null;
        }

        /// <summary>
        /// 以二进制格式吧保存数据到文件
        /// 这种方式存储空间小，但不方便查看
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        protected virtual void SaveBinary(GameData data,int index)
        {
            var path = GetFilePath(index);
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }

        #endregion

        #region JSON 存档方式
        protected  virtual void SaveJSON(GameData data,int index)
        {
            var json = data.ToJson();
            var path = GetFilePath(index);
            File.WriteAllText(path, json);
        }

        protected virtual GameData LoadJSON(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                var json =  File.ReadAllText(path);
                return GameData.FromJson(json);
            }
            return null;
        }

        #endregion

        #region PlayerPrefs 存档方式
        /// <summary>
        /// 将数据保存到 PlayerPrefs （JSON格式字符串）
        /// </summary>
        protected virtual void SavePlayerPrefs(GameData data,int index)
        {
            var json = data.ToJson() ;
            var key = index.ToString() ;
            PlayerPrefs.SetString(key, json);
        }

        /// <summary>
        /// 从 PlayerPrefs 读取数据
        /// </summary>
        /// <returns></returns>
        protected virtual GameData LoadPlayerPrefs(int index)
        {
            var key = index.ToString() ;

            if (PlayerPrefs.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);
                return GameData.FromJson(json);
            }
            return null;
        }

        /// <summary>
        /// 删除 PlayerPrefs 中的存档数据。
        /// </summary>
        /// <param name="index"></param>
        protected virtual void DeletePlayerPrefs(int index)
        {
            var key = index.ToString();

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        #endregion
    }
}
