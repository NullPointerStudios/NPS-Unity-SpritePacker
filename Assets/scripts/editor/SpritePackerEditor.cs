using UnityEngine;
using System.Collections;
using UnityEditor;

namespace NPS
{
    [CustomEditor(typeof(SpritePacker))]
    public class SpritePackerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SpritePacker packer = (SpritePacker) target;
            if (GUILayout.Button("Generate Sheet"))
            {
                packer.GenerateSheet();
            }
        }
    }
}