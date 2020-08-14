using UnityEngine;

namespace ModMiner
{
    /// <summary>
    /// Inject modding interface into game only in single player mode
    /// </summary>
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject("__ModMiner__").AddComponent<ModMiner>();
            new GameObject("__ModManager__").AddComponent<ModManager>();
        }
    }
}
