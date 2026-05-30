namespace WPG.Core
{
    // Ścieżki do paczek Asset Store (import usera). GameAssetLoader próbuje je po kolei.
    // Skan Assets/ (2026-05-28): brak paczek GUI/audio — tylko Fantasy Forest Environment Free Sample.
    public static class GameAssetPaths
    {
        // --- Fantasy Free GUI ---
        public static readonly string[] GuiPanel =
        {
            "Assets/Fantasy Free GUI/Sprites/panel.png",
            "Assets/Fantasy Free GUI/Sprites/Panel.png",
            "Assets/Fantasy Free GUI/panel.png",
            "Assets/Fantasy Free GUI/Panel/panel.png",
        };

        public static readonly string[] GuiButton =
        {
            "Assets/Fantasy Free GUI/Sprites/button.png",
            "Assets/Fantasy Free GUI/Sprites/Button.png",
            "Assets/Fantasy Free GUI/button.png",
        };

        public static readonly string[] GuiBarBackground =
        {
            "Assets/Fantasy Free GUI/Sprites/bar_background.png",
            "Assets/Fantasy Free GUI/Sprites/Bar_Background.png",
            "Assets/Fantasy Free GUI/Sprites/bar_bg.png",
            "Assets/Fantasy Free GUI/bar_background.png",
        };

        public static readonly string[] GuiBarFillHp =
        {
            "Assets/Fantasy Free GUI/Sprites/bar_red.png",
            "Assets/Fantasy Free GUI/Sprites/Bar_Red.png",
            "Assets/Fantasy Free GUI/Sprites/hp_bar.png",
            "Assets/Fantasy Free GUI/bar_red.png",
        };

        public static readonly string[] GuiBarFillMana =
        {
            "Assets/Fantasy Free GUI/Sprites/bar_blue.png",
            "Assets/Fantasy Free GUI/Sprites/Bar_Blue.png",
            "Assets/Fantasy Free GUI/Sprites/mana_bar.png",
            "Assets/Fantasy Free GUI/bar_blue.png",
        };

        public static readonly string[] GuiIconFrame =
        {
            "Assets/Fantasy Free GUI/Sprites/icon_frame.png",
            "Assets/Fantasy Free GUI/Sprites/Icon_Frame.png",
            "Assets/Fantasy Free GUI/Sprites/slot.png",
            "Assets/Fantasy Free GUI/icon_frame.png",
        };

        public static readonly string[] GuiMenuBackground =
        {
            "Assets/Fantasy Free GUI/Sprites/menu_background.png",
            "Assets/Fantasy Free GUI/Sprites/Menu_Background.png",
            "Assets/Fantasy Free GUI/Sprites/background.png",
            "Assets/Fantasy Free GUI/menu_background.png",
        };

        // --- Modern RPG icons ---
        public static readonly string[] IconMelee =
        {
            "Assets/Modern RPG icons/Icons/sword.png",
            "Assets/Modern RPG icons/Icons/icon_sword.png",
            "Assets/Modern RPG Icons/Icons/sword.png",
            "Assets/Modern RPG icons/sword.png",
        };

        public static readonly string[] IconFireball =
        {
            "Assets/Modern RPG icons/Icons/fireball.png",
            "Assets/Modern RPG icons/Icons/fire.png",
            "Assets/Modern RPG icons/Icons/icon_fire.png",
            "Assets/Modern RPG Icons/Icons/fireball.png",
        };

        public static readonly string[] IconHeal =
        {
            "Assets/Modern RPG icons/Icons/heal.png",
            "Assets/Modern RPG icons/Icons/potion.png",
            "Assets/Modern RPG icons/Icons/icon_heal.png",
            "Assets/Modern RPG Icons/Icons/heal.png",
        };

        public static readonly string[] IconHerb =
        {
            "Assets/Modern RPG icons/Icons/herb.png",
            "Assets/Modern RPG icons/Icons/leaf.png",
            "Assets/Modern RPG icons/Icons/plant.png",
            "Assets/Modern RPG icons/Icons/mushroom.png",
            "Assets/Modern RPG Icons/Icons/herb.png",
        };

        public static readonly string[] IconAmulet =
        {
            "Assets/Modern RPG icons/Icons/amulet.png",
            "Assets/Modern RPG icons/Icons/ring.png",
            "Assets/Modern RPG icons/Icons/necklace.png",
            "Assets/Modern RPG icons/Icons/relic.png",
            "Assets/Modern RPG Icons/Icons/amulet.png",
        };

        // --- Basic RPG Sounds ---
        public static readonly string[] SfxHit =
        {
            "Assets/Basic RPG Sounds/hit.wav",
            "Assets/Basic RPG Sounds/sword_hit.wav",
            "Assets/Basic RPG Sounds/SFX/hit.wav",
            "Assets/Basic RPG Sounds/weapon_hit.wav",
        };

        public static readonly string[] SfxDeath =
        {
            "Assets/Basic RPG Sounds/death.wav",
            "Assets/Basic RPG Sounds/enemy_death.wav",
            "Assets/Basic RPG Sounds/SFX/death.wav",
        };

        public static readonly string[] SfxUIClick =
        {
            "Assets/Basic RPG Sounds/ui_click.wav",
            "Assets/Basic RPG Sounds/click.wav",
            "Assets/Basic RPG Sounds/SFX/ui_click.wav",
            "Assets/Basic RPG Sounds/button_click.wav",
        };

        public static readonly string[] SfxFireballCast =
        {
            "Assets/Basic RPG Sounds/fireball.wav",
            "Assets/Basic RPG Sounds/spell_fire.wav",
            "Assets/Basic RPG Sounds/fire_cast.wav",
            "Assets/Basic RPG Sounds/SFX/fireball.wav",
        };

        // Aliasy używane przez GameAudioManager i GameAssetRegistry (SfxCast).
        public static readonly string[] SfxFireball = SfxFireballCast;
        public static readonly string[] SfxCast = SfxFireballCast;

        public static readonly string[] SfxFootstep =
        {
            "Assets/_Game/Audio/SFX/footstep_grass_1.wav",
            "Assets/Basic RPG Sounds/footstep.wav",
            "Assets/Basic RPG Sounds/footstep_grass.wav",
            "Assets/Basic RPG Sounds/SFX/footstep.wav",
            "Assets/Basic RPG Sounds/walk_grass.wav",
        };

        public static readonly string[] SfxFootstep2 =
        {
            "Assets/_Game/Audio/SFX/footstep_grass_2.wav",
        };

        // Skok gracza — krótki wydech/chrząknięcie dojrzałego mężczyzny ("hszyy", Mixkit "Fighting man voice").
        public static readonly string[] SfxJump =
        {
            "Assets/_Game/Audio/SFX/jump_grunt.wav",
        };

        // Długi spadek (dłużej niż zwykły skok) — szelest powietrza/płaszcza (Mixkit "Air woosh", dawne jump.wav).
        public static readonly string[] SfxFallWoosh =
        {
            "Assets/_Game/Audio/SFX/fall_woosh.wav",
        };

        // Podskok/odbicie goblina (CC-BY 3.0, Yo Frankie! / Blender Foundation).
        public static readonly string[] SfxGoblinHop =
        {
            "Assets/_Game/Audio/SFX/goblin_hop.ogg",
        };

        // Chrząknięcie/wydech gracza przy ciosie pięścią (dojrzały mężczyzna ~40 lat).
        public static readonly string[] SfxPunch =
        {
            "Assets/_Game/Audio/SFX/punch_grunt.wav",
        };

        // --- Muzyka (warstwy) — import usera do Assets/_Game/Audio/Music/ ---
        public static readonly string[] MusicSoundtrack =
        {
            "Assets/_Game/Audio/Music/SOUNDTRACK.wav",
        };

        public static readonly string[] MusicMenu =
        {
            "Assets/_Game/Audio/Music/MENU.wav",
        };

        public static readonly string[] MusicAmbient =
        {
            "Assets/_Game/Audio/Music/AMBIENT.wav",
        };

        public const string ResMusicSoundtrack = "Audio/Music/SOUNDTRACK";
        public const string ResMusicMenu = "Audio/Music/MENU";
        public const string ResMusicAmbient = "Audio/Music/AMBIENT";

        // Resources fallback (user może skopiować klipy/sprites do Resources/)
        public const string ResUiPanel = "UI/panel";
        public const string ResUiButton = "UI/button";
        public const string ResUiBarBg = "UI/bar_background";
        public const string ResUiBarHp = "UI/bar_red";
        public const string ResUiBarMana = "UI/bar_blue";
        public const string ResUiIconFrame = "UI/icon_frame";
        public const string ResUiMenuBg = "UI/menu_background";
        public const string ResIconMelee = "UI/icon_sword";
        public const string ResIconFire = "UI/icon_fire";
        public const string ResIconHeal = "UI/icon_heal";
        public const string ResIconHerb = "UI/icon_herb";
        public const string ResIconAmulet = "UI/icon_amulet";
        public const string ResSfxHit = "Audio/hit";
        public const string ResSfxDeath = "Audio/death";
        public const string ResSfxClick = "Audio/ui_click";
        public const string ResSfxFireball = "Audio/fireball";
        public const string ResSfxCast = "Audio/fireball";
        public const string ResSfxFootstep = "Audio/footstep";
        public const string ResSfxFootstep2 = "Audio/SFX/footstep_grass_2";
        public const string ResSfxJump = "Audio/SFX/jump_grunt";
        public const string ResSfxFallWoosh = "Audio/SFX/fall_woosh";
        public const string ResSfxGoblinHop = "Audio/SFX/goblin_hop";
        public const string ResSfxPunch = "Audio/SFX/punch_grunt";

        // --- Świat (Agent B) — import usera; skan fallback w GameAssetRegistry ---
        public static readonly string[] WorldTrees =
        {
            "Assets/Nature Starter Kit 2/Prefabs/Tree.prefab",
            "Assets/Nature Starter Kit 2/Prefabs/Pine.prefab",
            "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab",
            "Assets/_Game/Prefabs/World/TreeLarge.prefab",
        };

        public static readonly string[] WorldBushes =
        {
            "Assets/Nature Starter Kit 2/Prefabs/Bush.prefab",
            "Assets/Nature Starter Kit 2/Models/Bush.prefab",
            "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab",
            "Assets/_Game/Prefabs/World/Bush.prefab",
        };

        public static readonly string[] WorldGrass =
        {
            "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab",
            "Assets/Nature Starter Kit 2/Prefabs/Grass.prefab",
            "Assets/Nature Starter Kit 2/Models/Grass.prefab",
            "Assets/_Game/Prefabs/World/Grass.prefab",
        };

        public static readonly string[] WorldRocks =
        {
            "Assets/Rock_pack/Prefabs/Rock.prefab",
            "Assets/Rock_pack/Prefabs/Rock_01.prefab",
            "Assets/Rocks HD/Prefabs/Rock.prefab",
            "Assets/Rocks HD/Prefabs/Rock_01.prefab",
            "Assets/_Game/Prefabs/World/Rock.prefab",
        };

        public static readonly string[] WorldRuins =
        {
            "Assets/RPG Dungeon Pack Sample/Prefabs/Wall.prefab",
            "Assets/RPG Dungeon Pack Sample/Prefabs/Column.prefab",
            "Assets/RPG Dungeon Kit/Prefabs/Wall.prefab",
            "Assets/RPG Dungeon Kit/Prefabs/Pillar.prefab",
            "Assets/RPG Dungeon Kit/Prefabs/Column.prefab",
            "Assets/_Game/Prefabs/World/Ruin.prefab",
        };

        // --- Teren / niebo (Fantasy Forest Environment Free Sample) ---
        public static readonly string[] TexturePath =
        {
            "Assets/Fantasy Forest Environment Free Sample/Textures/dirt01.tga",
            "Assets/Fantasy Forest Environment Free Sample/Materials/dirt01.mat",
        };

        public static readonly string[] TextureGrass =
        {
            "Assets/Fantasy Forest Environment Free Sample/Textures/grass01.tga",
            "Assets/Fantasy Forest Environment Free Sample/Materials/grass01.mat",
        };

        public static readonly string[] SkyboxMaterial =
        {
            "Assets/Fantasy Forest Environment Free Sample/Materials/skyMaterial.mat",
            "Assets/Invector-3rdPersonController_LITE/3D Models/Others/basicSky.mat",
        };

        public const string ResTexturePath = "World/dirt01";
        public const string ResTextureGrass = "World/grass01";

        // --- Postacie (Agent C) — GanzSe, Starter Assets URP, gobliny ---
        public const string GanzSeDruidPrefab =
            "Assets/URP GanzSe Free Modular Character Pack/Prefabs/Modular Character/GanzSe Free Modular Character Update 1_1.prefab";

        public static readonly string[] DruidModel =
        {
            GanzSeDruidPrefab,
            "Assets/_Game/Prefabs/Characters/DruidModel.prefab",
            "Assets/URP GanzSe Free Modular Character Pack/Prefabs/Modular Character/GanzSe Free Modular Character Original 1_0.prefab",
            "Assets/GanzSe FREE Modular Character - Fantasy Low Poly Pack/Prefabs/Character.prefab",
            "Assets/GanzSe FREE Modular Character - Fantasy Low Poly Pack/Prefabs/Male.prefab",
            "Assets/GanzSe Character/Prefabs/GanzSeMale.prefab",
            "Assets/GanzSe Character/Prefabs/GanzSe.prefab",
            "Assets/GanzSe Character/Prefabs/Character.prefab",
            "Assets/GanzSe Modular Character/Prefabs/Character.prefab",
            "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/PlayerArmature.prefab",
            "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/NestedParentArmature_Unpack.prefab",
            "Assets/Starter Assets - Third Person Character Controller/Third Person Controller/Prefabs/PlayerArmature.prefab",
            "Assets/Starter Assets - Third Person | URP/Third Person Controller/Prefabs/PlayerArmature.prefab",
        };

        public static readonly string[] DruidModelFbx =
        {
            "Assets/GanzSe Character/Models/GanzSe.fbx",
            "Assets/GanzSe FREE Modular Character - Fantasy Low Poly Pack/Models/Character.fbx",
            "Assets/GanzSe Character/GanzSeMale.fbx",
        };

        // Fantasy Goblin pack (import folder: Assets/Goblin/)
        public const string FantasyGoblinFolder = "Assets/Goblin";
        public const string FantasyGoblinMeleePrefab = "Assets/Goblin/Prefab/skin1.prefab";
        public const string FantasyGoblinMeleeWeaponPrefab = "Assets/Goblin/Prefab/goblin_stonesword.prefab";
        public const string FantasyGoblinArcherPrefab = "Assets/Goblin/Prefab/skin2.prefab";
        public const string FantasyGoblinElitePrefab = "Assets/Goblin/Prefab/skin3.prefab";

        public const string GoblinBodyMaterialSkin1 = "Assets/Goblin/Materials/skin1/gobs_body.mat";
        public const string GoblinPartsMaterialSkin1 = "Assets/Goblin/Materials/skin1/gobs_parts.mat";
        public const string GoblinBodyMaterialSkin2 = "Assets/Goblin/Materials/skin2/gobs_body.mat";
        public const string GoblinPartsMaterialSkin2 = "Assets/Goblin/Materials/skin2/gobs_parts.mat";

        public const string WPGFireGlowMaterial = "Assets/_Game/Materials/Fixups/WPG_FireGlow.mat";
        public const string WPGMushroomGlowMaterial = "Assets/_Game/Materials/Fixups/WPG_MushroomGlow.mat";
        public const string WPGMushroomStemMaterial = "Assets/_Game/Materials/Fixups/WPG_MushroomStem.mat";
        public const string WPGSmokeMaterial = "Assets/_Game/Materials/Fixups/WPG_Smoke.mat";

        public static readonly string[] GoblinModel =
        {
            FantasyGoblinMeleePrefab,
            FantasyGoblinMeleeWeaponPrefab,
            "Assets/_Game/Prefabs/Enemies/GoblinModel.prefab",
            "Assets/Fantasy Goblin/Prefabs/Goblin.prefab",
            "Assets/3D Stylized Goblin/Prefabs/Goblin_Warrior.prefab",
            "Assets/3D Stylized Goblin/Prefabs/Goblin.prefab",
            "Assets/Stylized Goblins Archer & Warrior/Prefabs/Goblin_Warrior.prefab",
        };

        public static readonly string[] GoblinArcherModel =
        {
            FantasyGoblinArcherPrefab,
            FantasyGoblinMeleePrefab,
            "Assets/Fantasy Goblin/Prefabs/Goblin_Archer.prefab",
            "Assets/3D Stylized Goblin/Prefabs/Goblin_Archer.prefab",
            "Assets/Stylized Goblins Archer & Warrior/Prefabs/Goblin_Archer.prefab",
        };

        public static readonly string[] GoblinElite =
        {
            FantasyGoblinElitePrefab,
            FantasyGoblinArcherPrefab,
            "Assets/_Game/Prefabs/Enemies/GoblinElite.prefab",
            "Assets/Fantasy Goblin/Prefabs/Fantasy Goblin.prefab",
            "Assets/Fantasy Goblin/Prefabs/Goblin_Shaman.prefab",
            "Assets/Fantasy Goblin/Prefabs/Goblin Elite.prefab",
        };

        // --- Invector Third Person Controller LITE (visual + locomotion animator) ---
        public const string InvectorFolder = "Assets/Invector-3rdPersonController_LITE";

        /// <summary>Tokeny do wykrywania folderów Invector po imporcie (nazwy mogą się różnić).</summary>
        public static readonly string[] InvectorFolderTokens =
        {
            "invector-3rdpersoncontroller",
            "invector_3rdpersoncontroller",
            "invector/basic locomotion",
            "third person controller - basic locomotion",
            "/invector/",
        };

        public const string InvectorCharacterPrefab =
            "Assets/Invector-3rdPersonController_LITE/3D Models/Characters/Invector@V-Bot 2.0/Prefab/VBOT2.0_Custom.prefab";

        public const string InvectorThirdPersonPrefab =
    "Assets/_Game/Prefabs/Player/BlinkPlayerController.prefab";

        public const string InvectorLocomotionController =
            "Assets/Invector-3rdPersonController_LITE/Animator/Invector@BasicLocomotion.controller";

        // Runtime/build fallback (Resources.Load) dla animacji goblinów.
        // Skopiuj controller do Assets/_Game/Resources/Anim/InvectorBasicLocomotion.controller
        // oraz (opcjonalnie) avatar do Assets/_Game/Resources/Anim/GoblinHumanoidAvatar.asset.
        public const string ResLocomotionController = "Anim/InvectorBasicLocomotion";
        public const string ResGoblinAvatar = "Anim/GoblinHumanoidAvatar";

        public static readonly string[] InvectorCharacter =
        {
            InvectorCharacterPrefab,
            InvectorThirdPersonPrefab,
        };
    }
}
