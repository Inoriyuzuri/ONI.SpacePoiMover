using HarmonyLib;
using KMod;
using UnityEngine;

namespace SpacePOIMover
{
    public class SpacePOIMoverMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            Debug.Log("[SpacePOIMover] Mod loaded!");
        }
    }
}
