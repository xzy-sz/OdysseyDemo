using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    [RequireComponent(typeof(Player))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Footsteps")]
    public class PlayerFootsteps : MonoBehaviour
    {
        [Header("General Settings")]
        public float footstepVolume = 0.3f;
        public float stepOffset = 1.25f;
        public AudioClip[] defaultFootsteps;
        public AudioClip[] defaultLandings;
        public Surface[] surfaces;

        protected Player m_player;
        protected AudioSource m_audio;
        protected Vector3 m_lastLateralPosition;

        /// <summary>存储不停地面类型的落地声映射</summary>
        protected Dictionary<string, AudioClip[]> m_landings = new Dictionary<string, AudioClip[]>();
        
        /// <summary>存储不同地面类型的脚步声映射</summary>
        protected Dictionary<string, AudioClip[]> m_footsteps = new Dictionary<string, AudioClip[]>();

        [System.Serializable]
        public class Surface
        {
            /// <summary>表示地面类型的标签</summary>
            public string tag;

            /// <summary>玩家在该地面上的脚步声音数组</summary>
            public AudioClip[] footsteps;

            /// <summary>玩家在该地面落地声音数组</summary>
            public AudioClip[] landings;
        }

        protected virtual void Start()
        {
            m_player = GetComponent<Player>();
            // 监听，落地的时候发出落地声
            m_player.entityEvents.OnGroundEnter.AddListener(Landing);

            if(!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }

            foreach(var surface in surfaces)
            {
                m_footsteps.Add(surface.tag, surface.footsteps);
                m_landings.Add(surface.tag,surface.landings);
            }
        }

        /// <summary>
        /// 播放落地音效（根据地面类型选择不同的音效）
        /// </summary>
        protected virtual void Landing()
        {
            if (!m_player.onWater)
            {
                if (m_landings.ContainsKey(m_player.groundHit.collider.tag))
                {
                    PlayRandomClip(m_landings[m_player.groundHit.collider.tag]);
                }
                else
                {
                    PlayRandomClip(defaultLandings);
                }
            }
        }

        protected virtual void Update()
        {
            if(m_player.isGrounded && m_player.states.IsCurrentOfType(typeof(WalkPlayerState)))
            {
                var position = transform.position;
                var lateralPosition = new Vector3(position.x,0,position.z);
                var distance = (m_lastLateralPosition - lateralPosition).magnitude;

                if(distance > stepOffset)
                {
                    if (m_footsteps.ContainsKey(m_player.groundHit.collider.tag))
                    {
                        PlayRandomClip(m_footsteps[m_player.groundHit.collider.tag]);
                    }
                    else
                    {
                        PlayRandomClip(defaultFootsteps);
                    }
                    m_lastLateralPosition = lateralPosition;
                }
            }
        }

        /// <summary>
        /// 从给定的音效数组中随机播放一个音效
        /// </summary>
        /// <param name="clips"></param>
        protected virtual void PlayRandomClip(AudioClip[] clips)
        {
            if(clips.Length > 0)
            {
                var index = Random.Range(0, clips.Length);
                m_audio.PlayOneShot(clips[index],footstepVolume);
            }
        }
    }
}
