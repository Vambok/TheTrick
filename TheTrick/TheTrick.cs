using HarmonyLib;//
using OWML.Common;//
using OWML.ModHelper;//
using System;//
using System.Collections.Generic;//
using System.Reflection;//
using UnityEngine;//


namespace TheTrick;
/* USEFUL INFOS
Cam relative pos: 0 0,8496 0,15

TODO:
- Prisoner :
    - After Prisoner's raft trip
        - Add "sunset" item far away
            - find justification
        - Put slide under sunset
            - the slide depicts an owlk jumping out of the raft to the hatch
    - Bridge & lightwell
X        - deIncline bridge: all underwater
X        - Fix floor
X        - Fix audio
X        - code 3 good/bad close/open the hatch
        - deactivate fluiddetector over hole
X    - Make Artifact interactible
X        - Destroy base GO
X        - Get a lantern clone : PLantern
X        - Make flame lit
X        - If water lit-PLantern: deactivate Prisoner, it's lantern, and the trigger zone.
X        - If Prisoner dies: SetLit false

        - delete additionnal flameEffect when pLantern off

    - Cases enter DW: (+ case is the only case where Prisoner is out alive)
        - Dead
            - with PLantern-lit: onEntry #>dead
            - (else: onExit:dead)
        - Alive
            + with OwnLantern if PL-lit: onExit if P-Out dead
            - with PLantern-lit: onEntry #>out
            - (else: onExit:alive)
    - If no TimeLoop and in the + case: no end
        - if meditates: time jump
            -

# Wake in DW as Prisoner
    - Wake as protoartifact (short time)
    - Wakeup point inside Prisoner's head
        - Put player cam inside head
        - Deactivate player's body
        - Filter simulation glitches
    - Prisoner:
        - puts hand on forehead
    - Out of the SIM
    - If go DW with it again: insta DW exit
    - If go DW with own lantern: Pmind glitches
*/

public class TheTrick : ModBehaviour {
    public static TheTrick Instance;
    MVBMod_Animator myAnimator;

    public void Awake() {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        // You won't be able to access OWML's mod helper in Awake.
    }

    public void Start() {
        // Starting here, you'll have access to OWML's mod helper.
        ModHelper.Console.WriteLine($"newstone, newtext, MovRel follow, noGamePad, secret gear lightwell", MessageType.Success); // Version checker
        new Harmony("Vambok.TheTrick").PatchAll(Assembly.GetExecutingAssembly());
        //OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
        LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
    }

