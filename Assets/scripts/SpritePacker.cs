#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NPS
{
    [CreateAssetMenu(fileName = "New Sprite Sheet", menuName = "SpritePacker/SpriteSheet")]
    public class SpritePacker : ScriptableObject
    {
        public List<Sprite> sprites;
        public string targetPath;
        public bool usePowerOf2;
        public bool forceSquareSprites;

        private Vector2 spriteDimensions;
        private Vector2 sheetDimensions;
        private int sideLength;
        private Texture2D sheet;

        public void GenerateSheet()
        {
            ValidateTargetPath();

            if (sprites.Count == 0)
            {
                Debug.Log("There are no sprites to add to the sheet.");
            }

            //Get the info necessary to create the sheet.
            spriteDimensions = GetSpriteMaxDimensions();
            sheetDimensions = GetSheetDimensions();

            //Create the sheet
            sheet = new Texture2D((int) sheetDimensions.x, (int) sheetDimensions.y);

            //Add images to the sheet
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    int index = (i * sideLength) + j;
                    if (index < sprites.Count)
                    {
                        AddSpriteToSheet(sprites[index], new Vector2(j, i));
                    }
                }
            }

            //Save resulting sprite sheet
            byte[] bytes = sheet.EncodeToPNG();
            File.WriteAllBytes(targetPath, bytes);
            Debug.Log("File written to " + targetPath);
            UpdateSpriteAssetSettings(targetPath, sideLength);
        }

        private Vector2 GetSpriteMaxDimensions()
        {
            Vector2 MaxDimensions = Vector2.one;
            foreach (Sprite sprite in sprites)
            {
                Vector2 currentDimensions = GetSpriteDimensions(sprite);
                MaxDimensions = Vector2.Max(MaxDimensions, currentDimensions);
            }
            if (usePowerOf2)
            {
                MaxDimensions.x = Mathf.NextPowerOfTwo((int) MaxDimensions.x);
                MaxDimensions.y = Mathf.NextPowerOfTwo((int) MaxDimensions.y);
            }
            if (forceSquareSprites)
            {
                MaxDimensions.x = Mathf.Max(MaxDimensions.x, MaxDimensions.y);
                MaxDimensions.y = MaxDimensions.x;
            }
            return MaxDimensions;
        }

        private Vector2 GetSpriteDimensions(Sprite _sprite)
        {
            if (_sprite == null)
            {
                return Vector2.zero;
            }

            return new Vector2(_sprite.rect.width, _sprite.rect.height);
        }

        private void ValidateTargetPath()
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                string assetPath = AssetDatabase.GetAssetPath(this);
                string folderPath = Path.GetDirectoryName(assetPath);
                targetPath = folderPath + "/" + name + ".png";
            }

            if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
            {
                Debug.LogError("The directory you are trying to create the sprite sheet in doesn't exist.");
            }
        }

        private Vector2 GetSheetDimensions()
        {
            sideLength = Mathf.CeilToInt(Mathf.Sqrt(sprites.Count));
            return new Vector2(sideLength * spriteDimensions.x, sideLength * spriteDimensions.y);
        }

        private void AddSpriteToSheet(Sprite _sprite, Vector2 _position)
        {
            Texture2D textureToAdd = ResizeTexture(_sprite.texture);
            if (textureToAdd != null)
            {
                int xStart = (int) (_position.x * spriteDimensions.x);
                int yStart = sheet.height - (int) (_position.y * spriteDimensions.y) - (int) spriteDimensions.y;
                sheet.SetPixels(xStart, yStart, (int) spriteDimensions.x, (int) spriteDimensions.y, textureToAdd.GetPixels());
            }
        }

        /// <summary>
        /// Get a particular sprites position on the sheet based on its index.
        /// </summary>
        /// <returns>A vector indicating the index of the sprite.</returns>
        private Vector2 GetSpritePosition(int _spriteIndex)
        {
            int x = _spriteIndex % sideLength;
            int y = _spriteIndex / sideLength;

            return new Vector2(x, y);
        }

        private Texture2D ResizeTexture(Texture2D _tex)
        {
            Texture2D newTexture;

            int horizontalPadding = 0;
            int verticalPadding = 0;

            try
            {
                newTexture = new Texture2D((int) spriteDimensions.x, (int) spriteDimensions.y);
                newTexture.SetPixels(horizontalPadding, verticalPadding, _tex.width, _tex.height, _tex.GetPixels());
            }
            catch
            {
                // If read/write is disabled on the sprites, we need to enable it.
                string path = AssetDatabase.GetAssetPath(_tex);
                TextureImporter A = (TextureImporter) AssetImporter.GetAtPath(path);
                A.isReadable = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                newTexture = new Texture2D((int) spriteDimensions.x, (int) spriteDimensions.y);
                newTexture.SetPixels(horizontalPadding, verticalPadding, _tex.width, _tex.height, _tex.GetPixels());
            }

            return newTexture;
        }

        private void UpdateSpriteAssetSettings(string _assetPath, int _sideLength)
        {
            AssetDatabase.ImportAsset(_assetPath);
            TextureImporter A = (TextureImporter) AssetImporter.GetAtPath(_assetPath);
            A.textureType = TextureImporterType.Sprite;
            A.spriteImportMode = SpriteImportMode.Multiple;

            SpriteMetaData[] spriteMetaData = new SpriteMetaData[sprites.Count];
            for (int x = 0; x < sprites.Count; x++)
            {
                spriteMetaData[x] = GenerateSpriteMetaData(x);
            }
            A.spritesheet = spriteMetaData;

            AssetDatabase.ImportAsset(_assetPath, ImportAssetOptions.ForceUpdate);
        }

        private SpriteMetaData GenerateSpriteMetaData(int _spriteIndex)
        {
            Vector2 position = GetSpritePosition(_spriteIndex) * spriteDimensions;
            position.y = sheetDimensions.y - position.y - spriteDimensions.y;

            SpriteMetaData data = new SpriteMetaData
            {
                name = sprites[_spriteIndex].name,
                rect = new Rect(position, spriteDimensions)
            };
            return data;
        }
    }
}
#endif