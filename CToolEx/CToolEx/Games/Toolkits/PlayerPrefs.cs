using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Core
{
    using UPPrefs = UnityEngine.PlayerPrefs;
    using LJData = LitJson.JsonData;

    /// <summary>
	/// 类名 : 本地数据储存
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2016-05-26 13:29
	/// 功能 : 优化 UnityEngine.PlayerPrefs 减少 IO 访问
    /// 参考 : http://www.previewlabs.com
	/// </summary>
    public static class PlayerPrefs
    {
        private static readonly Hashtable tbPlayer = new Hashtable();
        private static readonly LJData tbJson = new LJData();
        private static string fileDir = Application.persistentDataPath + "/";
        private static bool hashTableChanged = false;
        private static string serializedOutput = "";
        private static string serializedInput = "";

        private static bool isSaveToFile = false;
        private static string keyPrefix = "";
        private static string keyEncrypted = "encryptedData";
        private static string keyData = "data";

        //NOTE modify the iw3q part to an arbitrary string of length 4 for your project, as this is the encryption key
        private static byte[] bytes = ASCIIEncoding.ASCII.GetBytes("pljz" + SystemInfo.deviceUniqueIdentifier.Substring(0, 4));
        private static bool securityModeEnabled = false;
        private static bool canFlush = true;
        private static bool _isLoaded = false;

        static public void Init(string dirFile,string prefix,bool isSaveEncode = true,bool isSaveFile = false,bool isReLoad = true)
        {
            if (!string.IsNullOrEmpty(dirFile))
                fileDir = dirFile;
            keyPrefix = prefix;
            securityModeEnabled = isSaveEncode;
            isSaveToFile = isSaveFile;
            if(isReLoad)
                _isLoaded = false;

            LoadFromDisk();
        }

        public static void LoadFromDisk()
        {
            if (_isLoaded)
                return;
            _isLoaded = true;

            tbPlayer.Clear();
            tbJson.Clear();
            canFlush = true;
            hashTableChanged = false;
            serializedOutput = "";
            serializedInput = "";

            bool _isSaveFile = isSaveToFile;
#if UNITY_WEBPLAYER
            _isSaveFile = false;
#endif
            string _key1 = keyPrefix + keyEncrypted;
            string _key2 = keyPrefix + keyData;
            bool _isEncode = false;
            if (_isSaveFile)
            {
                StreamReader fileReader = null;
                string _pf1 = fileDir + _key1;
                string _pf2 = fileDir + _key2;
                if (File.Exists(_pf1))
                {
                    fileReader = new StreamReader(_pf1);
                    _isEncode = true;
                }
                else if(File.Exists(_pf2))
                {
                    fileReader = new StreamReader(_pf2);
                }
                if (fileReader != null)
                {
                    serializedInput = fileReader.ReadToEnd();
                    fileReader.Close();
                }
            }
            else
            {
                if (UPPrefs.HasKey(_key1))
                {
                    _isEncode = bool.Parse(UPPrefs.GetString(_key1));
                    serializedInput = UPPrefs.GetString(_key2);
                }
            }

            if (_isEncode && !string.IsNullOrEmpty(serializedInput))
                serializedInput = Decrypt(serializedInput);

            Deserialize();
        }

        public static void ReloadFromDisk()
        {
            _isLoaded = false;
            LoadFromDisk();
        }

        public static void StopFlush()
        {
            canFlush = false;
        }

        public static bool HasKey(string key)
        {
            return tbPlayer.ContainsKey(key);
        }

        public static void SetString(string key, string value, bool shouldFlush = false)
        {
            if (!tbPlayer.ContainsKey(key))
            {
                tbPlayer.Add(key, value);
            }
            else
            {
                tbPlayer[key] = value;
            }
            hashTableChanged = true;
            if (shouldFlush)
            {
                Flush();
            }
        }

        public static void SetInt(string key, int value, bool shouldFlush = false)
        {
            string valueString = System.Convert.ToBase64String(BitConverter.GetBytes(value));
            if (!tbPlayer.ContainsKey(key))
            {
                tbPlayer.Add(key, valueString);
            }
            else
            {
                tbPlayer[key] = valueString;
            }
            hashTableChanged = true;
            if (shouldFlush)
            {
                Flush();
            }
        }

        public static void SetFloat(string key, float value, bool shouldFlush = false)
        {
            string valueString = System.Convert.ToBase64String(BitConverter.GetBytes(value));
            if (!tbPlayer.ContainsKey(key))
            {
                tbPlayer.Add(key, valueString);
            }
            else
            {
                tbPlayer[key] = valueString;
            }
            hashTableChanged = true;
            if (shouldFlush)
            {
                Flush();
            }
        }

        public static void SetBool(string key, bool value, bool shouldFlush = false)
        {
            if (!tbPlayer.ContainsKey(key))
            {
                tbPlayer.Add(key, value);
            }
            else
            {
                tbPlayer[key] = value;
            }
            hashTableChanged = true;
            if (shouldFlush)
            {
                Flush();
            }
        }

        public static void SetLong(string key, long value, bool shouldFlush = false)
        {
            if (!tbPlayer.ContainsKey(key))
            {
                tbPlayer.Add(key, value);
            }
            else
            {
                tbPlayer[key] = value;
            }
            hashTableChanged = true;
            if (shouldFlush)
            {
                Flush();
            }
        }
        //------------------------------------------------------------------------------------------------------
        public static string GetString(string key, string defaultValue = "")
        {
            if (tbPlayer.ContainsKey(key))
            {
                return tbPlayer[key].ToString();
            }

            tbPlayer.Add(key, defaultValue);
            hashTableChanged = true;
            return defaultValue;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            if (tbPlayer.ContainsKey(key))
            {
                try
                {
                    string _v = tbPlayer[key].ToString();
                    byte[] _bts = Convert.FromBase64String(_v);
                    return BitConverter.ToInt32(_bts,0);
                    // return BitConverter.ToInt32(Convert.FromBase64String(tbPlayer[key].ToString()), 0);
                }
                catch
                {
                }                
            }
            SetInt(key, defaultValue);
            hashTableChanged = true;
            return defaultValue;
        }

        public static long GetLong(string key, long defaultValue = 0)
        {
            if (tbPlayer.ContainsKey(key))
            {
                return (long)tbPlayer[key];
            }
            tbPlayer.Add(key, defaultValue);
            hashTableChanged = true;
            return defaultValue;
        }

        public static float GetFloat(string key, float defaultValue = 0.0f)
        {
            if (tbPlayer.ContainsKey(key))
            {
                try
                {
                    string _v = tbPlayer[key].ToString();
                    byte[] _bts = Convert.FromBase64String(_v);
                    return (float)(BitConverter.ToSingle(_bts, 0));
                    // return (float)(BitConverter.ToSingle(Convert.FromBase64String(tbPlayer[key].ToString()), 0));
                }
                catch
                {
                }
            }
            SetFloat(key, defaultValue);
            hashTableChanged = true;
            return defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (tbPlayer.ContainsKey(key))
            {
                return (bool)tbPlayer[key];
            }
            tbPlayer.Add(key, defaultValue);
            hashTableChanged = true;
            return defaultValue;
        }

        public static void DeleteKey(string key)
        {
            tbPlayer.Remove(key);
            canFlush = true;
            hashTableChanged = true;
            Flush();
        }

        public static void DeleteAll()
        {
            tbPlayer.Clear();
            canFlush = true;
            hashTableChanged = true;
            Flush();
        }

        public static void EnableEncryption(bool enabled)
        {
            securityModeEnabled = enabled;
        }

        public static void Flush()
        {
            bool isCan = (hashTableChanged && canFlush);
            if (!isCan)
                return;

            bool _isEncode = securityModeEnabled;
            hashTableChanged = false;
            Serialize();
            string output = (_isEncode ? Encrypt(serializedOutput) : serializedOutput);
            serializedOutput = "";

            bool _isSaveFile = isSaveToFile;
#if UNITY_WEBPLAYER
            _isSaveFile = false;
#endif
            string _key1 = keyPrefix + keyEncrypted;
            string _key2 = keyPrefix + keyData;

            if (_isSaveFile)
            {
                string _pf1 = fileDir + _key1;
                string _pf2 = fileDir + _key2;
                if (File.Exists(_pf1))
                    File.Delete(_pf1);
                if (File.Exists(_pf2))
                    File.Delete(_pf2);
                string _pf = _isEncode ? _pf1 : _pf2;

                StreamWriter fileWriter = null;
                fileWriter = File.CreateText(_pf);
                fileWriter.Write(output);
                fileWriter.Flush();
                fileWriter.Close();
            }
            else
            {
                UPPrefs.SetString(_key2, output);
                UPPrefs.SetString(_key1, _isEncode.ToString());
                UPPrefs.Save();
            }
        }

        private static void Serialize()
        {
            serializedOutput = "";
            if (tbPlayer.Count <= 0)
                return;

            tbJson.Clear();
            tbJson.SetJsonType(LitJson.JsonType.Object);
            foreach (DictionaryEntry de in tbPlayer)
            {
                if (de.Value == null)
                    continue;
                tbJson[de.Key.ToString()] = new LJData(de.Value);
            }
            serializedOutput = tbJson.ToJson();
            tbJson.Clear();
        }

        private static void Deserialize()
        {
            if (string.IsNullOrEmpty(serializedInput))
                return;

            LJData _json = null;
            try
            {
                _json = LitJson.JsonMapper.ToObject(serializedInput);
                serializedInput = "";
            }
            catch
            {
            }

            if (_json == null)
            {
                return;
            }
            IDictionary dic = _json;
            object _val = null;
            string _key = null;
            LJData _jd = null;
            foreach (var key in dic.Keys)
            {
                _val = dic[key];
                _key = key.ToString();
                if (_val is Boolean)
                {
                    tbPlayer.Add(_key, (bool)_val);
                }
                else if (_val is Double)
                {
                    tbPlayer.Add(_key, (double)_val);
                }
                else if (_val is Int32)
                {
                    tbPlayer.Add(_key, (int)_val);
                }
                else if (_val is Int64)
                {
                    tbPlayer.Add(_key, (long)_val);
                }
                else if (_val is LJData)
                {
                    _jd = (LJData)_val;
                    if (_jd.IsObject || _jd.IsArray)
                    {
                        tbPlayer.Add(_key, _jd.ToJson());
                    }
                    else if (_jd.IsBoolean)
                    {
                        tbPlayer.Add(_key, (bool)_jd);
                    }
                    else if (_jd.IsDouble)
                    {
                        tbPlayer.Add(_key, (double)_jd);
                    }
                    else if (_jd.IsInt)
                    {
                        tbPlayer.Add(_key, (int)_jd);
                    }
                    else if (_jd.IsLong)
                    {
                        tbPlayer.Add(_key, (long)_jd);
                    }
                    else
                    {
                        tbPlayer.Add(_key, _jd.ToString());
                    }
                }
                else
                {
                    tbPlayer.Add(_key, _val.ToString());
                }
            }
        }

        private static string Encrypt(string originalString)
        {
            if (String.IsNullOrEmpty(originalString))
                return "";

            using(MemoryStream memoryStream = new MemoryStream())
            {
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
                StreamWriter writer = new StreamWriter(cryptoStream);
                writer.Write(originalString);
                writer.Flush();
                cryptoStream.FlushFinalBlock();
                writer.Flush();
                string _ret = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                writer.Close();
                cryptoStream.Close();
                return _ret;
            }
        }

        private static string Decrypt(string cryptedString)
        {
            if (String.IsNullOrEmpty(cryptedString))
                return "";
            
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString)))
            {
                CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
                StreamReader reader = new StreamReader(cryptoStream);
                string _ret = reader.ReadToEnd();
                reader.Close();
                cryptoStream.Close();
                return _ret;
            }
        }

        public static string GetSerializedOutput()
        {
            Serialize();
            return serializedOutput;
        }

        public static void SetSerializedInput(string inputString)
        {
            tbPlayer.Clear();
            canFlush = true;
            hashTableChanged = true;
            serializedInput = inputString;
            Deserialize();
            Flush();
        }

        public static void Save()
        {
            Flush();
        }
    }
}