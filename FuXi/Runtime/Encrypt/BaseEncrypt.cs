// ReSharper disable once CheckNamespace
namespace FuXi
{
    public abstract class BaseEncrypt
    {
        internal static readonly System.Collections.Generic.List<object> InternalReference =
            new System.Collections.Generic.List<object>()
            {
                new FuXi.FxEncryptXor(),
                new FuXi.FxEncryptOffset(),
            };
        
        /// <summary>
        /// 加密头
        /// </summary>
        protected virtual string EncryptHeader => "FuXiEncrypt";
        /// <summary>
        /// 加密模式
        /// </summary>
        public virtual EncryptMode EncryptMode => EncryptMode.OFFSET;

        /// <summary>
        /// 验证是否已经加密
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <returns></returns>
        internal bool IsEncrypted(byte[] sourceBytes)
        {
            byte[] header = System.Text.Encoding.UTF8.GetBytes(EncryptHeader);
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] != sourceBytes[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 加密数据，返回加密后字节数组
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <returns></returns>
        public virtual byte[] Encrypt(byte[] sourceBytes) { return sourceBytes; }

        /// <summary>
        /// 返回加密字节数组, 用于OFFSET
        /// </summary>
        /// <returns></returns>
        public virtual byte[] EncryptOffset(){ return null; }

        /// <summary>
        /// 解密, 用于XOR
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <returns></returns>
        public virtual byte[] DeEncrypt(byte[] sourceBytes) { return sourceBytes; }
    }
}