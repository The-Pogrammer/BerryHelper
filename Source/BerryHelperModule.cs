using System;

namespace Celeste.Mod.BerryHelper;

public class BerryHelperModule : EverestModule {
    public static BerryHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(BerryHelperModuleSettings);
    public static BerryHelperModuleSettings Settings => (BerryHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(BerryHelperModuleSession);
    public static BerryHelperModuleSession Session => (BerryHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(BerryHelperModuleSaveData);
    public static BerryHelperModuleSaveData SaveData => (BerryHelperModuleSaveData) Instance._SaveData;

    public static string LoggerTag = nameof(BerryHelperModule);
    public BerryHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(BerryHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(BerryHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        // TODO: apply any hooks that should always be active
    }

    public override void Unload() {
        // TODO: unapply any hooks applied in Load()
    }
}