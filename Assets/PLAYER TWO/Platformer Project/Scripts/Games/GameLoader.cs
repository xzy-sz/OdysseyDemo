using Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Games
{
    [AddComponentMenu("PLAYER TWO/Platfoemer Project/Games/Game Loader")] 
    public class GameLoader : Singleton<GameLoader>
    {
        /// <summary>
        /// 当任何加载过程开始执行时触发的条件
        /// 可在 Inspector 面板中绑定方法，例如播放加载动画，暂停游戏等
        /// </summary>
        public UnityEvent OnLoadStart;

        /// <summary>
        /// 当任何加载过程结束时触发的事件
        /// 可绑定方法，例如关闭加载动画，恢复游戏等
        /// </summary>
        public UnityEvent OnLoadFinish;

        /// <summary>
        /// 加载界面 UI 控制器
        /// </summary>
        public UIAnimator loadingScreen;


        [Header("Minimum Time")]
        public float startDelay = 1f;
        public float finishDelay = 1f;

        /// <summary>
        /// 当前是否正在加载
        /// </summary>
        public bool isLoading { get; protected set; }

        /// <summary>
        /// 当前加载进度(0-1)
        /// </summary>
        public float loadingProgress { get; protected set; }

        /// <summary>
        /// 当前场景的名称
        /// </summary>
        public string currentScene => SceneManager.GetActiveScene().name;

        /// <summary>
        /// 加载指定名称的场景
        /// 会在一下条件满足时执行
        /// - 当前没有正在加载的场景
        /// - 要加载的场景与当前场景不同
        /// </summary>
        /// <param name="scene"></param>
        public virtual void Load(string scene)
        {
            if(!isLoading && (currentScene != scene))
            {
                StartCoroutine(LoadRoutine(scene));
            }
        }

        /// <summary>
        /// 重新加载当前场景
        /// </summary>
        public virtual void ReLoad()
        {
            StartCoroutine(LoadRoutine(currentScene));
        }

        /// <summary>
        /// 场景加载的协程流程。
        /// 包含加载前延迟，加载过程进度记录，加载完成延迟，UI ，动画显示等步骤。
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        protected virtual IEnumerator LoadRoutine(string scene)
        {
            // 触发加载开始事件
            OnLoadStart?.Invoke();

            // 标记为加载中
            isLoading = true;

            // 激活加载界面并显示动画
            loadingScreen.SetActive(true);
            loadingScreen.Show();

            // 加载前延迟
            yield return new WaitForSeconds(startDelay);

            // 异步加载场景
            var operation = SceneManager.LoadSceneAsync(scene);
            loadingProgress = 0;

            while (!operation.isDone)
            {
                loadingProgress = operation.progress;   // 取值范围通常是0-0.9，完成时才为 1；
                yield return null;
            }

            loadingProgress = 1;    // 加载完成
            yield return new WaitForSeconds(finishDelay);   // 加载完成后的延迟

            // 标记加载结束
            isLoading = false;
            // 隐藏加载界面
            loadingScreen.Hide();
            // 触发加载结束事件
            OnLoadFinish?.Invoke();
        }
    }
}