    public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene) {
        if(newScene != OWScene.SolarSystem) return;
        ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
        ModHelper.Console.WriteLine("PlayerCamera name: " + GameObject.FindWithTag("MainCamera").name, MessageType.Success);
        myAnimator = GameObject.FindWithTag("MainCamera").AddComponent<MVBMod_Animator>();
        ModHelper.Console.WriteLine("MVBMod_Animator loaded!", MessageType.Success);
        Lightwell_manager();
        ModHelper.Console.WriteLine("Lightwell_manager loaded!", MessageType.Success);
        Prisoner_manager();
        ModHelper.Console.WriteLine("Prisoner_manager loaded!", MessageType.Success);
        Prisoner_trick();
        ModHelper.Console.WriteLine("Prisoner_trick loaded!", MessageType.Success);
        SlideReelBuilder();
        ModHelper.Console.WriteLine("Slide reel made!", MessageType.Success);
    }


    void Lightwell_manager() { // Creates secret area in upper Dreamzone 4 (the lightwell)
        // Bridge on water
        Transform secretBridge = GameObject.Instantiate(GameObject.Find("Sector_DreamZone_3").transform.Find("Structures_DreamZone_3").Find("Invisible_Bridge_Lower").gameObject, GameObject.Find("Simulation_DreamZone_4").transform).transform;
        secretBridge.localPosition = new Vector3(-15.8f, -0.6f, -69.2f);
        secretBridge.localEulerAngles = new Vector3(0, 25, 0);
        secretBridge.localScale = new Vector3(1.1f, 1, 1);
        secretBridge.gameObject.name = "MVBMod_LightwellBridge";
        secretBridge.gameObject.AddComponent<MVBTheTrick_tag>();
        Transform tartopom = GameObject.Instantiate(GameObject.Find("Simulation_DreamZone_3").transform.Find("Interactibles_DreamZone_3").GetChild(4), secretBridge);
        tartopom.localPosition = Vector3.zero;
        tartopom.gameObject.name = "MVBMod_LightwellBridge_SimMesh";
        // TEMP cheat lamp&bridge
        GameObject.Find("Prefab_IP_DreamLanternItem_2 (1)").transform.localPosition = new Vector3(-187, 203, 164.5f);
        GameObject.Find("Prefab_IP_DreamLanternItem_2 (1)").GetComponent<SphereCollider>().enabled = true;
        //GameObject.Instantiate(secretBridge, secretBridge).localScale = new Vector3(100, 1, 100);
        // Destination of water warpzone (down to island3's bridge)
        Transform secretWarpDest = GameObject.Find("ElevatorWarp_Lower_4").transform;
        secretWarpDest = GameObject.Instantiate(secretWarpDest, secretWarpDest.parent);
        secretWarpDest.localPosition = new Vector3(-36, 80.5f, -47);
        secretWarpDest.localEulerAngles = 180 * Vector3.up;
        secretWarpDest.gameObject.name = "MVBMod_LightwellWarpDest";
        secretWarpDest.gameObject.AddComponent<MVBTheTrick_tag>();
        // Warpzone in upper water
        Transform secretWarp = GameObject.Find("ElevatorWarp_Upper_4").transform;
        secretWarp = GameObject.Instantiate(secretWarp, secretWarp.parent);
        secretWarp.localPosition = new Vector3(39.3f, -12.5f, 58.4f);
        secretWarp.localEulerAngles = Vector3.zero;
        secretWarp.gameObject.name = "MVBMod_LightwellWarp";
        secretWarp.gameObject.AddComponent<MVBTheTrick_tag>();
        DreamElevatorWarpVolume warpControl = secretWarp.gameObject.GetComponent<DreamElevatorWarpVolume>();
        warpControl._attachedBody = warpControl._destinationBody;
        warpControl._destinationTransform = secretWarpDest;
        // Lightwell geo upper
        Transform terrainSource = GameObject.Find("Sector_DreamZone_4").transform.Find("Geo_DreamZone_4_Upper").Find("Terrain_DreamZone4_UpperLevel");
        // Lightwell secret gear's floor
        Transform floorGear = GameObject.Instantiate(terrainSource.Find("DockIsland"), terrainSource);
        floorGear.localPosition = new Vector3(-46, -1.4f, -39.5f);
        floorGear.localEulerAngles = new Vector3(0, 70, 0);
        floorGear.localScale = new Vector3(0.5f, 0.3f, 0.5f);
        floorGear.gameObject.name = "MVBMod_LightwellFloor";
        floorGear.gameObject.AddComponent<MVBTheTrick_tag>();
        // Lightwell center
        Transform floorHole = GameObject.Instantiate(terrainSource.Find("DZ4_PrisonIsland_Ext_Walls"), terrainSource);
        floorHole.localPosition = new Vector3(-39.3f, -4.7f, -58.4f);
        floorHole.localEulerAngles = new Vector3(0, 201.5f, 356);
        floorHole.localScale = Vector3.one / 2f;
        floorHole.gameObject.name = "MVBMod_LightwellHole";
        floorHole.gameObject.AddComponent<MVBTheTrick_tag>();
        // Sounds of the hatch
        floorHole.gameObject.SetActive(false);
        OWAudioSource audioSource = floorHole.gameObject.AddComponent<OWAudioSource>();
        audioSource.SetTrack(OWAudioMixer.TrackName.Environment);
        audioSource._audioLibraryClip = AudioType.NomaiDoorStopBig;
        audioSource._clipArrayLength = 15;
        audioSource._clipArrayIndex = -1;
        floorHole.gameObject.SetActive(true);
        audioSource.playOnAwake = false;
        // Hatch
        tartopom = GameObject.Find("Airlock_RaftHouse_Arrival").transform.Find("Structure_IP_Airlock");
        Transform hatch = GameObject.Instantiate(tartopom.Find("Floor"), floorHole);
        hatch.localPosition = new Vector3(-1.3f, 6.3f, -16.1f);
        hatch.localEulerAngles = new Vector3(340, 0, 4f);
        hatch.localScale = Vector3.one;
        GameObject theHatch = hatch.gameObject;
        theHatch.name = "MVBMod_LightwellHatch";
        hatch = GameObject.Instantiate(tartopom.Find("Floor_col"), hatch);
        hatch.localPosition = Vector3.zero;
        hatch.localEulerAngles = Vector3.zero;
        hatch.localScale = Vector3.one;
        hatch.gameObject.name = "MVBMod_LightwellHatch_Collider";
        // Hidden controller gear
        Transform secretGear = GameObject.Instantiate(GameObject.Find("UpperDestination_DreamZone_4").transform.Find("Prefab_IP_DW_GearInterface_Standing"), GameObject.Find("Interactibles_DreamZone_4_Upper").transform);
        secretGear.localPosition = new Vector3(-33.1f, 0.6f, -77.5f);
        secretGear.localEulerAngles = new Vector3(0, 65, 0);
        secretGear.gameObject.name = "MVBMod_LightwellGear";
        secretGear.gameObject.AddComponent<MVBTheTrick_tag>();
        secretGear.Find("GreenLight_Top").gameObject.SetActive(false);
        secretGear = secretGear.Find("InteractReceiver_Gear");
        GearInterfaceEffects interfaceEffect = secretGear.GetComponent<GearInterfaceEffects>();
        InteractReceiver interactReceiver = secretGear.GetComponent<InteractReceiver>();
        interactReceiver._screenPrompt._text = "<CMD> Close lightwell hatch";
        interactReceiver._textID = (UITextType)2;
        bool wellClosed = false;
        Action secretGearAction = (() => {
            float soundLength = audioSource.PlayOneShot(AudioType.NomaiDoorStartBig).length;
            if(wellClosed) {
                interfaceEffect.AddRotation(135f);
                interactReceiver._screenPrompt._text = "<CMD> Close lightwell hatch";
                interactReceiver._textID = (UITextType)2;
                wellClosed = false;
                myAnimator.Animate(theHatch.transform, new Vector3(-1.3f, 6.3f, -16.1f), soundLength, new Vector3(-20, 0, 4f));
            } else {
                interfaceEffect.AddRotation(-135f);
                audioSource.PlayDelayed(soundLength);
                interactReceiver._screenPrompt._text = "<CMD> Open lightwell hatch";
                interactReceiver._textID = (UITextType)3;
                wellClosed = true;
                myAnimator.Animate(theHatch.transform, new Vector3(-1.3f, 9.2f, -0.3f), soundLength, new Vector3(360, 0, 4f));
            }
            interactReceiver._resetInteractionTime = Time.time + soundLength;
        });
        interactReceiver.OnPressInteract += (() => {
            AlarmBridgeController lightFromWell = GameObject.Find("AlarmBridgeController").GetComponent<AlarmBridgeController>();
            if(wellClosed) {
                lightFromWell.OnClose();
            } else {
                lightFromWell.OnOpen();
            }
            secretGearAction();
        });
        interactReceiver.EnableInteraction();
        EclipseCodeController4 codeTotemController = GameObject.Find("Interactibles_Island_C").transform.Find("Prefab_IP_DW_CodeTotem").gameObject.GetComponent<EclipseCodeController4>();
        codeTotemController.OnOpen += (() => {
            if(!wellClosed) { secretGearAction(); }
        });
        codeTotemController.OnClose += (() => {
            if(wellClosed) { secretGearAction(); }
        });
    }

    void Prisoner_manager() { // Deals with the changes to the true Prisoner's Lantern
        Transform mummy = GameObject.Find("Prefab_IP_SleepingMummy_v2 (PRISONER)").transform;
        mummy.Find("Mummy_IP_ArtifactAnim").gameObject.SetActive(false);
        Transform prisonersLantern = GameObject.Instantiate(GameObject.Find("Prefab_IP_DreamLanternItem_2 (1)").transform, mummy);
        prisonersLantern.localPosition = new Vector3(0, 1.63f, 0.66f);
        prisonersLantern.localEulerAngles = Vector3.zero;
        prisonersLantern.gameObject.name = "MVBMod_PLantern";
        prisonersLantern.gameObject.AddComponent<MVBTheTrick_tag>().type = "DreamLanternController";
        DreamLanternController pLanternController = prisonersLantern.gameObject.GetComponent<DreamLanternController>();
        GameObject.Find("GhostDirector_Prisoner").GetComponent<PrisonerDirector>().OnPrisonerDeparted += (() => {
            pLanternController.enabled = true;
            pLanternController.SetLit(false);
            DialogueConditionManager.SharedInstance.SetConditionState("LINKED_TO_PRISONER", false);
            pLanternController.gameObject.GetComponent<DreamLanternItem>()._lanternType = DreamLanternType.Functioning;
            GlobalMessenger.FireEvent("PlayerResurrection");
        });
        // Awake-wtf-bug's corrective patch:
        float[] normalConcealerStartPos = [-0.0001f, -0.1018f, -0.2137f, 0.2263f, 0.1016f];
        prisonersLantern = prisonersLantern.Find("Props_IP_Artifact_ViewModel").Find("Focuser");
        for(int i = 0;i < 5;i++) {
            prisonersLantern.GetChild(i).localEulerAngles -= 90 * Vector3.forward;
            pLanternController._focuserPetalsBaseEulerAngles[i].z -= 90f;
            pLanternController._concealerCoversStartPos[i].y = normalConcealerStartPos[i];
        }
        pLanternController._concealerRootsBaseScale[0] = Vector3.one;
        pLanternController._concealerRootsBaseScale[1] = Vector3.one;
    }

    void Prisoner_trick() { // Manages the Prisoner's d*ck move
        ModHelper.Console.WriteLine("trick init", MessageType.Success);
        GameObject.Find("Prefab_IP_VisionTorchItem").transform.Find("Prefab_IP_VisionTorchProjector").Find("VisionBeam").Find("TriggerVolume_MindProjector").gameObject.GetComponent<OWTriggerVolume>().OnEntry += ((GameObject hitObj) => {
            if(hitObj.CompareTag("PlayerCameraDetector")) { // ProbeDetector TEMP tag, "PlayerCameraDetector"
                ModHelper.Console.WriteLine("trick started", MessageType.Success);
                Transform prisonerLantern = GameObject.Find("PrisonerFillLight").transform.parent;
                Transform target = Locator.GetDreamWorldController()._playerLantern.gameObject.transform;
                Vector3 pivot = prisonerLantern.parent.parent.position;
                //Get current world-space look-origin position
                Vector3 worldLookOrigin = prisonerLantern.TransformPoint(Vector3.down * 0.3f);
                //Compute the desired look rotation from look-origin to target
                Quaternion desiredRotation = Quaternion.LookRotation(target.position - worldLookOrigin, Vector3.up);
                //Compute delta rotation from current to desired
                Quaternion deltaRotation = desiredRotation * Quaternion.Inverse(prisonerLantern.rotation);
                //Apply rotation around pivot
                Vector3 targetPosition = pivot + deltaRotation * (prisonerLantern.position - pivot);
                //Send to smoothing systems
                ModHelper.Console.WriteLine("trick calculated", MessageType.Success);
                MVBMod_Animator myAnimator = Locator.GetPlayerCamera().gameObject.AddComponent<MVBMod_Animator>();
                myAnimator.Animate(prisonerLantern, prisonerLantern.parent.InverseTransformPoint(targetPosition), 2f, prisonerLantern.parent.InverseTransformRotation(desiredRotation).eulerAngles, (int)MVBMod_Animator.AnimationType.pLantern_exchange, (int)MVBMod_Animator.AnimationType.pLantern_reset, 2);
                ModHelper.Console.WriteLine("trick animation sent", MessageType.Success);
                myAnimator.Animate(prisonerLantern, Vector3.zero, 2f, Vector3.zero, (int)MVBMod_Animator.AnimationType.pLantern_reset, (int)MVBMod_Animator.AnimationType.Global_reset, 1);
                myAnimator.Animate(prisonerLantern, Vector3.zero, 0, (int)MVBMod_Animator.AnimationType.Global_reset);
                DreamLanternController pDreamLanterncontroller = prisonerLantern.gameObject.GetComponent<DreamLanternController>();
                myAnimator.AddAction((int)MVBMod_Animator.AnimationType.pLantern_exchange, () => pDreamLanterncontroller.MoveTowardFocus(1f, 2f), 2);
                myAnimator.AddAction((int)MVBMod_Animator.AnimationType.pLantern_exchange, () => pDreamLanterncontroller.MoveTowardFocus(0f, 1f), 2, 1);
                myAnimator.AddAction((int)MVBMod_Animator.AnimationType.pLantern_reset, () => pDreamLanterncontroller.MoveTowardFocus(0f, 1f));
                myAnimator.AddAction((int)MVBMod_Animator.AnimationType.Global_reset, () => { myAnimator.ResetAll(); DialogueConditionManager.SharedInstance.SetConditionState("LINKED_TO_PRISONER", false); });
                myAnimator.SetTrigger((int)MVBMod_Animator.AnimationType.pLantern_exchange);
                ModHelper.Console.WriteLine("trick animation started", MessageType.Success);
            }
        });
    }

    public static GameObject SlideReelBuilder(GameObject parent, Sector sector, Texture2D[] slideTextures) {
        GameObject reelGO = Instantiate(GameObject.Find("Prefab_IP_Reel_1_Story_Complete"), parent.transform);
        reelGO.name = "Custom_SlideReel";
        SlideReelItem slideReel = reelGO.GetComponent<SlideReelItem>();
        slideReel.SetSector(sector);
        slideReel.SetVisible(true);
        reelGO.GetComponent<OWCollider>()._physicsRemoved = false;

        // Create slides
        Slide[] slides = new Slide[slideTextures.Length];
        for(int i = 0;i < slideTextures.Length;i++) slides[i] = new() { textureOverride = slideTextures[i] };

        // Assign to container
        SlideCollectionContainer container = reelGO.GetComponent<SlideCollectionContainer>() ?? reelGO.AddComponent<SlideCollectionContainer>();
        container.slideCollection = new(slideTextures.Length) {
            slides = slides,
            streamingAssetIdentifier = ""
        };
        slideReel._slideCollectionContainer = container;

        // Create and assign reel texture (4x4 grid max 16 slides)
        Texture2D reelTexture = MakeReelTexture(slideTextures);
        MeshRenderer slidesBack = reelGO.transform.Find("Slides_Back")?.GetComponent<MeshRenderer>();
        MeshRenderer slidesFront = reelGO.transform.Find("Slides_Front")?.GetComponent<MeshRenderer>();
        if(slidesBack && slidesFront) {
            slidesBack.material.mainTexture = reelTexture;
            slidesBack.material.SetTexture("_EmissionMap", reelTexture);
            slidesFront.material.mainTexture = reelTexture;
            slidesFront.material.SetTexture("_EmissionMap", reelTexture);
        }
        reelGO.SetActive(true);
        return reelGO;

        static Texture2D MakeReelTexture(Texture2D[] slideTextures) {
            int tileSize = slideTextures[0].width;
            int gridSize = 4;
            Texture2D reelTexture = new(tileSize * gridSize, tileSize * gridSize, TextureFormat.RGBA32, false);
            for(int i = 0;i < Mathf.Min(slideTextures.Length, 16);i++) {
                int x = (i % gridSize) * tileSize;
                int y = (i / gridSize) * tileSize;
                reelTexture.SetPixels(x, y, tileSize, tileSize, slideTextures[i].GetPixels());
            }
            reelTexture.Apply();
            reelTexture.name = "CustomSlideAtlas";
            return reelTexture;
        }
    }
}

