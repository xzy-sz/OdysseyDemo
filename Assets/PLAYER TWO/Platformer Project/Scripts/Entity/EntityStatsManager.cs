using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.MPE;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity
{
    public abstract class EntityStatsManager<T> : MonoBehaviour where T : EntityStats<T>
    {
        public T[] stats;

        /// <summary>
        /// 当前激活的属性实例
        /// </summary>
        public T current { get; protected set; }

        /// <summary>
        /// 切换当前属性到指定索引的属性集
        /// </summary>
        /// <param name="to"></param>
        public virtual void Change(int to)
        {
            if(to >= 0 && to <= stats.Length)
            {
                if(current != stats[to])
                {
                    current = stats[to];
                }
            }
        }

        protected virtual void Start()
        {
            if(stats.Length > 0)
            {
                current = stats[0];
            }
        }
    }
}
