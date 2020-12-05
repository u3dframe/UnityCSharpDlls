/// <summary>
/// Base64加密工具
/// Ancher : Canyon / 龚阳辉
/// Create : 2018-12-09 22:30
/// </summary>
namespace Core.Kernel.Cipher
{
	public static class Base64Ex
    {
		// 编码
		static public string Encode(string source,System.Text.Encoding encode){
			if (string.IsNullOrEmpty (source))
				return "";
			string res = "";
			try {
				byte[] buffer = encode.GetBytes(source);
				res = Encode(buffer,source);
			} catch {
				res = source;
			}
			return res;
		}

		// 编码
		static public string Encode(byte[] buffer,string defVal = ""){
			if (buffer == null || buffer.Length == 0)
				return "";
			
			string res = "";
			try {
				res = System.Convert.ToBase64String(buffer);
			} catch {
				res = defVal;
			}
			return res;
		}
		
		// 编码
		static public string Encode(string source){
			return Encode (source,System.Text.UTF8Encoding.UTF8);
		}

		// 解码
		static public string Decode(string source,System.Text.Encoding encode){
			byte[] buffer = Decode2Bytes(source,encode);
			if (buffer == null || buffer.Length == 0)
				return "";
			return Decode(buffer,source);
		}

		// 解码
		static public byte[] Decode2Bytes(string source,System.Text.Encoding encode){
			if (string.IsNullOrEmpty (source))
				return null;
			byte[] res = null;
			try {
				res = System.Convert.FromBase64String(source);
			} catch {
			}
			return res;
		}

		// 解码
		static public string Decode(byte[] buffer,System.Text.Encoding encode,string defVal = ""){
			if (buffer == null || buffer.Length == 0)
				return "";
			string res = defVal;
			try {
				res = encode.GetString(buffer);
			} catch {
				res = defVal;
			}
			return res;
		}

		// 解码
		static public string Decode(byte[] buffer,string defVal = ""){
			return Decode (buffer,System.Text.UTF8Encoding.UTF8,defVal);
		}

		// 解码
		static public string Decode(string source){
			return Decode (source,System.Text.UTF8Encoding.UTF8);
		}

		// 解码
		static public byte[] Decode2Bytes(string source){
			return Decode2Bytes (source,System.Text.UTF8Encoding.UTF8);
		}
    }
}