[HarmonyPatch]
public class MyPatchClass {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Sector), nameof(Sector.OnEntry))]
    public static void Sector_OnEntry_Postfix(Sector __instance) { // Loading new meshes, colliders and custom functions
        foreach(MVBTheTrick_tag marker in __instance.gameObject.GetComponentsInChildren<MVBTheTrick_tag>(true)) {
            bool newObj = true;
            if(marker.type != null) {
                switch(marker.type) {
                case ("DreamLanternController"):
                    DreamLanternItem pLanternItem = marker.gameObject.GetComponent<DreamLanternItem>();
                    pLanternItem._lanternType = DreamLanternType.Malfunctioning;
                    pLanternItem.onPickedUp += new OWEvent<OWItem>.OWCallback((OWItem pLanternItem) => { pLanternItem.enabled = true; });
                    DreamLanternController pLanternController = marker.gameObject.GetComponent<DreamLanternController>();
                    pLanternController.enabled = true;
                    pLanternController.SetLit(true);
                    pLanternController._focus = -0.1f;
                    LanternFluidDetector fluidDetector = marker.gameObject.transform.Find("FluidDetector").gameObject.GetComponent<LanternFluidDetector>();
                    fluidDetector.OnEnterFluidType += ((FluidVolume.Type fluidType) => {
                        if(fluidType == FluidVolume.Type.WATER && pLanternController.IsLit()) {
                            pLanternController.enabled = true;
                            pLanternController.SetLit(false);
                            DialogueConditionManager.SharedInstance.SetConditionState("LINKED_TO_PRISONER", false);
                            pLanternItem._lanternType = DreamLanternType.Functioning;
                            Transform prison = GameObject.Find("Sector_PrisonCell").transform;
                            prison.Find("Effects_PrisonCell").GetChild(3).gameObject.SetActive(false);
                            prison.Find("Ghosts_PrisonCell").gameObject.SetActive(false);
                            prison.Find("Interactibles_PrisonCell").Find("PrisonerSequence").Find("LanternTableSocket").gameObject.SetActive(false);
                        }
                    });
                    fluidDetector.GetShape().SetActivation(true);
                    marker.type = null;
                    break;
				default:
					break;
                }
            }
            if(newObj) {
                foreach(Renderer renderer in marker.gameObject.GetComponentsInChildren<Renderer>(true)) {
                    renderer.enabled = true;
                }
                foreach(Collider collider in marker.gameObject.GetComponentsInChildren<Collider>(true)) {
                    collider.enabled = true;
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.OnEnterDreamWorld))]
    public static void DreamLanternItem_OnEnterDreamWorld_Postfix(DreamLanternItem __instance) { // Activating simulation glitch
        if(__instance.gameObject.GetComponent<MVBTheTrick_tag>() != null && __instance._lanternType == DreamLanternType.Malfunctioning) {
            //            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", GameObject.Find("MVBMod_PrisonerView").GetComponent<OWCamera>());
            OWInput.ChangeInputMode(InputMode.None);
            Locator.GetPlayerTransform().GetComponent<PlayerResources>().ToggleInvincibility();
            Locator.GetDeathManager().ToggleInvincibility();
        } else if(DialogueConditionManager.SharedInstance.GetConditionState("LINKED_TO_PRISONER")) {
            OWCamera playerCamera = Locator.GetPlayerCamera();
            MVBMod_Animator myAnimator = playerCamera.gameObject.GetComponent<MVBMod_Animator>();
            //            myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.To_Prisoners_head, () => { GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", GameObject.Find("MVBMod_PrisonerView").GetComponent<OWCamera>()); });
            myAnimator.Animate(__instance.transform, __instance.transform.localPosition, 0, (int)MVBMod_Animator.AnimationType.To_Prisoners_head, (int)MVBMod_Animator.AnimationType.From_Prisoners_head, -2f);
            //            myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.From_Prisoners_head, () => { GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", playerCamera); });
            myAnimator.Animate(__instance.transform, __instance.transform.localPosition, 0, (int)MVBMod_Animator.AnimationType.From_Prisoners_head, (int)MVBMod_Animator.AnimationType.To_Prisoners_head, -6f);
            myAnimator.SetTrigger((int)MVBMod_Animator.AnimationType.To_Prisoners_head, 5f);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.OnExitDreamWorld))]
    public static void DreamLanternItem_OnExitDreamWorld_Postfix(DreamLanternItem __instance) {
        if(__instance.gameObject.GetComponent<MVBTheTrick_tag>() != null && __instance._lanternType == DreamLanternType.Malfunctioning) {
            Locator.GetPlayerTransform().GetComponent<PlayerResources>().ToggleInvincibility();
            Locator.GetDeathManager().ToggleInvincibility();
            GameObject.Find("MVBMod_PrisonerView").GetComponent<OWCamera>().enabled = false;
            __instance._lanternType = DreamLanternType.Nonfunctioning;
            Transform flameEffect = __instance.gameObject.transform.Find("Props_IP_Artifact_ViewModel");
            flameEffect = GameObject.Instantiate(flameEffect.Find("Flame_ViewModel"), flameEffect);
            flameEffect.localPosition = 0.2f * Vector3.up;
            flameEffect.localEulerAngles = 180 * Vector3.forward;
            flameEffect.localScale = Vector3.one;
            GameObject.Instantiate(flameEffect, __instance.gameObject.transform.Find("Props_IP_Artifact"));
            OWInput.ChangeInputMode(InputMode.Character);
            DialogueConditionManager.SharedInstance.SetConditionState("LINKED_TO_PRISONER", true);
            __instance._fluidDetector.GetShape().SetActivation(true);
            __instance._lanternController.SetLit(true);
        } else if(DialogueConditionManager.SharedInstance.GetConditionState("LINKED_TO_PRISONER")) {
            Locator.GetPlayerCamera().gameObject.GetComponent<MVBMod_Animator>().ResetAll(); // Clear all animations
        }
    }
}

