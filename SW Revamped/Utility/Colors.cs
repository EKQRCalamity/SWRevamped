using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SWRevamped.Utility
{
    internal static class Colors
    {
        internal static bool ColorEquals(Color color, Color otherColor)
        {
            return color.A == otherColor.A && color.B == otherColor.B && color.G == otherColor.G && color.A == otherColor.A;
        }

        internal static Color NameToColor(String color)
        {
            return color switch
            {
                "Red" => Color.Red,
                "Orange" => Color.Orange,
                "Green" => Color.Green,
                "Blue" => Color.Blue,
                "Black" => Color.Black,
                "Yellow" => Color.Yellow,
                "LightGreen" => Color.LightGreen,
                "OrangeRed" => Color.OrangeRed,
                _ => Color.White,
            };
        }

        internal static string ColorToName(Color color)
        {
            string name = "";
            // Blue, Red, OrangeRed, White, Black, Orange, Green, LightGreen, Yellow
            switch (color)
            {
                case var value when ColorEquals(color, Color.Red):
                    name = "Red";
                    break;
                case var value when ColorEquals(color, Color.Orange):
                    name = "Orange";
                    break;
                case var value when ColorEquals(color, Color.Green):
                    name = "Green";
                    break;
                case var value when ColorEquals(color, Color.Blue):
                    name = "Blue";
                    break;
                case var value when ColorEquals(color, Color.Black):
                    name = "Black";
                    break;
                case var value when ColorEquals(color, Color.Yellow):
                    name = "Yellow";
                    break;
                case var value when ColorEquals(color, Color.LightGreen):
                    name = "LightGreen";
                    break;
                case var value when ColorEquals(color, Color.White):
                    name = "White";
                    break;
                case var value when ColorEquals(color, Color.OrangeRed):
                    name = "OrangeRed";
                    break;
                default:
                    name = "White";
                    break;
            }
            return name;
        }
    }
}
