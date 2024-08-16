using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ScriptedImporter(1, "sdf")]
public class ImporterSDF : ScriptedImporter
{
    private struct Tex3DDefinition
    {
        public string name;
        public int width;
        public int height;
        public int depth;
        public int channels;
        public float[] data;
    }

    private GraphicsFormat GetGraphicsFormatFromChannelCount(int channelCount)
    {
        switch (channelCount)
        {
            case 1:
                return GraphicsFormat.R32_SFloat;
            case 2:
                return GraphicsFormat.R16G16_SFloat ;
            case 3:
                return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
            case 4:
                return GraphicsFormat.R8G8B8A8_UNorm;
            default:
                break;
        }

        return GraphicsFormat.None;
    }

    private Color ColorFromArray(float[] channelArray)
    {
        
        if (channelArray.Length == 1)
        {
            return new Color(channelArray[0], channelArray[0], channelArray[0], channelArray[0]);
        }

        if (channelArray.Length == 2)
        {
            return new Color(channelArray[0], channelArray[1], channelArray[1], channelArray[1]);
        }
        
        if (channelArray.Length == 3)
        {
            return new Color(channelArray[0], channelArray[1], channelArray[2], channelArray[2]);
        }
        
        if (channelArray.Length == 4)
        {
            return new Color(channelArray[0], channelArray[1], channelArray[2], channelArray[3]);
        }

        throw new ArgumentOutOfRangeException("channelArray is larger than 4 cahnnels or smaller than 1 channel");
    }

    private void Construct3DTextureFromDefinition(Tex3DDefinition definition, string databasePath)
    {

        string path = Path.Join(Path.GetDirectoryName(databasePath), definition.name + ".asset");

        GraphicsFormat format = GetGraphicsFormatFromChannelCount(definition.channels);
        Texture3D tex3D = new Texture3D(definition.width, definition.height, definition.depth, format,
            TextureCreationFlags.None);
        List<Color> colors = new List<Color>();
        for (int pixel = 0; pixel < definition.data.Length; pixel += definition.channels)
        {
            float[] color = new float[definition.channels];
            for (int channel = 0; channel < color.Length; channel++)
            {
                color[channel] = definition.data[pixel + channel];
            }
            Color c = ColorFromArray(color);
            colors.Add(c);
        }
        tex3D.SetPixels(colors.ToArray());
        tex3D.Apply();
        AssetDatabase.CreateAsset(tex3D, path);
    }
    
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string path = Path.Join(Application.dataPath.Replace("Assets", ""), ctx.assetPath);
        string jsonString = "";
        using (StreamReader r = new StreamReader(path))
        {
            jsonString = r.ReadToEnd();
        }
        Tex3DDefinition definition = JsonUtility.FromJson<Tex3DDefinition>(jsonString);
        Construct3DTextureFromDefinition(definition, ctx.assetPath);
        AssetDatabase.DeleteAsset(ctx.assetPath);
    }
}