public class MVBMod_Animator : MonoBehaviour {
    private class MvbAnimation(Transform target, Vector3 toPos, Vector3 toRot, float duration, int followId = 0, float delay = 0f) {
        public int followId = followId;
        public Transform target = target;
        public Vector3 fromPos = target.localPosition, toPos = toPos;
        public Vector3 fromRot = target.localEulerAngles, toRot = toRot;
        public float startTime = Time.time, duration = duration, delay = delay;
        public bool Update() {
            if(Time.time >= startTime) {
                float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Time.time - startTime) / duration));
                target.localPosition = Vector3.Lerp(fromPos, toPos, smooth);
                target.localEulerAngles = Vector3.Lerp(fromRot, toRot, smooth);
                if(smooth >= 1f) return true;
            }
            return false;
        }
    }

    private struct MvbAnimAction(Action action, int interval, int offset) {
        public Action action = action;
        public int interval = interval;
        public int offset = offset;
    }

    readonly Dictionary<int, MvbAnimation> activeAnimations = [];
    readonly Dictionary<int, float> idRunning = [];
    readonly Dictionary<int, MvbAnimAction> scheduledActions = [];
    void Update() {
        if(scheduledActions.ContainsKey(0))
            scheduledActions[0].action();
        if(activeAnimations.ContainsKey(0))
            if(activeAnimations[0].Update()) {
                if(activeAnimations[0].followId > 0)
                    idRunning[activeAnimations[0].followId] = RollDelay(activeAnimations[0].delay);
                activeAnimations.Remove(0);
            }
        List<int> completed = [];
        foreach((int id, float delay) in idRunning) {
            if(scheduledActions.TryGetValue(id, out MvbAnimAction act) && ((Time.frameCount - act.offset) % act.interval == 0))
                act.action();
            if(activeAnimations.TryGetValue(id, out MvbAnimation anim) && Time.time >= delay)
                if(anim.Update()) {
                    completed.Add(id);
                    if(anim.followId > 0)
                        idRunning[anim.followId] = RollDelay(anim.delay);
                }
        }
        foreach(int id in completed) idRunning.Remove(id);
    }
    float RollDelay(float delay) => Time.time + ((delay < 0f) ? UnityEngine.Random.Range(-1f, -0.1f) * delay : delay);

    public void Animate(Transform target, Vector3 toPos, float duration, Vector3 toRot, int id = 0, int nextId = 0, float delay = 0f) {
        activeAnimations[id] = new MvbAnimation(target, toPos, toRot, duration, nextId, delay);
    }
    public void Animate(Transform target, Vector3 toPos, float duration, int id = 0, int nextId = 0, float delay = 0f) {
        Animate(target, toPos, duration, target.localEulerAngles, id, nextId, delay);
    }
    public void Animate(Transform target, float duration, Vector3 toRot, int id = 0, int nextId = 0, float delay = 0f) {
        Animate(target, target.localPosition, duration, toRot, id, nextId, delay);
    }
    public void SetTrigger(int triggerId, float delay = 0f) {
        if(triggerId > 0) {
            idRunning[triggerId] = Time.time + delay;
        } else if(triggerId < 0) {
            idRunning.Remove(-triggerId);
        } else {
            idRunning.Clear();
        }
    }
    public void AddAction(int id, Action action, int interval = 1, int offset = 0) {
        scheduledActions[id] = new MvbAnimAction(action, Mathf.Max(interval, 1), offset);
    }
    public void ResetAll() {
        activeAnimations.Clear();
        idRunning.Clear();
        scheduledActions.Clear();
    }

    public enum AnimationType {
        None = 0,
        Global_reset,
        To_Prisoners_head,
        From_Prisoners_head,
        pLantern_exchange,
        pLantern_reset
    }
}

public class MVBTheTrick_tag : MonoBehaviour {
    public string type = null;
}
