
public enum Sides
{
    UP,
    DOWN,
    RIGHT,
    FRONT,
    LEFT,
    BACK
}

public static class Side
{
    public static Sides ReverseHorizontalSide(Sides side)
    {
        switch (side)
        {
            case Sides.LEFT:
                return Sides.RIGHT;
            case Sides.RIGHT:
                return Sides.LEFT;
            case Sides.FRONT:
                return Sides.BACK;
            case Sides.BACK:
                return Sides.FRONT;
            default:
                return side;
        }
    }
}
