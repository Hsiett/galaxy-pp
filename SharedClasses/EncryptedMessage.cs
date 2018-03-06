using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SharedClasses
{
    [Serializable]
    public class EncryptedMessage
    {
        private byte[] sessionKey;
        private byte[] data;
        private byte[] IV;

        public byte[] EncryptObject(object o, string RSAKey)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] buf;
            using (MemoryStream memStream = new MemoryStream())
            {
                formatter.Serialize(memStream, o);
                buf = memStream.ToArray();
                memStream.Close();
            }
            return Encrypt(buf, RSAKey);
        }


        public byte[] Encrypt(byte[] data, string RSAKey)
        {
            //Generate random 256 bit AES key
            using (AesCryptoServiceProvider AES = new AesCryptoServiceProvider())
            {
                AES.KeySize = 256;
                AES.GenerateKey();
                AES.GenerateIV();
                
                //Encrypt the data with aes
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (
                        CryptoStream cryptoStream = new CryptoStream(memStream, AES.CreateEncryptor(),
                                                                     CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        this.data = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }

                //Encrypt the AES key with RSA
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(RSAKey);
                    sessionKey = rsa.Encrypt(AES.Key, true);
                }

                IV = AES.IV;
                return AES.Key;
            }
        }

        public void EncryptObject(object o, byte[] AESKey)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] buf;
            using (MemoryStream memStream = new MemoryStream())
            {
                formatter.Serialize(memStream, o);
                buf = memStream.ToArray();
                memStream.Close();
            }
            Encrypt(buf, AESKey);
        }


        public void Encrypt(byte[] data, byte[] AESKey)
        {
            //Generate random 256 bit AES key
            using (AesCryptoServiceProvider AES = new AesCryptoServiceProvider())
            {
                AES.Key = AESKey;
                AES.GenerateIV();

                //Encrypt the data with aes
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (
                        CryptoStream cryptoStream = new CryptoStream(memStream, AES.CreateEncryptor(),
                                                                     CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        this.data = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }

                IV = AES.IV;
            }
        }

        public byte[] DecryptAESKey(string RSAKey)
        {
            byte[] AESKey;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(RSAKey);
                AESKey = rsa.Decrypt(sessionKey, true);
            }
            return AESKey;
        }

        public object DecryptObject(string RSAKey)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] buf = Decrypt(RSAKey);
            object ret;
            using (MemoryStream memStream = new MemoryStream(buf))
            {
                ret = formatter.Deserialize(memStream);
                memStream.Close();
            }
            return ret;
        }

        public byte[] Decrypt(string RSAKey)
        {
            //Decrypt the AES key with RSA
            byte[] AESKey = DecryptAESKey(RSAKey);

            byte[] returner;
            using (AesCryptoServiceProvider AES = new AesCryptoServiceProvider())
            {
                AES.Key = AESKey;
                AES.IV = IV;
                //Decrypt the data with aes
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (
                        CryptoStream cryptoStream = new CryptoStream(memStream, AES.CreateDecryptor(),
                                                                     CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        returner = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            return returner;
        }

        public object DecryptObject(byte[] AESKey)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] buf = Decrypt(AESKey);
            object ret;
            using (MemoryStream memStream = new MemoryStream(buf))
            {
                ret = formatter.Deserialize(memStream);
                memStream.Close();
            }
            return ret;
        }

        public byte[] Decrypt(byte[] AESKey)
        {
            byte[] returner;
            using (AesCryptoServiceProvider AES = new AesCryptoServiceProvider())
            {
                AES.Key = AESKey;
                AES.IV = IV;
                //Decrypt the data with aes
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (
                        CryptoStream cryptoStream = new CryptoStream(memStream, AES.CreateDecryptor(),
                                                                     CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        returner = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            return returner;
        }
   }
}