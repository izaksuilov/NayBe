using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static NativeGallery;

public class ImageSelect : MonoBehaviour
{
	
	private void Start()
	{
		string directoryPath = Application.persistentDataPath + "/avatar.jpg";
		if (File.Exists(directoryPath))
		{
			Texture2D texture = LoadImageAtPath(directoryPath);
			GetComponent<RawImage>().texture = texture;
		}
	}

	// Start is called before the first frame update
	public void PickImage()
	{
		string directoryPath = Application.persistentDataPath + "/avatar.jpg";
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) 
		{
			Permission permission = GetImageFromGallery((filePath) =>
			{
				if (filePath != null)
				{
					File.Copy(filePath, directoryPath, true);
					GetComponent<RawImage>().texture = LoadImageAtPath(directoryPath);
				}
			}, "Выберите Ваш Аватар", "image/jpg");
		}
		//else if (Application.platform == RuntimePlatform.WindowsEditor)
		//{
		//	string filePath = Build.OpenFolderPanel("Выберите Ваш Аватар", "", "image/*");
		//	if (filePath != null)
		//	{
		//		WWW request = new WWW("file:///" + filePath);
		//		GetComponent<RawImage>().texture = request.texture;
		//	}
		//}
	}
}
