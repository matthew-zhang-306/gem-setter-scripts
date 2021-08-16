using UnityEngine;

public static class Helpers {

    public static Vector2 ToVector2(this Vector3 vector) {
        return new Vector2(vector.x, vector.y);
    }

    public static Vector3 ToVector3(this Vector2 vector, float z = 0) {
        return new Vector3(vector.x, vector.y, z);
    }
    

    public static Vector2 WithX(this Vector2 vector, float x) {
        return new Vector2(x, vector.y);
    }

    public static Vector2 WithY(this Vector2 vector, float y) {
        return new Vector2(vector.x, y);
    }

    public static Vector3 WithX(this Vector3 vector, float x) {
        return new Vector3(x, vector.y, vector.z);
    }

    public static Vector3 WithY(this Vector3 vector, float y) {
        return new Vector3(vector.x, y, vector.z);
    }

    public static Vector3 WithZ(this Vector3 vector, float z) {
        return new Vector3(vector.x, vector.y, z);
    }

    
    public static Vector2 ToTilePosition(this Vector2 vector) {
        return new Vector2(
            Mathf.Round(vector.x - 0.5f) + 0.5f,
            Mathf.Round(vector.y - 0.5f) + 0.5f
        );
    }

    public static Vector2 ToTilePosition(this Vector3 vector) {
        return new Vector2(
            Mathf.Round(vector.x - 0.5f) + 0.5f,
            Mathf.Round(vector.y - 0.5f) + 0.5f
        );
    }


    public static Color WithAlpha(this Color color, float alpha) {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static float GetHue(this Color color) {
        float h;
        Color.RGBToHSV(color, out h, out _, out _);
        return h;
    }

    public static float GetSat(this Color color) {
        float s;
        Color.RGBToHSV(color, out _, out s, out _);
        return s;
    }

    public static float GetVal(this Color color) {
        float v;
        Color.RGBToHSV(color, out _, out _, out v);
        return v;
    }

    public static Color WithHue(this Color color, float hue) {
        float s, v;
        Color.RGBToHSV(color, out _, out s, out v);
        return Color.HSVToRGB(hue, s, v);
    }

    public static Color WithSat(this Color color, float sat) {
        float h, v;
        Color.RGBToHSV(color, out h, out _, out v);
        return Color.HSVToRGB(h, sat, v);
    }

    public static Color WithVal(this Color color, float val) {
        float h, s;
        Color.RGBToHSV(color, out h, out s, out _);
        return Color.HSVToRGB(h, s, val);
    }


    public static void SetColor(this ParticleSystem particleSystem, Color color) {
        var colorModule = particleSystem.colorOverLifetime;
        colorModule.enabled = true;

        // set color gradient to match color
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(color.WithAlpha(1), 0.0f),
                new GradientColorKey(color.WithAlpha(1), 1.0f)
            }, new GradientAlphaKey[] {
                new GradientAlphaKey(color.a, 0.0f),
                new GradientAlphaKey(color.a, 1.0f)
            }
        );

        colorModule.color = grad;
    }
}

public delegate void EmptyDelegate();