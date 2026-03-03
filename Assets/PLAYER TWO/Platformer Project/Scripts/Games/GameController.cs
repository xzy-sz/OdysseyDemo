using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Games
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Games/Game Controller")]
    public class GameController : MonoBehaviour
    {
        protected Game m_game => Game.Instance;

        protected GameLoader m_loader => GameLoader.Instance;

        public virtual void AddRetries(int amount) => m_game.retries += amount;

        public virtual void LoadScene(string scene) => m_loader.Load(scene);
    }
}
