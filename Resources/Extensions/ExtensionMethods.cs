using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using ConvenienceMethods;
using Discord.WebSocket;
using System.Linq;
using NepBot.Data;
using Discord.Commands;
using Discord;
using Discord.Rest;
using System.Text;

namespace NepBot.Resources.Extensions
{
    public static class ExtensionMethods
    {
        public enum TagT { role, user, channel }

        public static string[] CharacterLimit(this string input)
        {
            var str = input.ToCharArray();
            StringBuilder sb = new StringBuilder();
            List<string> ls = new List<string>();
            if (str.Length < 1850)
            {
                ls.Add(input);
                return ls.ToArray();
            }
            for (int i = 0; i < str.Length; i++)
            {
                sb.Append(str[i]);
                if (sb.ToString().Length >= 1850 && (str[i] == ' ' || str[i] == '\n'))
                {
                    ls.Add(sb.ToString());
                    sb = new StringBuilder();
                }
            }
            ls.Add(sb.ToString());
            return ls.ToArray();
        }

        public static string FormatTag(this ulong f, TagT tagType)
        {
            string format = string.Empty;

            if (tagType == TagT.channel)
            {
                format = $"<#{f}>";
            }
            else if (tagType == TagT.role)
            {
                format = $"<@&{f}>";
            }
            else if (tagType == TagT.user)
            {
                format = $"<@!{f}>";
            }
            return format;
        }

        public static string NeptuneEmojis(bool neptuneWon)
        {
            //<:nepsparkles:591333460757577738>
            //<:nephuuh:591333461067956224>
            if (neptuneWon)
                return "<:nepsparkles:591333460757577738>";
            return "<:nephuuh:591333461067956224>";
        }

        public static string MakeCustomEmoji(this string toRep, string name, string id)
        {
            //<:perpell:571875093870018576>
            return $"<:{name}:{id}>";
        }

        public static System.DrawingCore.Imaging.ImageFormat ImageFormatType(this Bitmap bmp)
        {
            System.DrawingCore.Imaging.ImageFormat ff = System.DrawingCore.Imaging.ImageFormat.Jpeg;
            if (bmp.RawFormat.Equals(System.DrawingCore.Imaging.ImageFormat.Gif))
                ff = System.DrawingCore.Imaging.ImageFormat.Gif;
            if (bmp.RawFormat.Equals(System.DrawingCore.Imaging.ImageFormat.Png))
                ff = System.DrawingCore.Imaging.ImageFormat.Png;
            return ff;
        }

        public static void TextToImage(List<string> splitting, ref Bitmap bitMap, ref MemoryStream stream, bool hasAttachment)
        {
            GraphicsMethods gr = new GraphicsMethods();
            for (int i = (!hasAttachment) ? 1 : 0; i < splitting.Count; i++)
            {
                if (i >= 3)
                    break;
                bitMap = gr.InsertTextOnImage("Impact", 150f, bitMap, splitting[i], (hasAttachment) ? i > 0 : i > 1, false);
            }
            bitMap.Save(stream, bitMap.RawFormat);
            stream.Position = 0;
        }

        public static string RemoveTagCharacters(this string bas)
        {
            string[] chars = new string[] { "<!", "<@!", ">", "<@", "<#" };
            foreach (string g in chars)
            {
                bas = bas.Replace(g, string.Empty);
            }
            return bas;
        }

        public static string[] GenericSplit(string txt, params string[] splitter)
        {
            //https://media.discordapp.net/attachments/474802733438861312/733210441446195250/Screen_Shot_2020-07-16_at_2.04.55_AM.png?width=149&height=678
            if (txt.Contains("discordapp") && txt.Contains("?"))
            {
                var p = txt.Split('?');
                txt = txt.Replace(p[1], string.Empty);
            }
            if (txt == null)
                return null;
            string[] txtSplit = txt.Split(splitter, StringSplitOptions.None);
            txtSplit[0] = txtSplit[0].RemoveTagCharacters();
            return txtSplit;
        }

