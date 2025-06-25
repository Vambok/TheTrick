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
        //PlayerData.SetPersistentCondition("SOLANUM_FOLLOW", false);
        //PlayerData.SetPersistentCondition("STONE_ENTERED_TIMELOOPCORE", false);
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
        secretBridge.gameObject.AddComponent<MVBMod_tag>();
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
        secretWarpDest.gameObject.AddComponent<MVBMod_tag>();
        // Warpzone in upper water
        Transform secretWarp = GameObject.Find("ElevatorWarp_Upper_4").transform;
        secretWarp = GameObject.Instantiate(secretWarp, secretWarp.parent);
        secretWarp.localPosition = new Vector3(39.3f, -12.5f, 58.4f);
        secretWarp.localEulerAngles = Vector3.zero;
        secretWarp.gameObject.name = "MVBMod_LightwellWarp";
        secretWarp.gameObject.AddComponent<MVBMod_tag>();
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
        floorGear.gameObject.AddComponent<MVBMod_tag>();
        // Lightwell center
        Transform floorHole = GameObject.Instantiate(terrainSource.Find("DZ4_PrisonIsland_Ext_Walls"), terrainSource);
        floorHole.localPosition = new Vector3(-39.3f, -4.7f, -58.4f);
        floorHole.localEulerAngles = new Vector3(0, 201.5f, 356);
        floorHole.localScale = Vector3.one / 2f;
        floorHole.gameObject.name = "MVBMod_LightwellHole";
        floorHole.gameObject.AddComponent<MVBMod_tag>();
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
        secretGear.gameObject.AddComponent<MVBMod_tag>();
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
                myAnimator.MvbAnimator(theHatch.transform, new Vector3(-1.3f, 6.3f, -16.1f), soundLength, new Vector3(-20, 0, 4f));
            } else {
                interfaceEffect.AddRotation(-135f);
                audioSource.PlayDelayed(soundLength);
                interactReceiver._screenPrompt._text = "<CMD> Open lightwell hatch";
                interactReceiver._textID = (UITextType)3;
                wellClosed = true;
                myAnimator.MvbAnimator(theHatch.transform, new Vector3(-1.3f, 9.2f, -0.3f), soundLength, new Vector3(360, 0, 4f));
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
        prisonersLantern.gameObject.AddComponent<MVBMod_tag>().type = "DreamLanternController";
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
                // STEP 1: Get current world-space look-origin position
                Vector3 worldLookOrigin = prisonerLantern.TransformPoint(Vector3.down * 0.3f);
                // STEP 2: Compute the desired look rotation from look-origin to target
                Quaternion desiredRotation = Quaternion.LookRotation(target.position - worldLookOrigin, Vector3.up);
                // STEP 3: Compute delta rotation from current to desired
                Quaternion deltaRotation = desiredRotation * Quaternion.Inverse(prisonerLantern.rotation);
                // STEP 4: Apply rotation around pivot
                Vector3 targetPosition = pivot + deltaRotation * (prisonerLantern.position - pivot);
                // STEP 5: Send to smoothing systems
                ModHelper.Console.WriteLine("trick calculated", MessageType.Success);
                MVBMod_Animator myAnimator = Locator.GetPlayerCamera().gameObject.AddComponent<MVBMod_Animator>();
                myAnimator.MvbAnimator(prisonerLantern, prisonerLantern.parent.InverseTransformPoint(targetPosition), 2f, prisonerLantern.parent.InverseTransformRotation(desiredRotation).eulerAngles, (int)MVBMod_Animator.AnimationType.pLantern_exchange, (int)MVBMod_Animator.AnimationType.pLantern_reset, 2);
                ModHelper.Console.WriteLine("trick animation sent", MessageType.Success);
                myAnimator.MvbAnimator(prisonerLantern, Vector3.zero, 2f, Vector3.zero, (int)MVBMod_Animator.AnimationType.pLantern_reset, (int)MVBMod_Animator.AnimationType.Global_reset, 1);
                myAnimator.MvbAnimator(prisonerLantern, Vector3.zero, 0, (int)MVBMod_Animator.AnimationType.Global_reset);
                DreamLanternController pDreamLanterncontroller = prisonerLantern.gameObject.GetComponent<DreamLanternController>();
                myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.pLantern_exchange, () => pDreamLanterncontroller.MoveTowardFocus(1f, 2f), 2);
                myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.pLantern_exchange, () => pDreamLanterncontroller.MoveTowardFocus(0f, 1f), 2, 1);
                myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.pLantern_reset, () => pDreamLanterncontroller.MoveTowardFocus(0f, 1f));
                myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.Global_reset, () => { myAnimator.MvbAnimator(); DialogueConditionManager.SharedInstance.SetConditionState("LINKED_TO_PRISONER", false); });
                myAnimator.MvbAnimator((int)MVBMod_Animator.AnimationType.pLantern_exchange);
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
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(TitleScreenAnimation), nameof(TitleScreenAnimation.Update))]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) { // Fuck gamepads
        return new CodeMatcher(instructions).MatchForward(false,
            new CodeMatch(i => i.opcode == System.Reflection.Emit.OpCodes.Ldc_R4 && Convert.ToInt16(i.operand) == 6),
            new CodeMatch(System.Reflection.Emit.OpCodes.Add),
            new CodeMatch(i => i.opcode == System.Reflection.Emit.OpCodes.Stfld && ((FieldInfo)i.operand).Name == "_fadeOutGamepadTime")
        ).Repeat(match => match.SetOperandAndAdvance(0f)).Instructions();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Sector), nameof(Sector.OnEntry))]
    public static void Sector_OnEntry_Postfix(Sector __instance) { // Loading new meshes, colliders and custom functions
        foreach(MVBMod_tag marker in __instance.gameObject.GetComponentsInChildren<MVBMod_tag>(true)) {
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
        if(__instance.gameObject.GetComponent<MVBMod_tag>() != null && __instance._lanternType == DreamLanternType.Malfunctioning) {
            //            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", GameObject.Find("MVBMod_PrisonerView").GetComponent<OWCamera>());
            OWInput.ChangeInputMode(InputMode.None);
            Locator.GetPlayerTransform().GetComponent<PlayerResources>().ToggleInvincibility();
            Locator.GetDeathManager().ToggleInvincibility();
        } else if(DialogueConditionManager.SharedInstance.GetConditionState("LINKED_TO_PRISONER")) {
            OWCamera playerCamera = Locator.GetPlayerCamera();
            MVBMod_Animator myAnimator = playerCamera.gameObject.GetComponent<MVBMod_Animator>();
            //            myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.To_Prisoners_head, () => { GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", GameObject.Find("MVBMod_PrisonerView").GetComponent<OWCamera>()); });
            myAnimator.MvbAnimator(__instance.transform, __instance.transform.localPosition, 0, (int)MVBMod_Animator.AnimationType.To_Prisoners_head, (int)MVBMod_Animator.AnimationType.From_Prisoners_head, -2f);
            //            myAnimator.MvbBonusAction((int)MVBMod_Animator.AnimationType.From_Prisoners_head, () => { GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", playerCamera); });
            myAnimator.MvbAnimator(__instance.transform, __instance.transform.localPosition, 0, (int)MVBMod_Animator.AnimationType.From_Prisoners_head, (int)MVBMod_Animator.AnimationType.To_Prisoners_head, -6f);
            myAnimator.MvbAnimator((int)MVBMod_Animator.AnimationType.To_Prisoners_head, 5f);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.OnExitDreamWorld))]
    public static void DreamLanternItem_OnExitDreamWorld_Postfix(DreamLanternItem __instance) {
        if(__instance.gameObject.GetComponent<MVBMod_tag>() != null && __instance._lanternType == DreamLanternType.Malfunctioning) {
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
            Locator.GetPlayerCamera().gameObject.GetComponent<MVBMod_Animator>().MvbAnimator(); // Clear all animations
        }
    }
}

