using UnityEditor;

namespace Core.Art
{
	using ArtCMD = Core.Art.CMDArtAssets;
    /// <summary>
    /// 类名 : 图片格式化
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2021-11-13 10:23
    /// 功能 : 
    /// </summary>
	public static class TextureFmt
	{
		static int compressionRate = 60;
		static bool isUseOutInput = true;
		static TextureFmt()
		{
			ArtCMD.funcCompressionRate = CurrCompressionRate;
			//UnityEngine.Debug.LogError("TextureFmt");
		}
		
		static int CurrCompressionRate(string assetPath,int defCompressionRate)
		{
			int _r = defCompressionRate;
            if(assetPath.Contains("Assets/_Develop/Characters/Builds") && assetPath.Contains("_d."))
                _r = 100;
            return _r;
		}
		
		[MenuItem("Assets/Tools_Art/Fmts/ASTC_4x4", false, 30)]
		static public void Fmt4()
		{
			ArtCMD.ReCompFormatSelFolder(4, compressionRate, isUseOutInput);
		}
		
		[MenuItem("Assets/Tools_Art/Fmts/ASTC_5x5", false, 30)]
		static public void Fmt5()
		{
			ArtCMD.ReCompFormatSelFolder(5, compressionRate, isUseOutInput);
		}
		
		[MenuItem("Assets/Tools_Art/Fmts/ASTC_6x6", false, 30)]
		static public void Fmt6()
		{
			ArtCMD.ReCompFormatSelFolder(6, compressionRate, isUseOutInput);
		}
		
		[MenuItem("Assets/Tools_Art/Fmts/ASTC_8x8", false, 30)]
		static public void Fmt8()
		{
			ArtCMD.ReCompFormatSelFolder(8, compressionRate, isUseOutInput);
		}

		[MenuItem("Assets/Tools_Art/Fmts/ASTC_10x10", false, 30)]
		static public void Fmt10()
		{
			ArtCMD.ReCompFormatSelFolder(10, compressionRate, isUseOutInput);
		}

		[MenuItem("Assets/Tools_Art/Fmts/ASTC_12x12", false, 30)]
		static public void Fmt12()
		{
			ArtCMD.ReCompFormatSelFolder(12, compressionRate, isUseOutInput);
		}
	}
}