namespace Kira
{
    public enum BlockType
    {
        DIRT,
        GRASS,
        WOOD,
        STONE,
    }

    public interface IBlock
    {
        public BlockType GetBlockType();
    }
}