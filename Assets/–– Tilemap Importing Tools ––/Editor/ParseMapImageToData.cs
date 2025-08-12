using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

#if UNITY_EDITOR
public class ParseMapImageToData : MonoBehaviour {

	public InfoProperty info = new InfoProperty( "Using Parse Map Image To Data",
		"1. Click the <b>Get Instructions from Project Brief</b> button below.\n\n" +
		"2. In that document, scroll down to the <b>Importing a Classic Level Image to Unity TileMap System</b> section.\n\n");

	
	public const string PROJECT_BRIEF_URL = "https://tinyurl.com/prod121-celeste-remix";
	[NaughtyAttributes.Button( "Get Instructions from Project Brief" )]
	void GetInstructions() {
		Application.OpenURL( PROJECT_BRIEF_URL );
	}
	
	[NaughtyAttributes.Button("Parse Map Image")]
	void Parse() {
		ParseMap();
	}

	[NaughtyAttributes.Button]
	void ClearDynamicFields() {
		outputSprites = null;
		numSprites = 0;
		checkSums.Clear();
		mapW = 0;
		mapData = null;
		newData = null;
		indices = null;
    }

    [Header("Inscribed - These must be set in the inspector")]
	public Texture2D    inputMap;
	public int			tileSizeInPixels = 16;
	public int       	outputSpritesTextureSize = 256;
    public string       outputMapTexturePNG = "mapTexture";
    public string       outputMapDataTXT = "mapData";
	[HideInInspector]
	public Vector2[]	stopPoints;

    [Header("Dynamic - Set automatically by the code")]
	public Texture2D    outputSprites;
	[NaughtyAttributes.ReadOnly]
	public int       	numSprites = 0;
	[NaughtyAttributes.ReadOnly]
	public List<ulong>	checkSums;

	private int        	mapW;
	private Color32[] 	mapData, newData;
	private string[]    indices;

	
	// Use this for initialization
	void Start () {
		//StartCoroutine( ParseMap() );
	}
	
	// Update is called once per frame
	public void ParseMap() {
		ClearDynamicFields(); // Just in case
		// Pull in the original Metroid map
		mapW = inputMap.width;
		int w = inputMap.width/tileSizeInPixels;
		int h = inputMap.height/tileSizeInPixels;
		
		indices = new string[w*h];
		
		mapData = inputMap.GetPixels32(0); // This will take a long time and a LOT of memory!
		
		// Create a new texture to hold the individual sprites
		newData = new Color32[outputSpritesTextureSize * outputSpritesTextureSize];
		outputSprites = new Texture2D(outputSpritesTextureSize, outputSpritesTextureSize, TextureFormat.ARGB32, false);
		
		// Create a list of checkSums for the individual sprites
		checkSums = new List<ulong>();
		
		ulong cs;
		int found = -1;
		int ndx;
		// Parse it one 16x16-pixel section at-a-time
		for (int j=0; j<h; j++) {
			for (int i=0; i<w; i++) {
				foreach (Vector2 stopPoint in stopPoints) {
					if (i == stopPoint.x && j == stopPoint.y) {
						print ("Hit a stopPoint: "+i+"x"+j);
					}
				}


				Color32[] chunk = GetChunk(i,j);
				// Convert this section to a checkSum
				cs = CheckSum(chunk);
				
				// Check to see whether the current checkSum matches an already-found one
				found = -1;
				for (int k=0; k<checkSums.Count; k++) {
					if (cs == checkSums[k]) {
						found = k;
						break;
					}
				}
				// If it doesn't, make a new checkSum and a new entry in the outputSprites Texture2D.
				if (found == -1) {
					checkSums.Add(cs);
					OutputChunk(chunk);
					found = numSprites;
					numSprites++;
                    print("Found Checksum: "+cs);
				}
				ndx = i + j*w;
//                if (ndx >= indices.Length || ndx < 0 || found >= MAP_CHARS.Length || found < 0) {
//                    SaveTextureToFile(outputSprites, "Resources/"+outputMapTexturePNG+".png");
//                    print("Break");
//                }
                if (found == 0) {
                    indices[ndx] = "..";
                } else {
                    indices[ndx] = found.ToString("x2"); //"D3");
                }
				//                print ("i="+i+"\tj="+j+"\tSprites found:"+numSprites);
			}
			print ("j="+j+"\tSprites found:"+numSprites);

			
			//yield return null;
		}
		
		// Generate the Texture2D from the newData
		outputSprites.SetPixels32(newData, 0);
		outputSprites.Apply(true);
		
        SaveTextureToFile(outputSprites, "Resources/"+outputMapTexturePNG+".png");
        print("Wrote file: Resources/"+outputMapTexturePNG+".png");
		
		// Output the text file 
		string[] ind2 = new string[h];
		string[] indTemp = new string[w];
		for (int i=0; i<h; i++) {
			System.Array.Copy(indices, i*w, indTemp, 0, w);
            ind2[i] = string.Join(" ", indTemp);
		}
		string str = string.Join("\n",ind2);
		
        print (str);
        File.WriteAllText(Application.dataPath+"/Resources/"+outputMapDataTXT+".txt", str);
        print("Wrote file: Resources/"+outputMapDataTXT+".txt");

        print("ParseMap() Complete!");
	}
	
	
	public Color32[] GetChunk(int x, int y) {
		Color32[] res = new Color32[tileSizeInPixels*tileSizeInPixels];
		x *= tileSizeInPixels;
		y *= tileSizeInPixels;
		int ndx;
		for (int j=0; j<tileSizeInPixels; j++) {
			for (int i=0; i<tileSizeInPixels; i++) {
				ndx = x+i + (y+j)*mapW;
				try {
					res[i + j*tileSizeInPixels] = mapData[ ndx ];
				}
				catch (System.IndexOutOfRangeException) {
					print ("GetChunk() Index out of range:"+ndx+"\tLength:"+mapData.Length+"\ti="+i+"\tj="+j);
				}
			}
		}
		return res;
	}
	