public class MVBMod_Animator : MonoBehaviour {
    class MvbAnimation(Transform toAnimate, Vector3 toWhere, float timeLength, Vector3 toAngle, int theId, int theFollowId, float theDelay) {
        public Transform toMove = toAnimate;
        public Vector3 start = toAnimate.localPosition;
        public Vector3 goal = toWhere;
        public float startTime = Time.time;
        public float duration = timeLength;
        public Vector3 rotStart = toAnimate.localEulerAngles;
        public Vector3 rotGoal = toAngle;
        public int id = theId;
        public int followId = theFollowId;
        public float delay = theDelay;
    }
    List<MvbAnimation> animationQueue = [];
    Dictionary<int, float> encountered = [];
    Dictionary<int, Tuple<Action, int, int>> buddies = [];

    private void FixedUpdate() {
        foreach((int theId, Tuple<Action, int, int> theAction) in buddies) {
            if(theId == 0 || encountered.ContainsKey(theId)) {
                if((Time.frameCount - theAction.Item3) % theAction.Item2 == 0) {
                    theAction.Item1();
                }
            }
        }
        if(animationQueue.Count > 0) {
            for(int i = 0;i < animationQueue.Count;i++) {
                if(animationQueue[i].id == 0 || encountered.TryGetValue(animationQueue[i].id, out animationQueue[i].startTime)) {
                    if(Time.time - animationQueue[i].startTime >= animationQueue[i].duration) {
                        animationQueue[i].toMove.localPosition = animationQueue[i].goal;
                        animationQueue[i].toMove.localEulerAngles = animationQueue[i].rotGoal;
                        if(animationQueue[i].id == 0) {
                            if(animationQueue[i].followId > 0) {
                                encountered.Add(animationQueue[i].followId, Time.time + ((animationQueue[i].delay < 0) ? UnityEngine.Random.Range(-1f, -0.1f) * animationQueue[i].delay : animationQueue[i].delay));
                            }
                            animationQueue.RemoveAt(i);
                            i--;
                        } else {
                            encountered.Remove(animationQueue[i].id);
                            if(animationQueue[i].followId > 0) {
                                encountered.Add(animationQueue[i].followId, Time.time + ((animationQueue[i].delay < 0) ? UnityEngine.Random.Range(-1f, -0.1f) * animationQueue[i].delay : animationQueue[i].delay));
                            }
                        }
                    } else if(Time.time >= animationQueue[i].startTime) {
                        animationQueue[i].toMove.localPosition = Vector3.Lerp(animationQueue[i].start, animationQueue[i].goal, Mathf.SmoothStep(0f, 1f, (Time.time - animationQueue[i].startTime) / animationQueue[i].duration));
                        animationQueue[i].toMove.localEulerAngles = Vector3.Lerp(animationQueue[i].rotStart, animationQueue[i].rotGoal, Mathf.SmoothStep(0f, 1f, (Time.time - animationQueue[i].startTime) / animationQueue[i].duration));
                    }
                }
            }
        }
    }