        public static string NameGetter(SocketUser su, SocketGuild sg)
        {
            bool flag = su == null;
            string result;
            if (flag)
            {
                result = "(User not in server anymore)";
            }
            else
            {
                SocketGuildUser user = sg.GetUser(su.Id);
                bool flag2 = string.IsNullOrEmpty(user.Nickname) || string.IsNullOrWhiteSpace(user.Nickname);
                if (flag2)
                {
                    result = user.Username;
                }
                else
                {
                    result = user.Nickname;
                }
            }
            return result;
        }

        public static void WriteToLog(ExtensionMethods.LogType logType, Exception m, string customMessage = "")
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame frame = stackTrace.GetFrame(1);
            Type reflectedType = frame.GetMethod().ReflectedType;
            MethodBase method = frame.GetMethod();
            string path = string.Empty;
            string text = string.Empty;
            string str = string.Empty;
            switch (logType)
            {
                case ExtensionMethods.LogType.Debug:
                    str = "Debug";
                    text = "Exception Message: " + m.Message + " <::> Custom Message: " + customMessage;
                    break;
                case ExtensionMethods.LogType.ErrorLog:
                    str = "Error Messages";
                    text = string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\nCustom Message (if any): {8}<<<", new object[]
                    {
                    m.Message,
                    m.TargetSite,
                    m.Source,
                    m.InnerException,
                    m.StackTrace,
                    m.HResult,
                    m.Data,
                    m.HelpLink,
                    customMessage
                    });
                    break;
                case ExtensionMethods.LogType.CustomMessage:
                    str = "Custom Message";
                    text = customMessage;
                    break;
                case ExtensionMethods.LogType.GenericMessage:
                    str = "Generic Message";
                    path = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Logs\\" + str + ".txt");
                    File.AppendAllText(path, customMessage);
                    return;
            }
            path = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\Logs\\" + str + ".txt");
            File.AppendAllText(path, string.Format("__________________________________________________________________\nCalled from class: {0} <::> Method: {1}\n{2}: {3}\n", new object[]
            {
                reflectedType,
                method,
                DateTime.Now,
                text
            }));
        }

        public static List<string> RecreateMemeTextList(List<string> f)
        {
            List<string> list = new List<string>();
            switch (f.Count)
            {
                case 1:
                    list.Add(f[0]);
                    list.Add("");
                    list.Add("");
                    list.Add("");
                    break;
                case 2:
                    list.Add(f[0]);
                    list.Add(f[1]);
                    list.Add("");
                    list.Add("");
                    break;
                case 3:
                    list.Add(f[0]);
                    list.Add(f[1]);
                    list.Add(f[2]);
                    list.Add("");
                    break;
                case 4:
                    list.Add(f[0]);
                    list.Add(f[1]);
                    list.Add(f[2]);
                    list.Add(f[3]);
                    break;
                case 5:
                case 6:
                case 7:
                    {
                        list.Add(f[0]);
                        list.Add(f[1]);
                        list.Add(f[2]);
                        list.Add(f[3]);
                        List<string> list2 = list;
                        list2[2] = list2[2] + "...";
                        break;
                    }
            }
            return list;
        }

        public static string NicknameChecker(this string name, SocketGuildUser su)
        {
            bool checkNickname = !string.IsNullOrEmpty(su.Nickname) || !string.IsNullOrWhiteSpace(su.Nickname);
            if (checkNickname)
            {
                return su.Nickname;
            }
            return su.Username;
        }

        public static string NicknameChecker(this ulong name, SocketGuildUser su)
        {
            bool checkNickname = !string.IsNullOrEmpty(su.Nickname) || !string.IsNullOrWhiteSpace(su.Nickname);
            if (checkNickname)
            {
                return su.Nickname;
            }
            return su.Username;
        }

        public static ulong FindUserIdByName(string username, SocketCommandContext context)
        {
            username = username.ToLower();
            foreach (SocketGuildUser socketGuildUser in context.Guild.Users)
            {

                bool flag2 = !string.IsNullOrEmpty(socketGuildUser.Nickname) || !string.IsNullOrWhiteSpace(socketGuildUser.Nickname);
                if (flag2)
                {
                    bool flag3 = socketGuildUser.Nickname.ToLower().Contains(username);
                    if (flag3)
                    {
                        return socketGuildUser.Id;
                    }
                }
                bool flag4 = socketGuildUser.Username.ToLower().Contains(username);
                if (flag4)
                {
                    return socketGuildUser.Id;
                }
            }
            return 0;
        }

        /// <summary>
        /// Used to get the user's name based on their ID number when being tagged. Can also tostring a ulong from userdata to get their ID as well.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="context"></param>
        /// <param name="requireExactName"></param>
        /// <returns></returns>
        public static string GetUsersName(string username, SocketCommandContext context, bool requireExactName = false)
        {
            try
            {
                username = username.RemoveTagCharacters().ToLower();
                //ulong isId = 0;
                string toReturn = string.Empty;
                if (ulong.TryParse(username, out ulong isId))
                {
                    return toReturn.NicknameChecker(context.Guild.GetUser(isId));
                }
                else
                {
                    return toReturn.NicknameChecker(context.Guild.GetUser(FindUserIdByName(username, context)));
                }
            }
            catch (Exception m)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, "From: FindGuildUser");
            }
            return string.Empty;
        }

        public static SocketUser GetSocketUser(string username, SocketCommandContext context, bool requireExactName = false)
        {
            try
            {
                IReadOnlyCollection<SocketGuildUser> f = context.Guild.Users;
                List<SocketGuildUser> matchesFound = new List<SocketGuildUser>();
                username = username.RemoveTagCharacters().ToLower();
                username = username.Replace("~", string.Empty);
                ulong isId = 0;
                if (ulong.TryParse(username, out isId))
                {
                    return context.Guild.GetUser(isId);
                }

                foreach (SocketGuildUser socketGuildUser2 in f)
                {
                    if (NicknameOrNot(socketGuildUser2, username) != 2)
                        matchesFound.Add(socketGuildUser2);
                }
                if (matchesFound.Count == 1)
                    return matchesFound[0];

                foreach (SocketGuildUser sgu in matchesFound)
                {
                    string name = (NicknameOrNot(sgu, username) == 0) ? sgu.Nickname.ToLower() : sgu.Username.ToLower();
                    if (username == name) // exact match check
                        return sgu;
                    int difference = name.Length - username.Length;
                    if (difference < 3)
                        return sgu;
                }
                return matchesFound[0];
            }
            catch (Exception m)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, "From: FindGuildUser");
            }
            return null;
        }

        //0 = nickname true, 1 = username true nickname false, 2 = either true but no matches found
        private static int NicknameOrNot(SocketGuildUser sgu, string matchingName)
        {
            bool hasnickname = !string.IsNullOrEmpty(sgu.Nickname) || !string.IsNullOrWhiteSpace(sgu.Nickname);
            if (hasnickname)
            {
                bool nicknamematches = sgu.Nickname.ToLower().Contains(matchingName);
                if (nicknamematches)
                {
                    return 0;
                }
            }
            bool usernamematches = sgu.Username.ToLower().Contains(matchingName);
            if (usernamematches)
            {
                return 1;
            }
            return 2;
        }

        public static bool IsInUserData(SocketUser sock)
        {
            bool flag = Program.ExpPoints.Count == 0;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = false;
                foreach (UserData userData in Program.ExpPoints)
                {
                    bool flag3 = userData.UserID == sock.Id;
                    if (flag3)
                    {
                        flag2 = true;
                        break;
                    }
                }
                result = flag2;
            }
            return result;
        }

        public static UserData FindPerson(ulong UserID)
        {
            UserData result = new UserData(UserID);
            foreach (UserData userData in Program.ExpPoints)
            {
                bool flag = userData.UserID == UserID;
                if (flag)
                {
                    result = userData;
                    break;
                }
            }
            return result;
        }

        public static int ConvertTo24HourTime(this int f, string pmAm)
        {
            bool flag = pmAm.ToLower() == "pm";
            if (flag)
            {
                bool flag2 = f >= 1 && f < 12;
                if (flag2)
                {
                    return f + 12;
                }
            }
            else
            {
                bool flag3 = f >= 12 && f < 13;
                if (flag3)
                {
                    return f - 12;
                }
            }
            return f;
        }

        public enum LogType
        {
            Debug,
            ErrorLog,
            CustomMessage,
            GenericMessage
        }
    }
}
