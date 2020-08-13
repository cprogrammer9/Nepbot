using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NepBot.Resources.Database
{
	public sealed class ImageData
	{
		public readonly string[] gifurls = File.ReadAllLines(Program.DataPath("bot gifs", "txt"));

		public readonly string[] monikaImages;

		public List<ACard> allCards = new List<ACard>();

		public List<MemeStorage> memeStorage = new List<MemeStorage>();

		private string[] MonikaImages()
		{
			string path = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Monika");
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			FileInfo[] files = directoryInfo.GetFiles();
			List<string> list = new List<string>();
			for (int i = 0; i < files.Length; i++)
			{
				list.Add(files[i].FullName);
			}
			return list.ToArray();
		}

		private void AddCards()
		{
			string str = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Cards\\Hearts");
			string str2 = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Cards\\Spades");
			string str3 = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Cards\\Diamonds");
			string str4 = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Cards\\Clubs");
			for (int i = 2; i < 15; i++)
			{
				this.allCards.Add(new ACard((i <= 10) ? i : 10, str + "\\" + i.ToString() + ".jpg"));
				this.allCards.Add(new ACard((i <= 10) ? i : 10, str2 + "\\" + i.ToString() + ".jpg"));
				this.allCards.Add(new ACard((i <= 10) ? i : 10, str3 + "\\" + i.ToString() + ".jpg"));
				this.allCards.Add(new ACard((i <= 10) ? i : 10, str4 + "\\" + i.ToString() + ".jpg"));
			}
		}

		public ImageData()
		{
			this.AddCards();
			this.monikaImages = this.MonikaImages();
			memeStorage.Add(new MemeStorage(8, .34f, 584, 393, $@"{Program.DataPath(@"Meme Image Placeovers\8dd", "png")}", "catdogmouse"));
			memeStorage.Add(new MemeStorage(8, .05f, 800, 900, $@"{Program.DataPath(@"Meme Image Placeovers\monikapoint", "png")}", "monikapoint"));
			memeStorage.Add(new MemeStorage(-1, .001f, 1000, 647, $@"{Program.DataPath(@"Meme Image Placeovers\mathwtf", "jpg")}", "mathwtf"));
		}
	}
}

public sealed class ACard
{
	private int _cardValue;

	public bool aceToOne = false;

	public int CardValue
	{
		get
		{
			bool flag = this.aceToOne && this.IsAce;
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				result = this._cardValue;
			}
			return result;
		}
		set
		{
			this._cardValue = value;
		}
	}

	public string CardImage { get; }

	public ACard(int cv, string ci)
	{
		this.CardValue = ((!ci.Contains("14.jpg")) ? cv : 11);
		this.CardImage = ci;
	}

	public bool IsAce
	{
		get
		{
			return this.CardImage.Contains("14.jpg");
		}
	}
}

public struct MemeStorage
{
	public readonly float offsetX;
	public readonly float offsetY;
	public readonly int sizeWidth;
	public readonly int sizeHeight;
	public readonly string imgPath;
	public readonly string memeName;

	public MemeStorage(float offsetX, float offsetY, int sizeWidth, int sizeHeight, string imgPath, string memeName)
	{
		this.offsetX = offsetX;
		this.offsetY = offsetY;
		this.sizeWidth = sizeWidth;
		this.sizeHeight = sizeHeight;
		this.imgPath = imgPath;
		this.memeName = memeName;
	}

}