using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;
using Landfall.Modding;
using System.Reflection;
using UnityEngine.Localization;
namespace IamTheCaptainNow;

[LandfallPlugin]
public class CaptainModelSwap {
    public static Harmony harmony;
    public static string GUID = "AnthonyStai.IamTheCaptainNow";
    public static GameObject prefab;
    public static bool Enabled = true;
    public static string AssemblyDirectory {
        get {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }

    static CaptainModelSwap() {
        Debug.Log($"Loading {GUID} running in {AssemblyDirectory}");
        Debug.Log("Loading assetbundles");

        AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(AssemblyDirectory, "CaptainPrefab"));
        if (assetBundle == null) {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }

        prefab = assetBundle.LoadAsset<GameObject>("CaptainModel");

        assetBundle.Unload(false);

        harmony = new(GUID);
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(PlayerAnimationHandler))]
public class PlayerPatches {
    [HarmonyPatch(nameof(PlayerAnimationHandler.Awake))]
    [HarmonyPostfix]
    private static void AwakePostFix(PlayerAnimationHandler __instance) {
        if (!CaptainModelSwap.Enabled) return;
        GameObject clone = GameObject.Instantiate(CaptainModelSwap.prefab);
        clone.transform.parent = __instance.gameObject.transform.Find("Visual/");
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localEulerAngles = Vector3.zero;
        __instance.GetComponent<PlayerCharacter>().refs.playerModel = clone.transform;
        __instance.GetComponent<PlayerCharacter>().refs.handLeft = clone.transform.Find("Captain/Armature/Hip/Spine_1/Spine_2/Spine_3/Shoulder_L/Arm_L/Elbow_L/Hand_L");
        __instance.GetComponent<PlayerCharacter>().refs.handRight = clone.transform.Find("Captain/Armature/Hip/Spine_1/Spine_2/Spine_3/Shoulder_R/Arm_R/Elbow_R/Hand_R");

        PlayerStatusSFX sfx = __instance.GetComponent<PlayerStatusSFX>();
        sfx.boostStepRef = clone.transform.Find("Captain/SFX/Steps/StepSFX_2");
        sfx.boostJumpRef = clone.transform.Find("Captain/SFX/Jumps/Jump_Mini");
        sfx.boostLandRef = clone.transform.Find("Captain/SFX/Jumps/Land_Mini");
        GameObject zoeModel = __instance.gameObject.transform.Find("Visual/Courier_Retake").gameObject;
        zoeModel.SetActive(false);
        __instance.animator = clone.transform.Find("Captain").GetComponent<Animator>();

        Debug.Log("THE PLAYER HAS AWOKEN");
    }
    [HarmonyPatch(nameof(PlayerAnimationHandler.Start))]
    [HarmonyPrefix]
    private static bool UpdatePrefix(PlayerAnimationHandler __instance) {
        if (__instance.animator == null && CaptainModelSwap.Enabled) __instance.animator = __instance.transform.Find("Visual/CaptainModel(Clone)/Captain").GetComponent<Animator>();
        return true;
    }
}

[HarmonyPatch(typeof(CharacterShaderHandler))]
public class ShaderPatches {
    [HarmonyPatch(nameof(CharacterShaderHandler.Update))]
    [HarmonyPrefix]
    private static bool UpdatePrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.Start))]
    [HarmonyPrefix]
    private static bool StartPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.HandleShield))]
    [HarmonyPrefix]
    private static bool HandleShieldPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.ResetCharacter))]
    [HarmonyPrefix]
    private static bool ResetCharacterPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.ApplyEffect))]
    [HarmonyPrefix]
    private static bool ApplyEffectPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.HandleEffect))]
    [HarmonyPrefix]
    private static bool HandleEffectPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.AddShaderEffect))]
    [HarmonyPrefix]
    private static bool AddShaderEffectPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
    [HarmonyPatch(nameof(CharacterShaderHandler.SetVector))]
    [HarmonyPrefix]
    private static bool SetVectorPrefix(PlayerAnimationHandler __instance) {
        return !CaptainModelSwap.Enabled;
    }
}
[HasteSetting]
public class ModelSwapSetting : OffOnSetting, IExposedSetting {
    public override void ApplyValue() {
        CaptainModelSwap.Enabled = base.Value == OffOnMode.ON;
    }

    public string GetCategory() => "Mods";

    // Token: 0x0600062A RID: 1578 RVA: 0x00024CD8 File Offset: 0x00022ED8
    public override OffOnMode GetDefaultValue() {
        return OffOnMode.ON;
    }

    public LocalizedString GetDisplayName() => new("CaptainAntStai", "settingName");

    // Token: 0x0600062B RID: 1579 RVA: 0x00024CEC File Offset: 0x00022EEC
    public override List<LocalizedString> GetLocalizedChoices() {
        return new List<LocalizedString>
        {
            new LocalizedString("Settings", "DisabledGraphicOption"),
            new LocalizedString("Settings", "EnabledGraphicOption")
        };
    }
}