namespace Kira
{
    public struct ChunkHandler
    {
        public IBlock[] blocks;

        public ChunkHandler(IBlock[] blocks)
        {
            this.blocks = blocks;
        }
    }
}