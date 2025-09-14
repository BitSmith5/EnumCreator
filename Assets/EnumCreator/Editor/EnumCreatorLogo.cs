using UnityEngine;
using UnityEditor;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Utility class for creating and managing the EnumCreator logo
    /// </summary>
    public static class EnumCreatorLogo
    {
        private static Texture2D _logoTexture;
        
        /// <summary>
        /// Gets or creates the EnumCreator logo texture
        /// </summary>
        public static Texture2D LogoTexture
        {
            get
            {
                if (_logoTexture == null)
                {
                    _logoTexture = CreateLogoTexture();
                }
                return _logoTexture;
            }
        }
        
        /// <summary>
        /// Creates a professional logo texture programmatically
        /// </summary>
        private static Texture2D CreateLogoTexture()
        {
            int width = 64;
            int height = 64;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            Color[] pixels = new Color[width * height];
            
            // Create a professional gradient background with enum-themed design
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(x, y, width, height);
                    pixels[y * width + x] = pixelColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        /// <summary>
        /// Calculates the color for each pixel to create the logo design
        /// </summary>
        private static Color GetPixelColor(int x, int y, int width, int height)
        {
            float centerX = width / 2f;
            float centerY = height / 2f;
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
            float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(centerX, centerY));
            
            // Create a radial gradient
            float gradient = 1f - (distance / maxDistance);
            
            // Base colors for professional look
            Color baseColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Blue
            Color accentColor = new Color(0.1f, 0.7f, 0.9f, 1f); // Cyan
            
            // Create gradient effect
            Color pixelColor = Color.Lerp(baseColor, accentColor, gradient);
            
            // Add some geometric patterns to represent enums
            if (IsInEnumPattern(x, y, width, height))
            {
                pixelColor = Color.Lerp(pixelColor, Color.white, 0.3f);
            }
            
            return pixelColor;
        }
        
        /// <summary>
        /// Creates geometric patterns that represent enum values
        /// </summary>
        private static bool IsInEnumPattern(int x, int y, int width, int height)
        {
            // Create small squares representing enum values
            int squareSize = 8;
            int spacing = 12;
            
            int gridX = (x + spacing / 2) / spacing;
            int gridY = (y + spacing / 2) / spacing;
            
            if (gridX % 2 == 0 && gridY % 2 == 0)
            {
                int localX = (x + spacing / 2) % spacing;
                int localY = (y + spacing / 2) % spacing;
                
                if (localX < squareSize && localY < squareSize)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Cleans up the logo texture
        /// </summary>
        public static void Cleanup()
        {
            if (_logoTexture != null)
            {
                Object.DestroyImmediate(_logoTexture);
                _logoTexture = null;
            }
        }
    }
}
