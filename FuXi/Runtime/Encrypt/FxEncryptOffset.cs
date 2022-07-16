// ReSharper disable once CheckNamespace
namespace FuXi
{
    /// <summary>
    /// 伏羲内置加密, 文件头偏移加密
    /// </summary>
    public class FxEncryptOffset : BaseEncrypt
    {
        protected override string EncryptHeader => "FxEncryptOffset";
        /// <summary>
        /// 加密验证序列
        /// </summary>
        private readonly byte[] encryptBytes = {0xE6, 0x88, 0x91, 0xE7, 0x88, 0xB1, 0xE4, 0xB8, 0xAD, 0xE5, 0x9B, 0xBD};

        public override EncryptMode EncryptMode => EncryptMode.OFFSET;

        public override byte[] Encrypt(byte[] sourceBytes)
        {
            if (this.IsEncrypted(sourceBytes)) return sourceBytes;
            byte[] header = System.Text.Encoding.UTF8.GetBytes(EncryptHeader);
            byte[] buffer = new byte[sourceBytes.Length + encryptBytes.Length + header.Length];
            
            header.CopyTo(buffer, 0);
            encryptBytes.CopyTo(buffer, header.Length);
            sourceBytes.CopyTo(buffer, header.Length + encryptBytes.Length);
            return buffer;
        }

        /// <summary>
        /// 验证加密序列 与bundle字节序列是否一致
        /// </summary>
        /// <returns></returns>
        public override byte[] EncryptOffset()
        {
            byte[] header = System.Text.Encoding.UTF8.GetBytes(EncryptHeader);
            byte[] buffer = new byte[header.Length + encryptBytes.Length];
            header.CopyTo(buffer, 0);
            encryptBytes.CopyTo(buffer, header.Length);
            return buffer;
        }
    }
}