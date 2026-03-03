using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity
{
    /// <summary>
    /// 泛型抽象类，用于定义实体的属性数据。
    /// 继承自ScriptableObject,方便通过Unity编辑器创建和管理数据资产。
    /// T 是具体的ScriptableObject类型，限定属性数据的具体实现。
    /// </summary>
    /// <typeparam name="T">继承自ScriptableObject的具体属性类型</typeparam>
    public abstract class EntityStats<T> : ScriptableObject where T : ScriptableObject
    {
    }
}
