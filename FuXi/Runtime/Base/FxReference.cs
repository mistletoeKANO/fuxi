namespace FuXi
{
    public class FxReference
    {
        internal int RefCount;
        internal void AddRef()
        {
            this.RefCount++;
        }
        internal bool SubRef()
        {
            return --RefCount <= 0;
        }
    }
}