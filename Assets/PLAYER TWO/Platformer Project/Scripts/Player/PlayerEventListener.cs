using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    [AddComponentMenu("PLAYER TWO/Platformer/ Project/Player/Player Event Listener")]
    public class PlayerEventListener : MonoBehaviour
    {
        // 玩家实例调用
        public Player player;

        // 玩家事件集合(用于外部绑定回调)
        public PlayerEvents events;

        /// <summary>
        /// Unity 生命周期方法
        /// 在 Start 阶段调用，自动初始化玩家引用和事件回调。
        /// </summary>
        protected virtual void Start()
        {
            InitializePlayer();
            InitializeCallbacks();
        }

        /// <summary>
        /// 初始化玩家引用。
        /// 如果没有手动赋值 player,则会自动在父物体中寻找 Player 组件。
        /// </summary>
        protected virtual void InitializePlayer()
        {
            if(!player)
            {
                player = GetComponentInParent<Player>();
            }
        }

        /// <summary>
        /// 初始化玩家事件回调。
        /// 将 Player 内部的事件(如跳跃，死亡，受伤等)转发到本地的events中，方便在 Inspector 中自定义逻辑。
        /// </summary>
        public virtual void InitializeCallbacks()
        {
            // 解耦操作，当玩家触发相关事件时该脚本做出相应（绑定该脚本的物体将会在玩家做出某个行为时相应对应的动作）
            player.playerEvents.Onjump.AddListener(() => events.Onjump.Invoke());
            player.playerEvents.OnHurt.AddListener(() => events.OnHurt.Invoke());
            player.playerEvents.OnDie.AddListener(() => events.OnDie.Invoke());
            player.playerEvents.OnSpin.AddListener(() => events.OnSpin.Invoke());
            player.playerEvents.OnPickUp.AddListener(() => events.OnPickUp.Invoke());
            player.playerEvents.OnThrow.AddListener(() => events.OnThrow.Invoke());
            player.playerEvents.OnStompStarted.AddListener(()=>  events.OnStompStarted.Invoke());
            player.playerEvents.OnStompFalling.AddListener(()=> events.OnStompFalling.Invoke());
            player.playerEvents.OnStompLanding.AddListener(()=>events.OnStompLanding.Invoke());
            player.playerEvents.OnStompEnding.AddListener(()=> events.OnStompEnding.Invoke());
            player.playerEvents.OnLedgeGrabbed.AddListener(()=> events.OnLedgeGrabbed.Invoke());
            player.playerEvents.OnLedgeClimbing.AddListener(()=> events.OnLedgeClimbing.Invoke());
            player.playerEvents.OnAirDive.AddListener(() => events.OnAirDive.Invoke());
            player.playerEvents.OnBackflip.AddListener(()=> events.OnBackflip.Invoke());
        }
    }
}
