public enum ItemGroup
{
    EXTRA_RARE = 10,
    RARE = 30,
    COMMON = 60,
}

static class ItemGroupMethods
{
    public static ItemGroup FromInt(int value) {
        if (value <= (int)ItemGroup.EXTRA_RARE)
        {
            return ItemGroup.EXTRA_RARE;
        }
        
        if (value <= (int)ItemGroup.RARE)
        {
            return ItemGroup.RARE;
        }

        return ItemGroup.COMMON;
    }
}