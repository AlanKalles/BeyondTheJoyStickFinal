using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 准心纹理生成器 - 用于生成简单的准心图案纹理
    /// </summary>
    public class CrosshairTextureGenerator : MonoBehaviour
    {
        /// <summary>
        /// 生成一个简单的十字准心纹理
        /// </summary>
        public static Texture2D GenerateCrosshairTexture(int size = 128, Color color = default, int lineWidth = 4, int gapSize = 16)
        {
            if (color == default)
            {
                color = Color.white;
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            // 填充透明背景
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            int center = size / 2;
            int halfLineWidth = lineWidth / 2;
            int halfGap = gapSize / 2;

            // 绘制十字准心
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    bool isInHorizontalLine = Mathf.Abs(y - center) < halfLineWidth;
                    bool isInVerticalLine = Mathf.Abs(x - center) < halfLineWidth;
                    bool isInGap = Mathf.Abs(x - center) < halfGap && Mathf.Abs(y - center) < halfGap;

                    // 绘制水平线（排除中心间隙）
                    if (isInHorizontalLine && !isInGap)
                    {
                        pixels[y * size + x] = color;
                    }

                    // 绘制垂直线（排除中心间隙）
                    if (isInVerticalLine && !isInGap)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }

            // 绘制中心点
            int dotSize = 6;
            int halfDot = dotSize / 2;
            for (int x = center - halfDot; x < center + halfDot; x++)
            {
                for (int y = center - halfDot; y < center + halfDot; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (distance < halfDot)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// 生成一个圆形准心纹理
        /// </summary>
        public static Texture2D GenerateCircleCrosshairTexture(int size = 128, Color color = default, int lineWidth = 4, int radius = 40)
        {
            if (color == default)
            {
                color = Color.white;
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            // 填充透明背景
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            int center = size / 2;
            int halfLineWidth = lineWidth / 2;

            // 绘制圆形准心
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                    // 外圆环
                    if (distance >= radius - halfLineWidth && distance <= radius + halfLineWidth)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }

            // 绘制中心点
            int dotSize = 8;
            for (int x = center - dotSize / 2; x < center + dotSize / 2; x++)
            {
                for (int y = center - dotSize / 2; y < center + dotSize / 2; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (distance < dotSize / 2)
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器中创建准心纹理资源
        /// </summary>
        [MenuItem("FishAndFisher/生成准心纹理/十字准心")]
        public static void CreateCrosshairTextureAsset()
        {
            Texture2D texture = GenerateCrosshairTexture(128, Color.white, 4, 16);

            // 保存为资源
            string path = "Assets/Texture/CrosshairTexture.png";
            System.IO.Directory.CreateDirectory("Assets/Texture");

            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();

            Debug.Log($"准心纹理已生成: {path}");
        }

        /// <summary>
        /// 在编辑器中创建圆形准心纹理资源
        /// </summary>
        [MenuItem("FishAndFisher/生成准心纹理/圆形准心")]
        public static void CreateCircleCrosshairTextureAsset()
        {
            Texture2D texture = GenerateCircleCrosshairTexture(128, Color.white, 4, 40);

            // 保存为资源
            string path = "Assets/Texture/CircleCrosshairTexture.png";
            System.IO.Directory.CreateDirectory("Assets/Texture");

            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();

            Debug.Log($"圆形准心纹理已生成: {path}");
        }
#endif
    }
}