    public void MvbBonusAction(int theId, Action theAction, int module = 1, int offset = 0) { // if theId=0 -> infinite repetitions of theAction each frame!!
        buddies.Add(theId, new Tuple<Action, int, int>(theAction, Mathf.Max(module, 1), offset));
    }

    public void MvbAnimator(Transform toAnimate, Vector3 toWhere, float timeLength, Vector3 toAngle, int id = 0, int followId = 0, float delay = 0) {
        animationQueue.Add(new MvbAnimation(toAnimate, toWhere, timeLength, toAngle, id, followId, delay));
    }
    public void MvbAnimator(Transform toAnimate, Vector3 toWhere, float timeLength, int id = 0, int followId = 0, float delay = 0) {
        MvbAnimator(toAnimate, toWhere, timeLength, toAnimate.localEulerAngles, id, followId, delay);
    }
    public void MvbAnimator(Transform toAnimate, float timeLength, Vector3 toAngle, int id = 0, int followId = 0, float delay = 0) {
        MvbAnimator(toAnimate, toAnimate.localPosition, timeLength, toAngle, id, followId, delay);
    }
    public void MvbAnimator(int followId, float delay = 0f) {
        if(followId > 0) {
            encountered.Add(followId, Time.time + delay);
        } else if(followId < 0) {
            encountered.Remove(-followId);
        } else {
            encountered.Clear();
        }
    }
    public void MvbAnimator() {
        encountered.Clear();
        buddies.Clear();
        animationQueue.Clear();
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

public class MVBMod_tag : MonoBehaviour {
    public string type = null;
}