	public ulong CheckSum(Color32[] chunk) {
		ulong res = 0;
		for (int i=0; i<chunk.Length; i++) {
            
            res += (ulong) (chunk[i].r * (i+1) + chunk[i].g * (i+2) + chunk[i].b * (i+3));

            //res += (ulong) ( (int) chunk[i].r * 1000000 + (int) chunk[i].g * 1000 + (int) chunk[i].b + i);
            /*
			switch (i%3) {
			case 0:
				res += (ulong) ( (int) chunk[i].r * 1000000 + (int) chunk[i].g * 1000 + (int) chunk[i].b );
				break;
			case 1:
				res += (ulong) ( (int) chunk[i].g * 1000000 + (int) chunk[i].b * 1000 + (int) chunk[i].r );
				break;
			case 2:
				res += (ulong) ( (int) chunk[i].b * 1000000 + (int) chunk[i].r * 1000 + (int) chunk[i].g );
				break;
			}
            */         
		}
		return res;
	}
	
	void OutputChunk(Color32[] chunk) {
		int spl = outputSpritesTextureSize / tileSizeInPixels;
		int x = numSprites % spl;
		int y = numSprites / spl;
        y = spl - y - 1;
		x *= tileSizeInPixels;
		y *= tileSizeInPixels;
		
		int ndxND, ndxC;
		for (int i=0; i<tileSizeInPixels; i++) {
			for (int j=0; j<tileSizeInPixels; j++) {
				ndxND = x+i + (y+j)*outputSpritesTextureSize;
				ndxC = i + j*tileSizeInPixels;
				
				try {
					newData[ ndxND ] = chunk[ ndxC ];
				}
				catch (System.IndexOutOfRangeException) {
					print ("OutputChunk() Index out of range:"+ndxND+"\tLengthND:"+newData.Length+"\tndxC="+ndxC+"\tLengthC"+chunk.Length+"\ti="+i+"\tj="+j);
				}
			}
		}
	}
	
	
	void SaveTextureToFile( Texture2D tex, string fileName) {
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/"+fileName, bytes);
	}
	
	
}
#endif