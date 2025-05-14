using System;

public static class Custom
{
	public static int Round(float f) {
        return (int) Math.Round(f, 0, MidpointRounding.AwayFromZero);
    }
}
