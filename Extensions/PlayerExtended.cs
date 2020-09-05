﻿using UnityEngine;

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
            new GameObject($"__{nameof(ModMiner)}__").AddComponent<ModMiner>();
        }
    }
}
