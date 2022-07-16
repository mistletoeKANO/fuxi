using System;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    /// <summary>
    /// 伏羲内置加密, 全字节异或加密
    /// </summary>
    public class FxEncryptXor : BaseEncrypt
    {
        protected override string EncryptHeader => "FxEncryptXOR";

        /// <summary>
        /// 加密序列
        /// </summary>
        private readonly byte[] EncryptBytes = {0xE6, 0x88, 0x91, 0xE7, 0x88, 0xB1, 0xE4, 0xB8, 0xAD, 0xE5, 0x9B, 0xBD};
        
        public override EncryptMode EncryptMode => EncryptMode.XOR;

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <returns></returns>
        public override byte[] Encrypt(byte[] sourceBytes)
        {
            if (this.IsEncrypted(sourceBytes)) return sourceBytes;
            
            byte[] header = System.Text.Encoding.UTF8.GetBytes(EncryptHeader);
            byte[] buffer = new byte[header.Length + sourceBytes.Length];
            header.CopyTo(buffer, 0);
            
            sourceBytes = this.EncryptInternal(sourceBytes);
            sourceBytes.CopyTo(buffer, header.Length);
            
            return buffer;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <returns></returns>
        public override byte[] DeEncrypt(byte[] sourceBytes)
        {
            if (!this.IsEncrypted(sourceBytes)) return sourceBytes;
            
            byte[] header = System.Text.Encoding.UTF8.GetBytes(EncryptHeader);
            byte[] buffer = new byte[sourceBytes.Length - header.Length];
            Array.Copy(sourceBytes, header.Length, buffer, 0, buffer.Length);
            return this.EncryptInternal(buffer);
        }

        private byte[] EncryptInternal(byte[] sourceBytes)
        {
            int encLength = EncryptBytes.Length;
            int souLength = sourceBytes.Length;
            for (int i = 0; i < souLength; i++)
            {
                byte b = sourceBytes[i];
                byte encXor = EncryptBytes[i % encLength];
                sourceBytes[i] = Convert.ToByte(b ^ encXor);
            }
            return sourceBytes;
        }
    }
}