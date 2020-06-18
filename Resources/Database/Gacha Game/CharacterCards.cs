using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NepBot;
using NepBot.Data;
using NepBot.Resources.Database;
using NepBot.Resources.Extensions;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.IO;
using Imgur.API;
using Imgur.API.Models;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Authentication.Impl;
using NepBot.Resources.Code_Implements;

namespace NepBot.Resources.Database
{
    public class CharacterCards
    {
        static List<CardTypes> _c = new List<CardTypes>();
        static List<CardTypes> _r = new List<CardTypes>();
        static List<CardTypes> _sr = new List<CardTypes>();
        static List<CardTypes> _ssr = new List<CardTypes>();
        public static CardTypes[] allCards = new CardTypes[250];
        public enum Type { c, r, sr, ssr };
        public int idNumber;
        public static int idCreator = 0;
        public string CommonCards { get; } = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", @"Data\Character Cards\C");
        public string RareCards { get; } = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", @"Data\Character Cards\R");
        public string SRCards { get; } = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", @"Data\Character Cards\SR");
        public string SSRCards { get; } = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", @"Data\Character Cards\SSR");
        public static List<CardTypes> C { get => _c; set => _c = value; }
        public static List<CardTypes> R { get => _r; set => _r = value; }
        public static List<CardTypes> Sr { get => _sr; set => _sr = value; }
        public static List<CardTypes> Ssr { get => _ssr; set => _ssr = value; }

        public static int CardsCount
        {
            get
            {
                return C.Count + R.Count + Sr.Count + Ssr.Count;
            }
        }

        public CharacterCards()
        {
            LoadCardImages();
        }

        public CardTypes RandomCard()
        {
            List<CardTypes> ct = RandomCardList;
            int g = UtilityClass.ReturnRandom(0, ct.Count);
            return ct[g];
        }

        private List<CardTypes> RandomCardList
        {
            get
            {
                int g = UtilityClass.ReturnRandom(0, 101);
                if (g <= 78)
                    return C;
                else if (g > 78 && g <= 90)
                    return R;
                else if (g > 90 && g <= 98)
                    return Sr;
                else if (g > 98 && g <= 100)
                    return Ssr;
                return null;
            }
        }

        public void ImageSorter(string[] fileFolder, List<CardTypes> ctypes, Type t, int hack)
        {
            try
            {
                string[] path = File.ReadAllText(Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", @"Data\Character Cards\Imgur URLs.txt")).Split(new char[] { '|' });
                for (int i = 0; i < fileFolder.Length; i++)
                {
                    FileInfo f = new FileInfo(fileFolder[i]);
                    List<string> splits = f.Name.Split(new string[] { "_", "." }, StringSplitOptions.None).ToList();
                    Image im = Image.FromFile(fileFolder[i]);
                    ctypes.Add(new CardTypes(int.Parse(splits[2]), t, splits[1].Replace('-', ' '), im, fileFolder[i], int.Parse(splits[3])));// splits[4]));                                        
                    im.Dispose();
                    //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.GenericMessage, null, string.Concat("\n", ctypes[i].name, "\n"));
                    //string ggg = ImgurImplement.UploadImage(ctypes[i]).Result;
                    //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.GenericMessage, null, $"{ggg}|{ctypes[i].idNumber}|");
                    //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.CustomMessage, null, fileFolder[i]);
                    //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.GenericMessage, null, $"File Name: {f.Name} || {ctypes[i].name}\n");
                    allCards[ctypes[i].idNumber] = ctypes[i];
                }
                int sorter = 0;
                string imgid = "";
                foreach (string g in path)
                {
                    switch (sorter)
                    {
                        case 0:
                            imgid = g;
                            sorter = 1;
                            break;
                        case 1:
                            for (int i = 0; i < ctypes.Count; i++)
                            {
                                if (ctypes[i].idNumber.ToString() == g)
                                {
                                    ctypes[i].SetImgurID(imgid);
                                    break;
                                }
                            }
                            sorter = 0;
                            break;
                    }
                }

                //for (int i = 0; i < ctypes.Count; i++)
                //  ExtensionMethods.WriteToLog(ExtensionMethods.LogType.GenericMessage, null, $"{ctypes[i].imgurID} | {ctypes[i].name}|\n");
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, "From Character Cards");
                throw;
            }
            switch (hack)
            {
                case 0:
                    _c = ctypes;
                    break;
                case 1:
                    _r = ctypes;
                    break;
                case 2:
                    _sr = ctypes;
                    break;
                case 3:
                    _ssr = ctypes;
                    break;
            }
        }

        private void PrintOutCardData()
        {
            foreach (CardTypes cc in allCards)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.GenericMessage, null, $"Name: {cc.name}\nID Number: {cc.idNumber}\nImgur ID: {cc.imgurID}\nPath: {cc.path}\n______________________________\n");
            }
        }

        public void LoadCardImages()
        {
            ImageSorter(Directory.GetFiles(CommonCards, "*.*", SearchOption.TopDirectoryOnly), _c, Type.c, 0);
            ImageSorter(Directory.GetFiles(RareCards, "*.*", SearchOption.TopDirectoryOnly), _r, Type.r, 1);
            ImageSorter(Directory.GetFiles(SRCards, "*.*", SearchOption.TopDirectoryOnly), _sr, Type.sr, 2);
            ImageSorter(Directory.GetFiles(SSRCards, "*.*", SearchOption.TopDirectoryOnly), _ssr, Type.ssr, 3);
        }
    }

    public class CardTypes
    {
        public readonly CharacterCards.Type _cardType;
        public readonly int powerLevel;
        public readonly string name;
        public readonly Image _cardImage;
        public readonly string path;
        public readonly int idNumber;
        public string imgurID;

        public CardTypes(int powerLevel, CharacterCards.Type cardType, string name, Image cardImage, string path, int idnum)
        {
            this.powerLevel = powerLevel;
            _cardType = cardType;
            _cardImage = cardImage;
            this.name = name;
            this.path = path;
            idNumber = idnum;
        }

        public void SetImgurID(string id)
        {
            imgurID = id;
        }
    }
}
