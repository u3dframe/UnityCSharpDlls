public class AutoImportSetting : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        AudioImporter importer = (AudioImporter)assetImporter;
        if (importer.assetPath.Contains("Music"))
        {
            AudioImporterSampleSettings setting = new AudioImporterSampleSettings();
            setting.loadType = AudioClipLoadType.Streaming;
            setting.compressionFormat = AudioCompressionFormat.Vorbis;
            setting.quality = 100;
            importer.defaultSampleSettings = setting;
            importer.preloadAudioData = false;
            importer.loadInBackground = true;
            importer.forceToMono = false;
        }
        else if (importer.assetPath.Contains("Sfx") ||
                 importer.assetPath.Contains("Se") ||
                 importer.assetPath.Contains("Vox") ||
                 importer.assetPath.Contains("UI"))
        {
            AudioImporterSampleSettings setting = new AudioImporterSampleSettings();
            setting.loadType = AudioClipLoadType.DecompressOnLoad;
            setting.compressionFormat = AudioCompressionFormat.ADPCM;
            importer.defaultSampleSettings = setting;
            importer.preloadAudioData = false;
            importer.loadInBackground = false;
            importer.forceToMono = false;
        }

        importer.ClearSampleSettingOverride("Android");
        importer.ClearSampleSettingOverride("iOS");
    }

    void OnPreprocessTexture()
    {
        //自动设置类型;
        TextureImporter textureImporter = (TextureImporter)assetImporter;

        var path = AssetDatabase.GetAssetPath(textureImporter);
        if (!path.Contains(@"Assets/Resources"))//主要是针对是否导入项目某个文件夹下进行判断
        {
            return;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;

        SetTextureFormat(textureImporter);
        //TextureImporterPlatformSettings importPlatformAndroid = textureImporter.GetPlatformTextureSettings("Android");
        //importPlatformAndroid.overridden = true;
        //是否存在透明通道 例如虽是png格式  但是不存在透明通道可根据需求对图形单独做处理
        //if (textureImporter.DoesSourceTextureHaveAlpha())
        //{
        //    importPlatformAndroid.format = TextureImporterFormat.ASTC_RGBA_4x4;
        //}
        //else
        //{
        //    importPlatformAndroid.format = TextureImporterFormat.ASTC_RGBA_4x4;
        //}
        //textureImporter.SetPlatformTextureSettings(importPlatformAndroid);
        //TextureImporterPlatformSettings importPlatformIos = textureImporter.GetPlatformTextureSettings("iPhone");
        //importPlatformIos.overridden = true;
        //importPlatformIos.format = TextureImporterFormat.ASTC_RGBA_4x4;
        //textureImporter.SetPlatformTextureSettings(importPlatformIos);

    }
}