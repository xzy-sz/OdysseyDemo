using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Levels
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Levels/Level Finisher")]
    public class LevelFinisher : Singleton<LevelFinisher>
    {
        /// <summary>
        /// 当关卡完成时触发的事件(可在 Inspector 中绑定回调)
        /// </summary>
        public UnityEvent OnFinish;

        /// <summary>
        /// 当关卡退出时触发的事件(可在 Inspector 中绑定回调)
        /// </summary>
        public UnityEvent OnExit;

        /// <summary>
        /// 是否在通关时解锁下一个关卡。
        /// </summary>
        public bool unlockNextLevel;

        /// <summary>
        /// 通关后要加载的下一个场景名
        /// </summary>
        public string nextScene;

        /// <summary>
        /// 退出关卡后要加载的场景名
        /// </summary>
        public string exitScene;

        /// <summary>
        /// 加载场景前的延迟时间(秒)
        /// </summary>
        public float loadingDelay = 1f;

        // 一些方便访问的单调引用
        protected Game m_game => Game.Instance;
        protected Level m_level => Level.Instance;
        protected  LevelScore m_score => LevelScore.Instance;
        protected LevelPauser m_pauser => LevelPauser.Instance;
        protected GameLoader m_loader => GameLoader.Instance;
        protected Fader m_fader => Fader.Instance;

        /// <summary>
        /// 调用通关流程
        /// 会停止当前所有协程并启动 FinishRoutine
        /// </summary>
        public virtual void Finish()
        {
            StopAllCoroutines();
            StartCoroutine(FinishRoutine());
        }

        protected virtual IEnumerator FinishRoutine()
        {
            // 取消暂停，并禁止后续暂停
            m_pauser.Pause(false);
            m_pauser.canPause = false;

            // 停止计时
            m_score.stopTime = true;

            // 禁用玩家输入
            m_level.player.inputs.enabled = false;

            // 延迟一段时间
            yield return new WaitForSeconds(loadingDelay);

            // 如果设置了通关解锁下一关，则解锁
            if (unlockNextLevel)
            {
                m_game.UnlockNextLevel();
            }

            // 解锁鼠标
            Game.LockCursor(false);

            // 保存并合并关卡分数
            m_score.Consolidate();

            // 加载下一个场景
            m_loader.Load(nextScene);

            // 触发通关事件
            OnFinish?.Invoke();
        }

        /// <summary>
        /// 调用退出流程
        /// 不会保存分数
        /// </summary>
        public virtual void Exit()
        {
            StopAllCoroutines();
            StartCoroutine(ExitRoutine());
        }

        /// <summary>
        /// 关卡退出的协程流程
        /// 不保存分数，只做退出场景切换.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ExitRoutine()
        {
            // 取消暂停，并禁止后续暂停
            m_pauser.Pause(false);
            m_pauser.canPause = false;

            // 禁用玩家输入
            m_level.player.inputs.enabled = false;

            // 延迟一段时间(可用于播放退出动画)
            yield return new WaitForSeconds(loadingDelay);

            // 解锁鼠标光标
            Game.LockCursor(false);

            // 加载退出场景
            m_loader.Load(exitScene);

            // 触发退出事件
            OnExit?.Invoke();
        }
    }
}
